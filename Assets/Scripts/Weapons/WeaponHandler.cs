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

    [Header("Stats")]
    public WeaponInfo[] weaponInfoArr = new WeaponInfo[2];
    public Component[] weaponComponents = new Component[2];
    public Component[] prevWeaponScripts = new Component[2];
    private MethodInfo[] weaponMethods = new MethodInfo[2];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // getting player controller
        playerController = GetComponent<PlayerController>(); 

        // getting the weapon models
        weaponModels[0] = playerController.leftWeaponModel;
        weaponModels[1] = playerController.rightWeaponModel;

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
            object[] args = { slot, isChargedShot };
            weaponMethods[slot].Invoke(weaponComponents[slot], args);
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

                // change the PlayerController's cooldowns to point to here, and have the cooldowns update based on the WeaponInfo


            }



        }

    }



}
