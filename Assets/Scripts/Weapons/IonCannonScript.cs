using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class IonCannonScript : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponModel;
    public GameObject laserProjectile;
    public GameObject missileProjectile;
    public GameObject chargedShotProjectile;
    public GameObject playerShip;
    public GameObject chargeVisual;

    [Header("Stats")]
    public int weaponSlot;
    public int weaponLevel;

    private int currentLevel;
    public int projectileDamage;
    public float projectileSpeed;
    public int chargedDamage;

    public WeaponInfo weaponInfo;
    public WeaponHandler weaponHandler;
    public Transform firePoint;
    public float projectileLifetime = 3f;

    private int bounces = 1;
    private int chainRadius = 5;

    /*

    weapon behavior:
    - Has no basic attack
    - Charged attack fires a homing orb that once it hits, scans for nearby enemies then chains the orb to them
    - higher levels increases the amount of times it chains to another enemy

    weapon levels:
    1 - 
    2 - 
    3 - 

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
        missileProjectile = Resources.Load<GameObject>("Projectiles/missileProjectile");
        chargedShotProjectile = Resources.Load<GameObject>("Projectiles/projectileSphere");
        playerShip = transform.gameObject;

        // setting the damage vars
        projectileDamage = weaponInfo.weaponDamage;
        chargedDamage = weaponInfo.chargedDamage;
        projectileSpeed = weaponInfo.projectileSpeed;

        // setting the charge visual (optional)
        chargeVisual = chargedShotProjectile;
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

        // increasing its projspeed based on level
        int increase = (level - 1) * 15;
        float projectileSpeed = weaponInfo.projectileSpeed + increase;

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
        GameObject firedLaser = Instantiate(missileProjectile, firePoint.position, Quaternion.identity);
        
        // changing the layer
        firedLaser.layer = LayerMask.NameToLayer("PlayerProjectile"); // note: leaving it like this means the first enemy hit will take more damage which is fine

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


    private void ChainAttack(GameObject projectile, int bounces, GameObject lockedOnEnemy)
    {
        print("begin chain");

        if (lockedOnEnemy != null)
        {
            GameObject enemy = lockedOnEnemy;
            //LayerMask enemyMask = (1 << LayerMask.NameToLayer("Enemy"));
            int enemyMask = LayerMask.NameToLayer("Enemy");
            List<GameObject> alreadyHitEnemies = new List<GameObject>(); // can't hit the same enemy twice

            // do an overlap sphere on the enemy based on the number of bounces
            for (int i = 0; i < bounces; i++)
            {
                //print(transform.position);
                Collider[] allHit = Physics.OverlapSphere(enemy.transform.position, chainRadius);
                print(allHit.Length);

                // nothing else to chain to
                if (allHit.Length > 0)
                {
                    // deal damage to whoever comes first
                    foreach (Collider hitCollider in allHit)
                    {
                        // hit the first enemy that hasn't been hit already
                        GameObject hitEnemy = hitCollider.gameObject;

                        if (hitEnemy != null && !alreadyHitEnemies.Contains(hitEnemy))
                        {
                            // add them to the list
                            alreadyHitEnemies.Add(hitEnemy);

                            // get their script to hit them
                            EnemyCollision enemyCollisionScript = enemy.GetComponent<EnemyCollision>();

                            if (enemyCollisionScript != null)
                            {
                                // enemyInit should be the parent
                                EnemyInit enemyScript = enemy.transform.parent.GetComponent<EnemyInit>();

                                if (enemyScript != null)
                                    enemyScript.TakeDamage(projectileDamage);

                            }

                            // break and chain again, setting the new enemy as the starting point
                            print("chaining to: " + hitEnemy.name);
                            enemy = hitEnemy;
                            break;

                        }
                    }



                }
                else
                {
                    print("no targets!");
                    return;
                }



            }



        }

    }

    private IEnumerator StartChainAttack(GameObject projectile, int bounces, GameObject lockedOnEnemy)
    {
        // wait until the projectile is destroyed (it hit something, because it can only fire if locked onto an enemy)
        yield return new WaitUntil(() => projectile.IsDestroyed() == true);

        print("destroyed");

        yield return true;
    }

    // shoot the weapon based on the weapon's level
    public void FireWeapon(int slot, bool isChargedShot)
    {
        if (isChargedShot)
        {

            // check if there's an enemy 
            if (weaponHandler.lockedOnEnemies[slot] != null)
            {
                // create the shot, can only fire if there's a lock.
                GameObject firedShot = ChargedShot();

                GameObject enemy = weaponHandler.lockedOnEnemies[slot];

                // moving it towards the enemy using its rigidbody
                StartCoroutine(HomingProjectile(firedShot, enemy.transform));

                // start the chain lightning
                StartCoroutine(StartChainAttack(firedShot, bounces, enemy));

            }


        }

        // TODO:
        // the rest of the weapon ngl.
        // paste the stuff from the other weapon handlers cuz the missile launcher was really heavily changed


    }

}
