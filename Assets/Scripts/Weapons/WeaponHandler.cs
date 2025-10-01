using DG.Tweening;
using System;
using System.Collections;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    // initialize the player's weapons
    [Header("References")]
    public GameObject playerShip;
    public PlayerController playerController;
    public GameObject[] weaponModels = new GameObject[2];
    private GameObject lockOnIndicator;
    private GameObject[] indicators = new GameObject[2];

    [Header("Stats")]
    public WeaponInfo[] weaponInfoArr = new WeaponInfo[2];
    public Component[] weaponComponents = new Component[2];
    public Component[] prevWeaponScripts = new Component[2];
    private MethodInfo[] weaponMethods = new MethodInfo[2];
    public GameObject[] lockedOnEnemies = new GameObject[2];


    private float lockOnRadius = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // getting player controller
        playerController = GetComponent<PlayerController>(); 

        // getting the weapon models
        weaponModels[0] = playerController.leftWeaponModel;
        weaponModels[1] = playerController.rightWeaponModel;

        lockOnIndicator = Resources.Load<GameObject>("Projectiles/LockOnIndicator");

        // get their weapons
        LoadWeaponData();



    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // calling the script's fire methods via reflection
    public void FireWeapon(int slot, bool isChargedShot)
    {
        //print(slot + " " + isChargedShot);
        //print(weaponMethods[slot]);
        if (weaponMethods[slot] != null)
        {
            // invoke the fire function
            object[] args = { slot, isChargedShot };
            weaponMethods[slot].Invoke(weaponComponents[slot], args);

            // clear the locked on enemy
            lockedOnEnemies[slot] = null;
        }
    }


    // called to load the player's weapon functions, should be called in places like Shops where the player can swap out their weapons out of sight
    // shops will change the weaponInfoArr to update it with the new ScriptableObjects.
    public void LoadWeaponData()
    {
        // update the player's weapons based on the weaponNames array
        // note: this for loop does it twice, once per weapon slot.
        for (int i = 0; i <= 1; i++)
        {
            /*
            // remove the previous weapon scripts
            // note: take some consideration here, it should only be removed if you're replacing the slot.
            Component prevScript = gameObject.GetComponent(prevWeaponScripts[i]);
            if (prevScript)
                Destroy(prevScript);
            */

            // dynamically adding weapon scripts based on the defined script in the ScriptableObject
            string weaponName = weaponInfoArr[i].name + "Script";

            Type scriptType = Type.GetType(weaponName);

            if (scriptType != null)
            {
                // add their weapon script 
                Component weaponScript = gameObject.AddComponent(scriptType);
                weaponComponents[i] = weaponScript;

                // setting the weapon slot (have to do it using reflection since we don't know the type)
                FieldInfo fi = scriptType.GetField("weaponSlot");

                if (fi != null)
                    fi.SetValue(weaponScript, i);

                // getting the weapon script's fire method
                weaponMethods[i] = scriptType.GetMethod("FireWeapon");

                // PlayerController's cooldowns point here
                playerController.maxChargeTimes[i] = weaponInfoArr[i].maxChargeTime;

            }



        }

    }



    // create an indicator on the target
    private void ShowLockOn(int slot, GameObject target)
    {
        indicators[slot] = Instantiate(lockOnIndicator, target.transform.parent);

        Transform reticleRef = lockOnIndicator.transform.GetChild(0);
        GameObject reticle = indicators[slot].transform.GetChild(0).gameObject;

        Vector3 baseRotation = reticleRef.transform.rotation.eulerAngles;
        reticle.transform.eulerAngles = new Vector3(baseRotation.x * 3, baseRotation.y, baseRotation.z);

        Vector3 baseScale = reticle.transform.localScale;
        Vector3 targetRotation = baseRotation;

        // make it big
        reticle.transform.localScale *= 3;
        //reticle.transform.Rotate(new Vector3(1, 0, 0) * 330, Space.Self);

        reticle.transform.DORotate(targetRotation, 0.25f).SetEase(Ease.OutQuad);
        reticle.transform.DOScale(baseScale, 0.25f).SetEase(Ease.OutQuad);

        StartCoroutine(RemoveIndicator(slot));
    }


    IEnumerator RemoveIndicator(int slot)
    {

        // waits for the lock on to remove, then destroy it after X amount of time
        yield return new WaitUntil(() => lockedOnEnemies[slot] == null);

        // waits for a bit for the projectile to travel
        yield return new WaitForSeconds(1);

        // destroy the indicator
        Destroy(indicators[slot]);
        indicators[slot] = null;

    }


    // later make a MultiLockOn function that instead of locking onto a slot, instead locks on a number, then repeated the lockon
    // without counting the same enemy (raycasting again behind them)
    // this'll be useful for some missile weaposn that can fire in salvos


    // shapecast in front of the player, if it collides with an enemy then add it to a locked on array, then stop
    public void LockOn(int slot)
    {
        // if there's no lock on charging, return
        // if there's already a locked on enemy, return
        if (!weaponInfoArr[slot].chargedLocksOn || lockedOnEnemies[slot] != null)
            return;

        RaycastHit hit;
        int layerMask = LayerMask.NameToLayer("Enemy"); // for some reason this ignores that
        LayerMask enemyMask = (1 << LayerMask.NameToLayer("Enemy")); // BUT IT FUCKING ACCEPTS THIS PIECE OF SHIT
        //print(layerMask.value);

        Vector3 origin = playerShip.transform.position;
        Vector3 direction = playerShip.transform.forward;

        //Debug.DrawRay(origin, direction * 100f, Color.yellow, 100f);

        // the documentation literally says it accepts an integer why the hell does it not work when you pass an integer
        if (Physics.SphereCast(origin, lockOnRadius, direction, out hit, 100f, enemyMask))
        {
            // lock onto the enemy 
            lockedOnEnemies[slot] = hit.transform.gameObject;

            // show an indicator over them
            ShowLockOn(slot, lockedOnEnemies[slot]);

        }

    }

}
