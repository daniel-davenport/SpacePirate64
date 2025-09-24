using System.Collections;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public string enemyName; // used to define its information
    public EnemyInfo enemyInfo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DelayEnemyLoad(3));
        //LoadEnemyData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // loads the enemy's data based on the enemyName provided above.
    private void LoadEnemyData()
    {
        enemyInfo = Resources.Load<EnemyInfo>("ScriptableObjects/" + enemyName + "Info");

        if (enemyInfo == null)
        {
            Debug.LogWarning("enemy type " + enemyName + " not found.");
        }

        Instantiate(enemyInfo.enemyModel, transform.position, Quaternion.identity);

        print("enemy " + enemyName + " loaded successfully.");
    }

    IEnumerator DelayEnemyLoad(float delay)
    {
        //print("enemy info will load in " + delay + " seconds");
        yield return new WaitForSeconds(delay);
        //print("loading enemy data now.");

        LoadEnemyData();
    }
}
