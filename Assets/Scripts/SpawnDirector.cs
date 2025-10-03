using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public WeaponHandler weaponHandler;
    public LevelDirector levelDirector;
    public GameObject enemyHolder;

    [Header("Player Stats")]
    private float timeSinceDamage;
    private float timeSinceBombUsed;
    private int enemyKillCount;
    private int droneCount;

    [Header("Spawn Stats")]
    public float intensity; // starts at half
    public float maxIntensity;
    public int spawnTickets;
    public int maxSpawnTickets;

    [Header("Enemies List")]
    public TextAsset enemyListJson;
    public List<GameObject> spawnedEnemies;
    public List<GameObject> spawnableEnemies;


    // setting up enemy .json file
    [System.Serializable]
    public class Enemy
    {
        public string name;
        public int cost;
    }

    [System.Serializable]
    public class EnemyList
    {
        public Enemy[] enemies;
    }

    [SerializeField]
    public EnemyList allEnemiesList = new EnemyList();
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allEnemiesList = JsonUtility.FromJson<EnemyList>(enemyListJson.text);

        spawnTickets = 5;

        StartCoroutine(SpawnCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
        // check if the player takes damage, if they do lower intensity

        // check if the player has killed an enemy, if they do, increase intensity


    }


    private string FindEnemy(int cost)
    {
        string foundEnemy = "";

        // clone the array
        List<Enemy> randomEnemies = new List<Enemy>(allEnemiesList.enemies);

        while (foundEnemy == "" && randomEnemies.Count > 0)
        {

            // find a random index
            int randomIndex = Random.Range(0, randomEnemies.Count);

            // checking the random index's cost
            if (randomEnemies[randomIndex].cost <= cost)
            {
                // this is the enemy to be spawned
                foundEnemy = randomEnemies[randomIndex].name;
            }
            else
            {
                // remove this from the list and let the loop take it over
                randomEnemies.Remove(randomEnemies[randomIndex]);
            }

        }

        print(foundEnemy);

        return foundEnemy;
    }

    private void SpawnEnemy(string enemyName)
    {
        // instantiate the enemyHolder and change the enemyName to the spawned enemy name


    }


    private IEnumerator SpawnCoroutine()
    {
        print("coroutine started");
        // only spawn while the game is started
        while (true)
        {
            // waits until the game starts
            if (levelDirector.gameStarted == false)
                yield return new WaitUntil(() => levelDirector.gameStarted == true);

            // after this point, game should be started
            print("game in progress");



            // logic:
            // every second check if there are spawn tickets, 
            // if there are, then do an rng check against the max intensity level
            // if it passes, get a random enemy, if you can afford it then mark its name as the spawned enemy type and decrease tickets by that amount
            // then, check intensity
            // if intensity is high, increase max spawn tickets, if the intensity is low, do nothing
            // if spawn tickets are zero and intensity is high, set them to max
            // if spawn tickets are zero and intensity is low, set them to half

            if (spawnTickets > 0)
            {
                // getting an rng check to see if an enemy will spawn
                float rng = Random.Range(0, maxIntensity);

                print(rng);

                // less intensity = less spawns
                if (rng <= intensity)
                {
                    // spawn check passed

                    // get a random enemy from the available enemies
                    string randomEnemy = FindEnemy(spawnTickets);

                    if (randomEnemy != "")
                        SpawnEnemy(randomEnemy);

                }





            }






            // spawn controller is polled every second
            yield return new WaitForSeconds(1);

        }

    }


}
