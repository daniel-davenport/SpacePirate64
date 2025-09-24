using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private StateMachine stateMachine;
    public string enemyName; // used to define its information
    public EnemyInfo enemyInfo;

    public string enemyAIName;
    public Type scriptType;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateMachine = GetComponent<StateMachine>();


        //StartCoroutine(DelayEnemyLoad(3));
        LoadEnemyData();



        // dynamically adding the enemy ai based on the defined script in the scriptableobject
        enemyAIName = enemyInfo.enemyAI.name;
        
        scriptType = Type.GetType(enemyAIName);

        print(enemyAIName + " " + scriptType);

        if (scriptType != null)
        {
            gameObject.AddComponent(scriptType);
        }

        

        


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

        Instantiate(enemyInfo.enemyModel, transform.position, Quaternion.identity, transform);

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
