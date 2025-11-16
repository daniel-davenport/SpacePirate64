using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class SpawnDirector : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public WeaponHandler weaponHandler;
    public LevelDirector levelDirector;
    public ScoreHandler scoreHandler;
    public GameObject enemyHolder;
    public DroneGrid droneGrid;
    public ParticleHandler particleHandler;
    public Slider intensitySlider;

    [Header("Player Stats")]
    private float timeSinceDamage;
    private float timeSinceBombUsed;
    private int enemyKillCount;
    private int droneCount;

    [Header("Spawn Stats")]
    private float basePollTime = 1f; // how long between enemy spawn attempts
    public float intensity; // starts at half
    public float maxIntensity;
    public int spawnTickets;
    public int maxSpawnTickets;

    [Header("Enemies List")]
    private TextAsset enemyListJson;
    public List<GameObject> spawnedEnemies;
    public List<GameObject> spawnableEnemies;


    // setting up enemy .json file
    [System.Serializable]
    public class Enemy
    {
        public string name;
        public int cost;
        public bool usingGrid;
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
        //levelDirector = GameObject.FindFirstObjectByType<LevelDirector>();

        // reading enemy info from the physical json instead of a compiled one
        string enemyFilePath = Path.Combine(Application.streamingAssetsPath, "EnemyData.json");
        if (File.Exists(enemyFilePath)) {
            string fileContent = File.ReadAllText(enemyFilePath);
            enemyListJson = new TextAsset(fileContent);

            allEnemiesList = JsonUtility.FromJson<EnemyList>(enemyListJson.text);
        } else
        {
            print("ERROR: ENEMY DATA FILE NOT FOUND.");
        }
            

        intensity = Mathf.FloorToInt(maxIntensity / 2); // later change this based on difficulty
        spawnTickets = Mathf.FloorToInt(maxSpawnTickets / 2);

        StartCoroutine(SpawnCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        // clamping intensity
        if (intensity > maxIntensity) 
            intensity = maxIntensity;

        if (intensity < 0) 
            intensity = 0;

        if (intensitySlider)
        {
            float intensityRatio = (float)(intensity / maxIntensity);
            intensitySlider.value = intensityRatio;
        }
        
    }

    // changing the intensity from other scripts
    // taking damage, using bombs, playing poorly should fire this (increase = false)
    // killing an enemy, perfect parrying, playing well should fire this (increase = true)
    public void ChangeIntensity(bool increase, int amount)
    {

        if (increase == true)
        {
            intensity += amount;
            //print("intensity increased");
        }
        else
        {
            intensity -= amount;
            //print("intensity decreased");
        }

    }

    // simply halves intensity (useful when taking damage, etc)
    public void HalveIntensity()
    {
        intensity = Mathf.FloorToInt(intensity / 2);
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

            // finding out if it uses the drone grid
            bool usingDroneGrid = randomEnemies[randomIndex].usingGrid;

            if (usingDroneGrid == true)
            {
                int maxDrones = droneGrid.horizontalAmnt * droneGrid.verticalAmnt;
                int currentDrones = droneGrid.enemyCount;

                

                if (currentDrones >= maxDrones)
                {
                    // no more drones can fit, remove it and continue
                    spawnTickets += 1; // increases spawn tickets to allow a stronger enemy to spawn

                    randomEnemies.Remove(randomEnemies[randomIndex]);
                    continue;
                }

            }

            // checking the random index's cost
            if (randomEnemies[randomIndex].cost <= cost)
            {

                // this is the enemy to be spawned
                foundEnemy = randomEnemies[randomIndex].name;

                // subtract spawn tickets
                spawnTickets -= randomEnemies[randomIndex].cost;

            }
            else
            {
                // remove this from the list and let the loop take it over
                randomEnemies.Remove(randomEnemies[randomIndex]);
            }

        }

        return foundEnemy;
    }


    // just destroys projectiles, useful for blanks and stuff
    public void DestroyAllProjectiles()
    {
        // tells the projectiles to destroy themselves
        BroadcastMessage("LevelEnded", SendMessageOptions.DontRequireReceiver);
    }

    // destroying all enemies and resetting the list
    public void DestroyAllEnemies()
    {
        DestroyAllProjectiles();

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
            {
                Destroy(spawnedEnemies[i]);
            }
        }

        spawnedEnemies = new List<GameObject>();
    }


    private void SpawnEnemy(string enemyName)
    {
        // instantiate the enemyHolder and change the enemyName to the spawned enemy name
        GameObject spawnedEnemy = Instantiate(enemyHolder);

        // setting their name and reference to the player
        spawnedEnemy.GetComponent<EnemyInit>().playerShip = weaponHandler.playerShip;
        spawnedEnemy.GetComponent<EnemyInit>().enemyName = enemyName;
        spawnedEnemy.GetComponent<EnemyInit>().spawnDirector = this;
        spawnedEnemy.GetComponent<EnemyInit>().scoreHandler = scoreHandler;
        spawnedEnemy.GetComponent<EnemyInit>().particleHandler = particleHandler;
        spawnedEnemy.GetComponent<EnemyInit>().playerController = playerController;

        spawnedEnemies.Add(spawnedEnemy);
        // their AI should handle the rest.
    }


    private IEnumerator SpawnCoroutine()
    {
        // print("coroutine started");
        // only spawn while the game is started
        while (true)
        {
            // waits until the game starts
            if (levelDirector.gameStarted == false)
                yield return new WaitUntil(() => levelDirector.gameStarted == true);

            // after this point, game should be started
            //print("game in progress");

            // logic:
            // every second check if there are spawn tickets, 
            // if there are, then do an rng check against the max intensity level
            // if it passes, get a random enemy, if you can afford it then mark its name as the spawned enemy type and decrease tickets by that amount
            // then, check intensity
            // if intensity is high, increase max spawn tickets, if the intensity is low, do nothing
            // if spawn tickets are zero and intensity is high, set them to max
            // if spawn tickets are zero and intensity is low, set them to half
            float pollTime = basePollTime;


            if (spawnTickets > 0)
            {
                // getting an rng check to see if an enemy will spawn
                float rng = Random.Range(0, maxIntensity);

                //print(rng);

                // less intensity = less spawns
                if (rng <= intensity)
                {
                    // spawn check passed

                    // get a random enemy from the available enemies
                    string randomEnemy = FindEnemy(spawnTickets);

                    if (randomEnemy != "")
                        SpawnEnemy(randomEnemy);

                } else
                {
                    // if an enemy doesn't spawn, increase intensity
                    intensity += 1;
                }
            }
            else
            {
                // out of spawn tickets
                float intensityRatio = intensity / maxIntensity;

                // adaptive increase/decrease to enemy spawns based on how good/bad the player is doing
                if (intensityRatio > 0.5f)
                {
                    // intensity is high, keep the spawn tickets high by how good they're doing
                    maxSpawnTickets += Mathf.CeilToInt(10 * intensityRatio);
                    spawnTickets = maxSpawnTickets;
                } 
                else
                {
                    // intensity is low, increase spawn tickets by the ratio of intensity
                    // note: this means the worse you're performing, the less enemies will spawn
                    spawnTickets = Mathf.FloorToInt(maxSpawnTickets * intensityRatio);
                }

            }


            // playing at higher intensities will ramp the spawn timer
            if (intensity >= maxIntensity / 1.75)
            {
                // spawn time will approach 0.5 the better you do
                pollTime = basePollTime / ((intensity / maxIntensity) + 0.75f);
            }
            else
            {
                pollTime = basePollTime;
            }
            
            // spawn controller is polled every second, unless the player is preforming exceptional.
            yield return new WaitForSeconds(pollTime);

        }

    }


}
