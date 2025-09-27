using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public class DroneHeavyScoutAI : MonoBehaviour
{
    [Header("References")]
    public EnemyInit enemyBehavior;
    public StateMachine stateMachine;
    public GameObject enemyPlane;
    public GameObject playerShip;
    public GameObject laserProjectile;

    [Header("Settings")]
    public float attackPrepTime = 0.5f;
    public float minCooldown = 5f;
    public float maxCooldown = 6f;
    public float projectileSpeed = 35f;
    public float projectileLifetime = 4f;
    public float forwardOffset = 5f; // makes the drone fly closer to the player

    private Vector2 limits;

    // drone heavy scouts fly in front of drones and strafe to random positons, trying to shoot the player

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get state machine and enemybehavior (HP, attack, etc.) information
        stateMachine = GetComponent<StateMachine>();
        enemyBehavior = GetComponent<EnemyInit>();
        playerShip = GetComponent<EnemyInit>().playerShip;

        laserProjectile = Resources.Load<GameObject>("Projectiles/laserProjectile");

        // get the enemy plane and change its parent to the DroneGrid
        enemyPlane = GameObject.Find("EnemyPlane");
        Transform dgTrans = enemyPlane.transform;

        // setting the drone's limits to the camera's limits
        limits = enemyPlane.GetComponent<EnemyPlane>().limits;
        limits = new Vector2(limits.x * 4, limits.y * 4);

        if (dgTrans)
        {
            transform.SetParent(dgTrans);
            transform.position = dgTrans.position;
        }

    }

    // Update is called once per frame
    void Update()
    {

        // drone heavy scout ai:
        // get a random cooldown time
        // once the cooldown elapses, move to a new random location
        // once at that location, prep then shoot at the player
        if (stateMachine.currentState == StateMachine.EnemyState.Idle)
        {
            print("moving");

            // get a random location and move there
            Vector3 randomPos = GetRandomPosition();

            // tweening to that location
            transform.DOLocalMove(randomPos, 0.5f);

            StartCoroutine(AttackCooldown());
        }

    }

    private void LateUpdate()
    {
        // cause the drone to look at the player
        transform.LookAt(playerShip.transform.position);

        // clamps their position to the limits defined above
        Vector3 localPos = transform.position;
        Vector3 planePos = enemyPlane.transform.position;
        //transform.localPosition = new Vector3(Mathf.Clamp(localPos.x, -limits.x, limits.x), Mathf.Clamp(localPos.y, -limits.y, limits.y), planePos.z);
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 randomPos;
        float xPosition = Random.Range(-limits.x, limits.x);
        float yPosition = Random.Range(-limits.y, limits.y);

        randomPos = new Vector3(xPosition, yPosition, -forwardOffset);

        return randomPos;
    }

    private void ChangeColor(Color color)
    {
        Renderer objectRenderer = transform.GetChild(0).GetComponent<Renderer>();
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

        // setting its lifetime
        Destroy(firedLaser, projectileLifetime);

        // moving it towards the player
        Rigidbody rb = firedLaser.GetComponent<Rigidbody>();

        if (rb != null)
            rb.AddForce(firedLaser.transform.forward * projectileSpeed, ForceMode.Impulse);

        // setting their state to cooldown
        stateMachine.currentState = StateMachine.EnemyState.Cooldown;
        ChangeColor(Color.grey);

        // waiting for the cooldown
        yield return new WaitForSeconds(Random.Range(minCooldown, maxCooldown));

        stateMachine.currentState = StateMachine.EnemyState.Idle;
    }

    private IEnumerator AttackCooldown()
    {
        stateMachine.currentState = StateMachine.EnemyState.Preparing;
        ChangeColor(Color.red);

        yield return new WaitForSeconds(attackPrepTime);

        StartCoroutine(Attack());
    }


}
