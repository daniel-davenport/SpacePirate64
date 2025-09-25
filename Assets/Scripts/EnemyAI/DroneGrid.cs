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
    public float cellSize;

    [Header("Enemies")]
    public List<GameObject> enemyGrid;
    public int enemyCount = 0;
    public int freeSpace = 0; // the next free slot for a drone

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

    // Iterates through the list of drones and sees if any of them are null
    // returns an index
    private int FindListGap()
    {
        int gap = 0;
        int index = 0;

        foreach (GameObject drone in enemyGrid)
        {
            //print(drone);
            if (drone == null)
            {
                //print("gap at: " + index);
                gap = index;
                return gap;
            }

            index++;
        }

        if (enemyGrid.Count > 0)
            gap = enemyGrid.Count;

        return gap;

    }

    // get anytime a child is added (spawned) or removed (destroyed)
    private void OnTransformChildrenChanged()
    {
        int childrenCount = transform.childCount;

        if (childrenCount < enemyCount)
        {
            // an enemy was destroyed

            // find the new gap 
            //freeSpace = FindListGap();

        } else
        {
            // an enemy was spawned

            // getting the child just added
            int lastChildIndex = transform.childCount - 1;
            Transform lastChildTrans = transform.GetChild(lastChildIndex);

            // find if there's a gap in the drones, if there is insert it there
            freeSpace = FindListGap();

            // otherwise insert it at the end
            // note: when inserting if you do list.insert it will shove the null value, you should do list[freespace] = newobject to replace the null

            if (freeSpace >= enemyGrid.Count - 1)
            {
                // insert it at the end 
                enemyGrid.Add(lastChildTrans.gameObject);
            } 
            else
            {
                // insert it at the null point
                enemyGrid[freeSpace] = lastChildTrans.gameObject;
            }

            // determining the appropriate position
            int dronePlace = freeSpace + 1; // what X position it's at
            int verticalOffset = 0; // what Y position it's at

            // divide to get its vertical position
            while (dronePlace > horizontalAmnt)
            {
                if (dronePlace <= horizontalAmnt)
                    break;

                // subtract to get the next row
                dronePlace -= horizontalAmnt;
                verticalOffset += 1;

            }

            //print("drone will be placed at: " + dronePlace + ", " + verticalOffset);
            
            float droneXPos = cellSize * dronePlace;
            float droneYPos = cellSize * verticalOffset;

            // TODO:
            // the drone grid works, it fills in gaps as expected
            // all that is needed is to properly position it in space in a centered grid
            
            lastChildTrans.transform.position = new Vector3(droneXPos, droneYPos, transform.position.z);

            //print("enemy spawned at: " + freeSpace);

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
