using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    // initialize the player's weapons
    [Header("References")]
    public GameObject playerShip;
    public GameObject leftWeaponModel;
    public GameObject rightWeaponModel;
    public GameObject[] weaponModels = new GameObject[2];

    [Header("Stats")]
    public WeaponInfo[] weaponInfoArr = new WeaponInfo[2];
    public Component[] weaponComponents = new Component[2];
    public string[] prevWeaponScripts = new string[2];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // getting the weapon models
        weaponModels[0] = leftWeaponModel;
        weaponModels[1] = rightWeaponModel;

        // get their weapons
        LoadWeaponData();



    }

    // Update is called once per frame
    void Update()
    {
        
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

                //Renderer objectRenderer = weaponModels[i].GetComponent<Renderer>();
                //objectRenderer.material.color = weaponScript.;

            }



        }

    }

}
