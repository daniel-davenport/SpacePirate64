using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/WeaponInfo")]
public class WeaponInfo : ScriptableObject
{
    // weapon settings
    public string weaponDisplayName;
    public int weaponDamage;
    public float projectileSpeed;
    public float firingSpeed; // how fast you can fire, lower number = faster firing speed
    public bool explodeOnDestroy; // if the projectile should explode when it's destroyed

    // explosion settings
    public float explosionRadius;
    public int explosionDamage;

    // weapon exp
    public int maxEXP; // how much exp you need to level it up
    public int maxLevel; // how many levels

    // charged shot settings
    public bool chargedLocksOn; // if your charged attack locks on
    public bool hasChargedShot;
    public float chargeTime; // Tracks the current charge level
    public float maxChargeTime; // The charge time needed to fire a charged shot
    public float chargedSpeed;
    public int chargedDamage;


    // TODO:
    // set in the projectile settings the weapon type to compare to later
    public enum WeaponType
    {
        Laser, // laser-based, uses the laser projectile
        Missile, // missile-based, fires missile projectile
        Beam, // solid beam, hitbox checking
        Physical, // bullets, flak, etc.
        Custom // custom projectile not defined above
    }

    public Color weaponColor;
    public Color projectileColor;


}

