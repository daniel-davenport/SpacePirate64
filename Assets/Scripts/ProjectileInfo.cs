using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class ProjectileInfo : MonoBehaviour
{

    public ScoreHandler scoreHandler; // ref to score intercepts
    public Transform projectileHolder;
    public GameObject projectileOwner;
    public SFXScript sfxScript;
    public int projectileDamage;
    public bool parried;
    public bool interceptable;

    // dangerSettings
    private PlayerController pc;

    // explosion settings
    public float explosionRadius;
    public int explosionDamage;

    private GameObject explosionRef;
    private GameObject lockedOnEnemy = null;

    private void Start()
    {
        explosionRef = Resources.Load<GameObject>("Particles/explosion");

        // changing laserprojectile hitboxes based on ownership (player projectiles have bigger hitboxes to be more forgiving)
        if (gameObject.name == "laserProjectile")
        {
            if (gameObject.layer == LayerMask.NameToLayer("EnemyProjectile"))
            {
                gameObject.GetComponent<MeshCollider>().enabled = true;
                gameObject.GetComponent<CapsuleCollider>().enabled = false;
            }
            else
            {
                // it's a player projectile
                gameObject.GetComponent<MeshCollider>().enabled = false;
                gameObject.GetComponent<CapsuleCollider>().enabled = true;
            }
        }

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
                Destroy(gameObject);

                // score for intercepting
                if (scoreHandler != null)
                    scoreHandler.ChangePlayerScore("projectileIntercept");

                // play SFX
                if (sfxScript != null)
                {
                    sfxScript.PlaySFX("Intercept", true);
                }
                else
                {
                    if (pc != null)
                    {
                        pc.sfxScript.PlaySFX("Intercept", true);
                    }
                }
                
            }
        }

    }

    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded == false)
            return;

        if (interceptable == true && gameObject.layer != LayerMask.NameToLayer("PlayerProjectile"))
        {
            // make an explosion effect i think
            GameObject spawnedParticle = Instantiate(explosionRef);
            spawnedParticle.transform.localScale = new Vector3(2, 2, 2);
            spawnedParticle.transform.position = transform.position;
        }

        // tell the PC that it's not in danger anymore
        if (pc != null)
        {
            pc.inDanger = false;
        }


        // make an explosion effect, plus explosion
        if (explosionRadius > 0)
        {
            
            //LayerMask enemyMask = (1 << LayerMask.NameToLayer("Enemy"));
            int enemyMask = LayerMask.NameToLayer("Enemy");

            //print(transform.position);

            Collider[] allHit = Physics.OverlapSphere(transform.position, explosionRadius);

            // deal damage to them all
            foreach (Collider hitCollider in allHit)
            {
                GameObject enemy = hitCollider.gameObject;
                EnemyCollision enemyCollisionScript = enemy.GetComponent<EnemyCollision>();

                if (enemyCollisionScript != null)
                {
                    // enemyInit should be the parent
                    EnemyInit enemyScript = enemy.transform.parent.GetComponent<EnemyInit>();

                    if (enemyScript != null)
                        enemyScript.TakeDamage(explosionDamage);
                }

            }


            // make an explosion effect
            GameObject spawnedParticle = Instantiate(explosionRef);
            spawnedParticle.transform.localScale = new Vector3(explosionRadius, explosionRadius, explosionRadius);
            spawnedParticle.transform.position = transform.position;

            // play SFX
            if (sfxScript != null)
            {
                sfxScript.PlaySFX("MissileLauncherExplosion", true);
            }
            
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

            // TODO:
            // this works for missiles homing, add into the playercontroller a bool called 'InDanger'
            // if that bool is true, a spinning orange circle appears around the player
            // the bottom of their screen (above bombs) will flash the word "! MISSILE !"
            if (target != null && target.name.Contains("Player"))
            {
                pc = target.parent.GetComponent<PlayerController>();

                if (pc != null)
                {
                    pc.inDanger = true;
                }

            }
                

            transform.LookAt(target);

            // getting the direction between them
            Vector3 direction = transform.forward;

            if (target != null)
                direction = (target.position - transform.position);

            // note: this might work but might cause issues later, maybe have some other way of testing this?
            transform.position += (direction.normalized * projectileSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }


        if (pc != null)
        {
            pc.inDanger = false;
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
            //print(lockedOnEnemy);
            StartHoming(lockedOnEnemy.transform, projectileSpeed);
        }

    }


}
