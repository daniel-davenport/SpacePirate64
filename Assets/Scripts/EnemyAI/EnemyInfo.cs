using UnityEngine;


[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Objects/EnemyInfo")]
public class EnemyInfo : ScriptableObject
{
    public string enemyName;
    public string enemyDisplayName;
    public int enemyHealth;
    public int enemyMaxHealth;

    // how much damage their projectile deals
    public int projectileDamage;

    // how much scrap they drop on kill
    public int scrapDropped;

    public GameObject enemyModel;


    
    
}
