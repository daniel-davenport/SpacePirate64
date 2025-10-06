using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class EnemyInit : MonoBehaviour
{
    // initialize the enemy, get their model and ai
    private StateMachine stateMachine;
    public string enemyName; // used to define its information
    public EnemyInfo enemyInfo;

    // used to get the enemy AI information from its ScriptableObject
    public string enemyAIName;
    public Type scriptType;

    [Header("References")]
    public GameObject playerShip;
    public SpawnDirector spawnDirector;
    public ScoreHandler scoreHandler;

    [Header("Stats")]
    public int enemyHealth;
    public int enemyMaxHealth;
    public int projectileDamage;
    private bool tookDamage; // tracking if they were killed by taking damage or not


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // load enemy information (model, hp, etc)
        //StartCoroutine(DelayEnemyLoad(3));
        LoadEnemyData();

        // get their state machine
        stateMachine = GetComponent<StateMachine>();

        // dynamically adding the enemy ai based on the defined script in the ScriptableObject
        enemyAIName = enemyInfo.enemyAI.name;
        
        scriptType = Type.GetType(enemyAIName);

        //print(enemyAIName + " " + scriptType);

        if (scriptType != null)
        {
            // add their AI to the enemy gameobject
            gameObject.AddComponent(scriptType);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        // an enemy was destroyed, increase intensity
        if (spawnDirector != null)
            spawnDirector.ChangeIntensity(true, 1);

        // if the player killed them then increase their score + drop loot
        if (tookDamage == true)
        {
            // gain score for netting a kill
            scoreHandler.ChangePlayerScore("kill");
        }

    }

    // loads the enemy's data based on the enemyName provided above.
    private void LoadEnemyData()
    {
        enemyInfo = Resources.Load<EnemyInfo>("ScriptableObjects/Enemies/" + enemyName + "Info");

        if (enemyInfo == null)
        {
            Debug.LogWarning("enemy type " + enemyName + " not found.");
        }

        GameObject enemyModel = Instantiate(enemyInfo.enemyModel, transform.position, Quaternion.identity, transform);
        enemyModel.layer = LayerMask.NameToLayer("Enemy");

        // making a collider
        BoxCollider enemyCollider = enemyModel.AddComponent<BoxCollider>();

        // add a kinematic rigidbody
        Rigidbody rb = enemyModel.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // adding a collision script
        enemyModel.AddComponent<EnemyCollision>();

        // health + stats
        enemyHealth = enemyInfo.enemyHealth;
        enemyMaxHealth = enemyInfo.enemyMaxHealth;
        projectileDamage = enemyInfo.projectileDamage;

        //print("enemy " + enemyName + " loaded successfully.");
    }

    // handles taking damage outside of script
    public void TakeDamage(int damage, string style = null)
    {
        // enemies don't really have iframes so simply deal damage
        tookDamage = true;
        enemyHealth -= damage;
        //print("enemy take damage");

        if (enemyHealth <= 0)
        {
            // destroy the enemy and drop scrap
            Destroy(gameObject);
        }

        if (style != null)
        {
            // gain score for whatever was defined
            scoreHandler.ChangePlayerScore(style);
        }

    }

    IEnumerator DelayEnemyLoad(float delay)
    {
        //print("enemy info will load in " + delay + " seconds");
        yield return new WaitForSeconds(delay);
        //print("loading enemy data now.");

        LoadEnemyData();
    }
}
