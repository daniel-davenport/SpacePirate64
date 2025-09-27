using System;
using UnityEngine;

public class StateMachine : MonoBehaviour
{

    [Header("States")]
    public EnemyState currentState;
    
    public enum EnemyState
    {
        Idle,
        Preparing,
        Attacking,
        Cooldown,
        Dead
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
