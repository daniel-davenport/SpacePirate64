using UnityEngine;

public class BasicLaserScript : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponModel;

    [Header("Stats")]
    public int weaponSlot;
    public WeaponInfo weaponInfo;
    public WeaponHandler weaponHandler;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponHandler = GetComponent<WeaponHandler>();

        // you should have the weapon slot thanks to reflection
        weaponModel = weaponHandler.weaponModels[weaponSlot];
        weaponInfo = weaponHandler.weaponInfoArr[weaponSlot];

        Renderer objectRenderer = weaponModel.GetComponent<Renderer>();
        objectRenderer.material.color = weaponInfo.weaponColor;


    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
