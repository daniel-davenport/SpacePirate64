using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class GatlingLaserScript : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponModel;
    public GameObject laserProjectile;
    public GameObject chargedShotProjectile;
    public GameObject playerShip;
    public GameObject chargeVisual;

    [Header("Stats")]
    public int weaponSlot;
    public int weaponLevel;

    private int currentLevel;
    public int projectileDamage;
    public int chargedDamage;

    public WeaponInfo weaponInfo;
    public WeaponHandler weaponHandler;
    public Transform firePoint;
    public float projectileLifetime = 3f;

    /*

    weapon behavior:
    - fires a smaller projectile with increased damage compared to the basic laser
    - lacks a charged shot, instead holding down attack will constantly fire lasers

    weapon levels:
    1 - Cooldown shortened, damage increased
    2 - Projectile cooldown lowered slightly more, projectile size increased
    3 - Cooldown near instant, damage increased, projectile size increased

     */



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
        chargedShotProjectile = Resources.Load<GameObject>("Projectiles/projectileSphere");
        playerShip = transform.gameObject;

        // setting the damage vars
        projectileDamage = weaponInfo.weaponDamage;
        chargedDamage = weaponInfo.chargedDamage;

        // setting the charge visual (optional)
        chargeVisual = null; // optional
        weaponHandler.SetChargeVisual(weaponSlot, chargeVisual);

    }

    // Update is called once per frame
    void Update()
    {
        if (weaponHandler.weaponLevels[weaponSlot] != currentLevel)
            LevelChange(weaponHandler.weaponLevels[weaponSlot]);
    }

    // used to set the damage and stuff
    private void LevelChange(int level)
    {
        currentLevel = level;

        // update the WeaponHandler's cooldown and charge time levels here
        float firingSpeed = weaponInfo.firingSpeed / level;// reducing the firing speed

        // rapid fire once fully levelled up
        if (level > 2)
            firingSpeed = 0.1f;

        // damage increases linearly
        projectileDamage = weaponInfo.weaponDamage * currentLevel;

        weaponHandler.firingSpeeds[weaponSlot] = firingSpeed;
    }

    private GameObject ChargedShot()
    {
        // fire a charged shot in front
        GameObject chargedShot = Instantiate(chargedShotProjectile, firePoint.position, Quaternion.identity);

        // changing the layer
        chargedShot.layer = LayerMask.NameToLayer("PlayerProjectile");

        //firedLaser.transform.SetParent(playerShip.transform
        Quaternion targetRot = Quaternion.LookRotation(playerShip.transform.forward, Vector3.up);
        chargedShot.transform.rotation = targetRot;

        // setting the owner + damage
        chargedShot.GetComponent<ProjectileInfo>().projectileOwner = transform.gameObject;
        chargedShot.GetComponent<ProjectileInfo>().projectileDamage = chargedDamage;

        // todo:
        // delete the projectile when it reaches its destination

        // setting its lifetime
        Destroy(chargedShot, 10);

        // change its color
        // Renderer objectRenderer = firedLaser.gameObject.GetComponent<Renderer>();
        // objectRenderer.material.color = weaponInfo.projectileColor;

        return chargedShot;
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
        firedLaser.GetComponent<ProjectileInfo>().projectileDamage = projectileDamage;

        // setting its lifetime
        Destroy(firedLaser, projectileLifetime);

        // change its color
        Renderer objectRenderer = firedLaser.gameObject.GetComponent<Renderer>();
        objectRenderer.material.color = weaponInfo.projectileColor;

        return firedLaser;
    }


    // constantly turns the projectile to look at its target and moves it there
    private IEnumerator HomingProjectile(GameObject projectile, Transform target)
    {
        while (target != null && projectile != null)
        {
            projectile.transform.LookAt(target);

            // getting the direction between them
            Vector3 direction = (target.position - projectile.transform.position);

            // note: this might work but might cause issues later, maybe have some other way of testing this?
            projectile.transform.position += (direction.normalized * weaponInfo.chargedSpeed * Time.deltaTime);
            
            yield return new WaitForFixedUpdate();
        }

        if (target == null && projectile != null)
        {
            // enemy was killed, just keep going in that direction
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddForce(projectile.transform.forward * weaponInfo.projectileSpeed, ForceMode.Impulse);
        }

    }


    // shoot the weapon based on the weapon's level
    public void FireWeapon(int slot, bool isChargedShot)
    {

        if (isChargedShot)
        {

            // create the shot
            GameObject firedShot = ChargedShot();

            // check if there's an enemy 
            if (weaponHandler.lockedOnEnemies[slot] != null)
            {
                GameObject enemy = weaponHandler.lockedOnEnemies[slot];

                // moving it towards the enemy using its rigidbody
                StartCoroutine(HomingProjectile(firedShot, enemy.transform));

            }
            else
            {
                // just move the projectile in the forward direction
                Rigidbody rb = firedShot.GetComponent<Rigidbody>();

                if (rb != null)
                    rb.AddForce(firedShot.transform.forward * weaponInfo.projectileSpeed, ForceMode.Impulse);

            }


            

        }
        else
        {
            //print("slot " + slot + " shooting normal shot");

            // create a laser projectile at the fire point and shoot it in the direction the player is facing
            GameObject firedShot = CreateProjectile();

            // moving it forwards
            Rigidbody rb = firedShot.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddForce(firedShot.transform.forward * weaponInfo.projectileSpeed, ForceMode.Impulse);


        }


    }

}
