using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/WeaponInfo")]
public class WeaponInfo : ScriptableObject
{
    public string weaponDisplayName;
    public int weaponDamage;
    public float projectileSpeed;

    public bool hasChargedShot;
    public float chargeTime; // Tracks the current charge level
    public float maxChargeTime; // The charge time needed to fire a charged shot

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

