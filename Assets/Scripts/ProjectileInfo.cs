using System.Collections;
using UnityEngine;
using UnityEngine.ProBuilder;

public class ProjectileInfo : MonoBehaviour
{
    public Transform projectileHolder;
    public GameObject projectileOwner;
    public int projectileDamage;
    public bool parried;
    public bool interceptable;

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
        if (interceptable == true)
        {
            // make an explosion effect i think

        }
    }

    // destroy all projectiles when a level ends
    public void LevelEnded()
    {
        //print("level ended projectile destroyed");
        Destroy(gameObject);
    }


    public void StartHoming(Transform target, float projectileSpeed)
    {
        StartCoroutine(Homing(target, projectileSpeed));
    }


    private IEnumerator Homing(Transform target, float projectileSpeed)
    {
        while (transform.parent != null)
        {
            transform.LookAt(target);

            // getting the direction between them
            Vector3 direction = (target.position - transform.position);

            // note: this might work but might cause issues later, maybe have some other way of testing this?
            transform.position += (direction.normalized * projectileSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }

    }


}
