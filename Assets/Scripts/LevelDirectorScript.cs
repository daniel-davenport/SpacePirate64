using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class LevelDirector : MonoBehaviour
{

    [Header("References")]
    public GameObject playerPlane;
    public GameObject StartLine;
    public GameObject FinishLine;
    public SpawnDirector spawnDirector;
    public Transform startPosition;

    [Header("Level Objects")]
    public GameObject[] levelBlocks;
    public List<GameObject> spawnableBlocks;
    public List<GameObject> spawnedBlocks;

    [Header("Level Parameters")]
    public bool gameStarted;
    public int levelTickets;
    public int maxLevelTickets;
    public float inLevelSpeed = 40;
    public float outLevelSpeed = 20;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelBlocks = Resources.LoadAll<GameObject>("LevelBlocks/Enabled");

        spawnableBlocks = new List<GameObject>(levelBlocks); // resetting the spawnable blocks list

        
        //StartCoroutine(DelayGeneration(3f)); 
        StartGeneration();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // when they collide with the starting line
    public void StartCollided()
    {
        print("start line");
        playerPlane.GetComponent<ForwardMovement>().moveSpeed = inLevelSpeed;

        // make the start line invisible
        StartLine.GetComponent<MeshRenderer>().enabled = false;

        gameStarted = true;

    }


    // when they collide with the finish line
    public void FinishCollided()
    {
        print("finish line");
        playerPlane.GetComponent<ForwardMovement>().moveSpeed = outLevelSpeed;

        // make the finish line invisible
        FinishLine.GetComponent<MeshRenderer>().enabled = false;

        gameStarted = false;


        // clear all enemies
        spawnDirector.DestroyAllEnemies();

        // set intensity back to half
        spawnDirector.intensity = Mathf.FloorToInt(spawnDirector.maxIntensity / 2);

        // Reset your level tickets
        levelTickets = maxLevelTickets;

        // restart level generation
        StartGeneration();

        // set the playerplane's position to 0,0,0
        playerPlane.transform.position = Vector3.zero;

    }

    // destroying the spawned level and clearing the list so that it can be used again.
    public void ClearSpawnedBlocks()
    {
        if (spawnedBlocks.Count > 0)
        {
            foreach(GameObject spawnedBlock in spawnedBlocks)
            {
                Destroy(spawnedBlock);
            }
        }

        spawnedBlocks.Clear();
    }

    // delaying the generation to test execution speed
    IEnumerator DelayGeneration(float time)
    {
        print("level generation will begin in " + time + " seconds.");
        yield return new WaitForSeconds(time);
        print("generating level.");
        StartGeneration();
    }

    // clears the previous level and starts a new generation
    public void StartGeneration()
    {
        // clear the previous level
        ClearSpawnedBlocks();

        // refresh the spawnable blocks array
        spawnableBlocks = new List<GameObject>(levelBlocks);

        // basic logic:
        // while you have level tickets, get a random block from the spawnable list
        // if you have enough tickets to use that block, then select it and append it to the spawnedBlocks list, subtract the cost from levelTickets
        // if you don't have enough tickets for it, remove it from the spawnable list
        while (levelTickets > 0)
        {
            if (spawnableBlocks.Count > 0)
            {
                // get a random spawnable
                int randomIndex = Random.Range(0, spawnableBlocks.Count);

                GameObject randomBlock = spawnableBlocks[randomIndex];

                int randomBlockCost = randomBlock.GetComponent<LevelInformation>().levelCost;

                if (randomBlockCost > levelTickets)
                {
                    // remove it from the list and move on
                    spawnableBlocks.RemoveAt(randomIndex);
                    continue;
                } 
                else
                {
                    // add it to the spawned blocks and set its position properly
                    GameObject spawnedBlock = Instantiate(randomBlock);

                    Transform spawnedStart = spawnedBlock.GetComponent<LevelInformation>().startPosition;

                    // setting the position
                    if (spawnedBlocks.Count <= 0)
                    {
                        // set it to the leveldirector's start
                        Vector3 offset = startPosition.position - spawnedStart.position;
                        spawnedBlock.transform.position += offset;
                    } 
                    else
                    {
                        // set it to the previous item's start
                        Transform previousEnd = spawnedBlocks[spawnedBlocks.Count - 1].GetComponent<LevelInformation>().endPosition;

                        Vector3 offset = previousEnd.position - spawnedStart.position;
                        spawnedBlock.transform.position += offset;
                        spawnedBlock.transform.SetParent(transform);

                    }

                    // add it to the end of the list at the end
                    spawnedBlocks.Add(spawnedBlock);

                    // subtract the level tickets
                    levelTickets -= randomBlockCost;


                }


            } 
            else
            {
                // error condition, should never throw.
                Debug.Log("Ran out of spawnable blocks, breaking.");
                break;
            } 

            
        }

        print("level finished loading.");

        // set the start and finish lines
        StartLine.transform.position = startPosition.position;
        FinishLine.transform.position = spawnedBlocks[spawnedBlocks.Count - 1].GetComponent<LevelInformation>().endPosition.position;

        // and make them both visible
        StartLine.GetComponent<MeshRenderer>().enabled = true;
        FinishLine.GetComponent<MeshRenderer>().enabled = true;





    }


}
