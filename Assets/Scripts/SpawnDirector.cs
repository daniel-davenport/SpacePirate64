using System;
using System.Collections;
using System.Collections.Generic;
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
    private float intensity;
    private float maxIntensity = 50f;
    private int spawnTickets;
    private int maxSpawnTickets;

    [Header("Enemies List")]
    public TextAsset enemyListJson;
    public List<GameObject> spawnedEnemies;


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

        intensity = 0;
        spawnTickets = 5;

        StartCoroutine(SpawnCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
        // check if the player takes damage, if they do lower intensity

        // check if the player has killed an enemy, if they do, increase intensity


    }


    private void SpawnEnemy()
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



            // spawn controller is polled every second
            yield return new WaitForSeconds(1);

        }

    }


}
