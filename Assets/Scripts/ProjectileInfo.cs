using UnityEngine;

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
}
