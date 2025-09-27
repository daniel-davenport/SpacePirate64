using DG.Tweening;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{

    [Header("References")]
    public GameObject enemyHolder;
    public EnemyInit enemyInitScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyHolder = transform.parent.gameObject;
        enemyInitScript = enemyHolder.GetComponent<EnemyInit>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // checking the layer
        int otherLayer = other.gameObject.layer;

        //

        // colliding with an obstacle
        if (LayerMask.LayerToName(otherLayer) == "Obstacle")
        {

            print("collision");
            // deal heavy damage
            enemyInitScript.TakeDamage(5);

        }
        else if (LayerMask.LayerToName(otherLayer) == "EnemyProjectile" || LayerMask.LayerToName(otherLayer) == "PlayerProjectile")
        {
            // get the projectile info + damage
            GameObject projectile = other.gameObject;
            GameObject projOwner = other.gameObject.GetComponent<ProjectileInfo>().projectileOwner;
            int projDamage = other.gameObject.GetComponent<ProjectileInfo>().projectileDamage;

            // if it's an enemy projectile and they're not the owner, friendly fire damage

            if (LayerMask.LayerToName(otherLayer) == "EnemyProjectile" && projOwner != enemyHolder)
            {
                // came from another enemy, friendly fire bonus
                enemyInitScript.TakeDamage(projDamage);
                Destroy(projectile);

            }
            else if (LayerMask.LayerToName(otherLayer) == "PlayerProjectile")
            {
                // get the owner, if the owner is the same as this then also friendly fire bonus
                if (projOwner == enemyHolder)
                {
                    // friendly fire bonus
                }

                enemyInitScript.TakeDamage(projDamage);
                Destroy(projectile);

            }


            



        }

    }



}
