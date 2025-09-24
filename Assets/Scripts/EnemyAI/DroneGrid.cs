using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DroneGrid : MonoBehaviour
{
    // align each child in a grid similar to space invaders
    [Header("Settings")]
    public int horizontalAmnt;
    public int verticalAmnt;

    [Header("Enemies")]
    public List<GameObject> enemyGrid;
    public int enemyCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // try to align the grid
        //AlignGrid();
    }

    // returns the number of children in the grid
    private void GetChildren()
    {

    }

    // get anytime a child is added (spawned) or removed (destroyed)
    private void OnTransformChildrenChanged()
    {
        int childrenCount = enemyGrid.Count;

        if (childrenCount < enemyCount)
        {
            // an enemy was destroyed

        } else
        {
            // an enemy was spawned

        }

        enemyCount = childrenCount;
    }


    // lines up the grid based on positions and what is missing.
    private void AlignGrid()
    {
        // logic:
        // on first start, get the number of children of the drone grid and add them to the list
        // afterwards, everytime a child is added to the grid, add it to the list
        // if a child dies, set the index to null instead of removing it to preserve the gap
        // 


        // get all children and add them to a list
    }

}
