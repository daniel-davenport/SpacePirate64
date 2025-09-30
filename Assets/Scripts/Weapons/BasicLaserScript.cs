using UnityEngine;

public class BasicLaserScript : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponModel;
    public GameObject laserProjectile;
    public GameObject playerShip;

    [Header("Stats")]
    public int weaponSlot;
    public WeaponInfo weaponInfo;
    public WeaponHandler weaponHandler;
    public Transform firePoint;
    public float projectileLifetime = 4f;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponHandler = GetComponent<WeaponHandler>();

        // you should have the weapon slot thanks to reflection
        weaponModel = weaponHandler.weaponModels[weaponSlot];
        weaponInfo = weaponHandler.weaponInfoArr[weaponSlot];
        firePoint = weaponModel.transform.GetChild(0).transform;

        // setting the weapon's color
        Renderer objectRenderer = weaponModel.GetComponent<Renderer>();
        objectRenderer.material.color = weaponInfo.weaponColor;

        // loading the laser projectile
        laserProjectile = Resources.Load<GameObject>("Projectiles/laserProjectile");
        playerShip = transform.gameObject;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private GameObject CreateProjectile()
    {
        // fire a laser in front
        GameObject firedLaser = Instantiate(laserProjectile, firePoint.position, Quaternion.identity);
        
        // changing the layer
        firedLaser.layer = LayerMask.NameToLayer("PlayerProjectile");

        //firedLaser.transform.SetParent(playerShip.transform
        Quaternion targetRot = Quaternion.LookRotation(playerShip.transform.forward, Vector3.up);
        firedLaser.transform.rotation = targetRot;

        // setting the owner + damage
        firedLaser.GetComponent<ProjectileInfo>().projectileOwner = transform.gameObject;
        firedLaser.GetComponent<ProjectileInfo>().projectileDamage = weaponInfo.weaponDamage;

        // setting its lifetime
        Destroy(firedLaser, projectileLifetime);

        // change its color
        Renderer objectRenderer = firedLaser.gameObject.GetComponent<Renderer>();
        objectRenderer.material.color = weaponInfo.projectileColor;

        return firedLaser;
    }

    public void FireWeapon(int slot, bool isChargedShot)
    {

        if (isChargedShot)
        {
            print("slot " + slot + " shooting charged shot");

            // charged shot behavior:
            // while charging, continuously raycast to lock on to an enemy in front of you in a big cone or cylinder
            // this can be done in the WeaponHandler having an array called "LockedOnTarget" or something that holds 2 enemies, one for each slot
            // once you release the charged shot, check weaponhandler for a lock on, if there's a lockon then tween it to the enemy's position
            // otherwise, just shoot it forward and let it do its thing

            // charged shot deals increased damage and locks on to the first enemy you highlight

            // lockon indicator is a spinning diamond inside of a square?
        }
        else
        {
            print("slot " + slot + " shooting normal shot");

            // create a laser projectile at the fire point and shoot it in the direction the player is facing
            GameObject firedShot = CreateProjectile();

            // moving it towards the player
            Rigidbody rb = firedShot.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddForce(firedShot.transform.forward * weaponInfo.projectileSpeed, ForceMode.Impulse);


        }


    }

}
