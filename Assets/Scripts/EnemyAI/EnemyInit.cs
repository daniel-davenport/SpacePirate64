using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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
    public ParticleHandler particleHandler;
    public PlayerController playerController;
    public GameObject expDrop;

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

        // dynamically adding the enemy ai based on the defined name in the ScriptableObject
        enemyAIName = enemyInfo.enemyName + "AI";
        
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
        // cause the enemy's ai to stop when the player dies
        if (playerController.playerHealth <= 0)
            stateMachine.currentState = StateMachine.EnemyState.Untarget;
    }

    private void OnDestroy()
    {
        // getting the enemy's location
        Vector3 enemyPos = transform.position;  

        // an enemy was destroyed, increase intensity
        if (spawnDirector != null)
            spawnDirector.ChangeIntensity(true, 1);

        // if the player killed them then increase their score + drop loot
        if (tookDamage == true)
        {
            // gain score for netting a kill
            scoreHandler.ChangePlayerScore("kill");

            // also drop exp.
            // exp drop is similar to cave story where all enemies can drop 1-3 experience crystals.
            int rng = Random.Range(1, 3);

            for (int i = 0; i < rng; i++)
            {
                // create an exp drop and give it a random rotation
                GameObject droppedEXP = Instantiate(expDrop);
                droppedEXP.transform.position = enemyPos;



                // clamp it within the player's bounds so they can pick it up
                Vector3 position = droppedEXP.transform.position;
                
                float cameraLimX = playerController.cameraFollow.limits.x;
                float cameraLimY = playerController.cameraFollow.limits.y;

                float limitMultX = playerController.cameraFollow.limitMult.x;
                float limitMultY = playerController.cameraFollow.limitMult.y;

                // hardcoded limits mimicked from the playercontroller
                float xLimit = cameraLimX * limitMultX;
                float yLimit = cameraLimY * limitMultY;

                position.x = Mathf.Clamp(position.x, -xLimit, xLimit);
                position.y = Mathf.Clamp(position.y, -yLimit, yLimit);

                // setting the clamp
                droppedEXP.transform.position = new Vector3(position.x, position.y, droppedEXP.transform.position.z);




                // setting the exp value
                droppedEXP.GetComponent<PickupScript>().expValue = 1;

                Rigidbody rb = droppedEXP.GetComponent<Rigidbody>();

                // add force in positive Z so that it can fly in front of the player briefly and let them know it exists
                float kbForce = 250f;
                rb.AddForce(new Vector3(0, 0, 1) * kbForce, ForceMode.Impulse);

                // spinning it in a random direction
                float maxAngularVelocity = 10f;

                Vector3 randomAngularVelocity = new Vector3(
                    Random.Range(-maxAngularVelocity, maxAngularVelocity),
                    Random.Range(-maxAngularVelocity, maxAngularVelocity),
                    Random.Range(-maxAngularVelocity, maxAngularVelocity)
                );

                rb.angularVelocity = randomAngularVelocity;

                Destroy(droppedEXP, 5);



            }


        }

        particleHandler.CreateParticle(transform.position, "explosion");

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
