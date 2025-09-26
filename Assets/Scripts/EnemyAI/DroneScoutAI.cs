using UnityEngine;

public class DroneScoutAI : MonoBehaviour
{
    public EnemyInit enemyBehavior;
    public StateMachine stateMachine;
    public GameObject enemyPlane;
    public GameObject droneGrid;

    // drone scouts align themselves in a grid with other drone scouts, similar to space invaders

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get state machine and enemybehavior (HP, attack, etc.) information
        stateMachine = GetComponent<StateMachine>();
        enemyBehavior = GetComponent<EnemyInit>();

        // get the enemy plane and change its parent to the DroneGrid
        enemyPlane = GameObject.Find("EnemyPlane");
        Transform dgTrans = enemyPlane.transform.Find("DroneGrid");

        if (dgTrans)
        {
            transform.SetParent(dgTrans);
        }

        // run their enemy ai
        EnemyAI();
    }

    // Update is called once per frame
    void Update()
    {
        //print("the drone scout is currently: " + stateMachine.currentState);
    }

    public void EnemyAI()
    {
        //print("drone scout ai");
    }

}
