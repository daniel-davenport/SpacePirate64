using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class DroneScoutAI : MonoBehaviour
{
    [Header("References")]
    public EnemyInit enemyInit;
    public StateMachine stateMachine;
    public GameObject enemyPlane;
    public GameObject droneGrid;
    public GameObject playerShip;
    public GameObject laserProjectile;

    [Header("Settings")]
    public float attackPrepTime = 0.5f;
    public float minCooldown = 3f;
    public float maxCooldown = 5f;
    public float projectileSpeed = 35f;
    public float projectileLifetime = 4f;

    // drone scouts align themselves in a grid with other drone scouts, similar to space invaders

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get state machine and enemyInit (HP, attack, etc.) information
        stateMachine = GetComponent<StateMachine>();
        enemyInit = GetComponent<EnemyInit>();
        playerShip = GetComponent<EnemyInit>().playerShip;

        laserProjectile = Resources.Load<GameObject>("Projectiles/laserProjectile");

        // get the enemy plane and change its parent to the DroneGrid
        enemyPlane = GameObject.Find("EnemyPlane");
        Transform dgTrans = enemyPlane.transform.Find("DroneGrid");

        if (dgTrans)
        {
            transform.SetParent(dgTrans);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //print("the drone scout is currently: " + stateMachine.currentState);
        
        // drone scout ai:
        // get a random cooldown time
        // once the cooldown elapses, prep then shoot towards the player
        if (stateMachine.currentState == StateMachine.EnemyState.Idle)
        {
            //print("preparing attack");
            StartCoroutine(AttackCooldown());
        }

    }

    private void ChangeColor(Color color)
    {
        Renderer objectRenderer = transform.GetChild(0).GetComponent<Renderer>();

        if (objectRenderer != null)
            objectRenderer.material.color = color;
    }

    private IEnumerator Attack()
    {
        // target the enemy player and shoot a laser at them
        stateMachine.currentState = StateMachine.EnemyState.Attacking;

        // fire a laser at the player
        GameObject firedLaser = Instantiate(laserProjectile, transform.position, Quaternion.identity);
        firedLaser.transform.SetParent(enemyPlane.transform);
        firedLaser.transform.LookAt(playerShip.transform.position);

        // setting the owner
        firedLaser.GetComponent<ProjectileInfo>().projectileOwner = transform.gameObject;
        firedLaser.GetComponent<ProjectileInfo>().projectileDamage = enemyInit.projectileDamage;

        // setting its lifetime
        Destroy(firedLaser, projectileLifetime);

        // moving it towards the player
        Rigidbody rb = firedLaser.GetComponent<Rigidbody>();

        if (rb != null)
            rb.AddForce(firedLaser.transform.forward * projectileSpeed, ForceMode.Impulse);

        // setting their state to cooldown
        stateMachine.currentState = StateMachine.EnemyState.Cooldown;
        ChangeColor(Color.grey);

        // play a sound effect
        enemyInit.sfxScript.PlaySFX("EnemyLaser");

        // waiting for the cooldown
        yield return new WaitForSeconds(Random.Range(minCooldown, maxCooldown));

        stateMachine.currentState = StateMachine.EnemyState.Idle;
    }

    private IEnumerator AttackCooldown()
    {
        stateMachine.currentState = StateMachine.EnemyState.Preparing;
        ChangeColor(Color.red);

        // play a sound effect
        enemyInit.sfxScript.PlaySFX("EnemyAlert");

        yield return new WaitForSeconds(attackPrepTime);

        StartCoroutine(Attack());
    }

}
