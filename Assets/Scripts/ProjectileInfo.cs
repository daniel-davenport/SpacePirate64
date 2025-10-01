using UnityEngine;

public class ProjectileInfo : MonoBehaviour
{
    public GameObject projectileOwner;
    public int projectileDamage;

    // destroying the projectile if it hits a wall or other object
    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        // colliding with an obstacle
        if (LayerMask.LayerToName(otherLayer) == "Obstacle")
        {
            Destroy(gameObject);
        }
    }
}
