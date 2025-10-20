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
    public Component[] equippedWeaponScripts = new Component[2];
    public String[] equippedWeaponNames = new String[2];
    private MethodInfo[] weaponMethods = new MethodInfo[2];
    public GameObject[] lockedOnEnemies = new GameObject[2];

    // stuff modified by weapon levels
    public float[] maxChargeTimes = new float[] { 1f, 1f }; // The charge time needed to fire a charged shot
    public float[] firingSpeeds = new float[2];

    // weapon levels
    public int[] weaponLevels = new int[2]; 
    public int[] weaponEXP = new int[2];

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

        weaponLevels[0] = 1;
        weaponLevels[1] = 1;

        // get their weapons
        LoadWeaponData();

        // testing weapon changing
        //StartCoroutine(TestWeaponSwap());

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

            // dynamically adding weapon scripts based on the name of the script assigned by the shop
            string weaponScriptName = weaponInfoArr[i].name + "Script";

            // check if this slot's weapon name matches the previous weapon name
            if (equippedWeaponNames[i] == weaponInfoArr[i].name)
            {
                // if it does then no need to do much of anything, go next.
                //print("same weapon already equipped");
                continue;
            }
            else 
            {
                // otherwise, remove the script so the new one can be added later
                Destroy(equippedWeaponScripts[i]);
            }
                
            Type scriptType = Type.GetType(weaponScriptName);

            if (scriptType != null)
            {
                // resetting the weapon level
                weaponLevels[i] = 1;

                // add their weapon script 
                Component weaponScript = gameObject.AddComponent(scriptType);
                weaponComponents[i] = weaponScript;

                // setting the weapon slot (have to do it using reflection since we don't know the type)
                FieldInfo fi = scriptType.GetField("weaponSlot");

                if (fi != null)
                    fi.SetValue(weaponScript, i);

                // getting the weapon script's fire method
                weaponMethods[i] = scriptType.GetMethod("FireWeapon");

                // setting default cooldowns/firing speeds, PlayerController looks at this
                maxChargeTimes[i] = weaponInfoArr[i].maxChargeTime;
                firingSpeeds[i] = weaponInfoArr[i].firingSpeed;

                // saving the weapon's name and script
                equippedWeaponNames[i] = weaponInfoArr[i].name;
                equippedWeaponScripts[i] = weaponScript;

            } else
            {
                Debug.Log("WARNING: WEAPON SCRIPT NOT FOUND - " +  weaponScriptName);
            }


        }

    }


    // testing function for checking weapon swap errors
    IEnumerator TestWeaponSwap()
    {
        yield return new WaitForSeconds(3);
        LoadWeaponData();
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

        reticle.transform.DORotate(targetRotation, 0.25f).SetEase(Ease.OutQuad).SetLink(reticle);
        reticle.transform.DOScale(baseScale, 0.25f).SetEase(Ease.OutQuad).SetLink(reticle);

        StartCoroutine(RemoveIndicator(slot));
    }


    IEnumerator RemoveIndicator(int slot)
    {

        // waits for the lock on to remove, then destroy it after X amount of time
        yield return new WaitUntil(() => lockedOnEnemies[slot] == null);

        // waits for a bit for the projectile to travel
        yield return new WaitForSeconds(0.33f);

        // destroy the indicator
        if (indicators[slot] != null)
        {
            Destroy(indicators[slot]);
            indicators[slot] = null;
        }

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


    // setting the charge visual in the PlayerController
    public void SetChargeVisual(int slot, GameObject visualRef)
    {
        GameObject chargeVisual = Instantiate(visualRef);

        // setting the visual to firepoint
        chargeVisual.transform.SetParent(weaponModels[slot].transform.GetChild(0).transform);

        // centering it
        chargeVisual.transform.localPosition = Vector3.zero;

        // destroying its collider and rb
        if (chargeVisual.GetComponent<Rigidbody>())
            Destroy(chargeVisual.GetComponent<Rigidbody>());

        if (chargeVisual.GetComponent<Collider>())
            Destroy(chargeVisual.GetComponent<Collider>());


        chargeVisual.transform.localScale = Vector3.zero;

        // setting it in the playercontroller
        playerController.chargeVisuals[slot] = chargeVisual;
    }

    // colliding with weapon EXP
    // note: they have to have IsTrigger set to true
    private void OnTriggerEnter(Collider other)
    {
        if (playerController.playerHealth <= 0)
            return;

        // checking the layer
        int otherLayer = other.gameObject.layer;

        // colliding with an obstacle
        if (LayerMask.LayerToName(otherLayer) == "Pickup")
        {
            // gaining exp
            if (other.gameObject.CompareTag("EXP"))
            {
                EXPScript expScript = other.transform.GetComponent<EXPScript>();
                // it's exp
                //print("picked up " + expScript.expValue +" exp");

                // add the value to both weapons
                for (int i = 0; i <= 1; i++)
                {
                    // stopping if you're max level
                    if (weaponLevels[i] >= weaponInfoArr[i].maxLevel)
                        continue;

                    weaponEXP[i] += expScript.expValue;

                    if (weaponEXP[i] >= weaponInfoArr[i].maxEXP)
                    {
                        // overflow
                        weaponEXP[i] -= weaponInfoArr[i].maxEXP;

                        // checking if there's another level above this one
                        if ((weaponLevels[i] + 1) <= weaponInfoArr[i].maxLevel)
                        {
                            weaponLevels[i] += 1;

                            print("slot " + i + " level up");

                            // show some effect


                        } else
                        {
                            // weapon max level
                            weaponEXP[i] = 0;

                            // plus whatever EXP goes to score for being level max


                        }

                    }

                }

                // destroying it
                Destroy(other.gameObject);
            }


        }
    }

}
