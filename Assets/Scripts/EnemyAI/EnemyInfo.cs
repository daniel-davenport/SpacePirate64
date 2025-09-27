using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Objects/EnemyInfo")]
public class EnemyInfo : ScriptableObject
{
    public string enemyName;
    public int enemyHealth;
    public int enemyMaxHealth;

    public int projectileDamage;

    public GameObject enemyModel;
    public MonoScript enemyAI;


    
    
}
