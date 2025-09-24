using UnityEngine;

public class DroneScoutAI : MonoBehaviour
{
    public EnemyInit enemyBehavior;
    public StateMachine stateMachine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get state machine and enemybehavior (HP, attack, etc.) information
        stateMachine = GetComponent<StateMachine>();
        enemyBehavior = GetComponent<EnemyInit>();

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
        print("drone scout ai");
    }

}
