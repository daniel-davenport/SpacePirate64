using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class ProjectileInfo : MonoBehaviour
{
    public Transform projectileHolder;
    public GameObject projectileOwner;
    public int projectileDamage;
    public bool parried;
    public bool interceptable;

    // explosion settings
    public float explosionRadius;
    public int explosionDamage;

    private GameObject explosionRef;
    private GameObject lockedOnEnemy = null;

    private void Start()
    {
        explosionRef = Resources.Load<GameObject>("Particles/explosion");
    }

    // destroying the projectile if it hits a wall or other object
    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        // colliding with an obstacle
        if (LayerMask.LayerToName(otherLayer) == "Obstacle")
        {
            Destroy(gameObject);
        }

        if (interceptable == true)
        {
            if (LayerMask.LayerToName(otherLayer) == "PlayerProjectile")
            {
                print("intercepted");
                Destroy(gameObject);
            }
        }

    }

    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded == false)
            return;

        if (interceptable == true)
        {
            // make an explosion effect i think

        }


        // make an explosion effect, plus explosion
        if (explosionRadius > 0)
        {
            
            //LayerMask enemyMask = (1 << LayerMask.NameToLayer("Enemy"));
            int enemyMask = LayerMask.NameToLayer("Enemy");

            Collider[] allHit = Physics.OverlapSphere(transform.position, explosionRadius, enemyMask);

            // deal damage to them all
            foreach (Collider hitCollider in allHit)
            {
                GameObject enemy = hitCollider.gameObject;
                print(allHit.Length);
                EnemyInit enemyScript = enemy.GetComponent<EnemyInit>();

                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(explosionDamage);
                }

            }


            // make an explosion effect
            GameObject spawnedParticle = Instantiate(explosionRef);
            spawnedParticle.transform.localScale = new Vector3(explosionRadius, explosionRadius, explosionRadius);
            spawnedParticle.transform.position = transform.position;

        }

    }

    // destroy all projectiles when a level ends
    public void LevelEnded()
    {
        //print("level ended projectile destroyed");
        Destroy(gameObject);
    }

    // homes in on whatever enemy is locked on
    public void StartHoming(Transform target, float projectileSpeed)
    {
        StartCoroutine(Homing(target, projectileSpeed));
    }


    private IEnumerator Homing(Transform target, float projectileSpeed)
    {
        while (gameObject != null && parried == false)
        {
            // resetting the rigidbody's speed so there isnt speedups
            Rigidbody rb = GetComponent<Rigidbody>();
            
            if (rb != null && rb.isKinematic == false)
                rb.linearVelocity = Vector3.zero;

            // todo:
            // change this back to just use a transform
            // add aoe explosion to missiles

            transform.LookAt(target);

            // getting the direction between them
            Vector3 direction = transform.forward;

            if (target != null)
                direction = (target.position - transform.position);

            // note: this might work but might cause issues later, maybe have some other way of testing this?
            transform.position += (direction.normalized * projectileSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }

    }


    // searching via shapecast to find an enemy to lock on
    public void StartFindEnemy(float projectileSpeed)
    {
        StartCoroutine(FindEnemy(projectileSpeed));
    }

    // constantly checks in front to find an enemy to lock onto
    public IEnumerator FindEnemy(float projectileSpeed)
    {

        while (lockedOnEnemy == null && gameObject != null)
        {

            float lockOnRadius = 10f;
            
            RaycastHit hit;
            LayerMask enemyMask = (1 << LayerMask.NameToLayer("Enemy")); 

            Vector3 startPos = transform.position;
            Vector3 direction = transform.forward;

            //Debug.DrawRay(origin, direction * 100f, Color.yellow, 100f);

            // the documentation literally says it accepts an integer why the hell does it not work when you pass an integer
            if (Physics.SphereCast(startPos, lockOnRadius, direction, out hit, 100f, enemyMask))
            {
                // lock onto the enemy 
                lockedOnEnemy = hit.transform.gameObject;
                break;
            }

            yield return new WaitForEndOfFrame();
        }


        if (gameObject != null)
        {
            print(lockedOnEnemy);
            StartHoming(lockedOnEnemy.transform, projectileSpeed);
        }

    }


}
