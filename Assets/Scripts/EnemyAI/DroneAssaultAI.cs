using DG.Tweening;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class DroneAssaultAI : MonoBehaviour
{
    [Header("References")]
    public EnemyInit enemyInit;
    public StateMachine stateMachine;
    public GameObject enemyPlane;
    public GameObject playerShip;
    public GameObject laserProjectile;
    public GameObject missileProjectile;

    [Header("Settings")]
    public float attackPrepTime = 2f;
    public float minCooldown = 3f;
    public float maxCooldown = 4f;
    public float projectileSpeed = 18f;
    public float projectileLifetime = 8f;
    private float forwardOffset = -10f; // makes the drone fly away from the player

    private bool lockingOn = false;
    private LineRenderer lockOnLine;
    private Vector3 boxExtents;
    private Vector2 limits;

    // drone heavy scouts fly in front of drones and strafe to random positons, trying to shoot the player

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get state machine and enemyInit (HP, attack, etc.) information
        stateMachine = GetComponent<StateMachine>();
        enemyInit = GetComponent<EnemyInit>();
        playerShip = GetComponent<EnemyInit>().playerShip;

        laserProjectile = Resources.Load<GameObject>("Projectiles/laserProjectile");
        missileProjectile = Resources.Load<GameObject>("Projectiles/missileProjectile");

        // get the enemy plane and change its parent to the DroneGrid
        enemyPlane = GameObject.Find("EnemyPlane");
        Transform dgTrans = enemyPlane.transform;

        //print(dgTrans);

        // setting the drone's limits to the camera's limits
        limits = enemyPlane.GetComponent<EnemyPlane>().limits;
        limits = new Vector2(limits.x * 4, limits.y * 4);

        if (dgTrans)
        {
            transform.SetParent(dgTrans);
            transform.position = dgTrans.position;
        }

        // make the drone look forward
        transform.rotation = Quaternion.LookRotation(dgTrans.forward);

        // get a random location and move there
        Vector3 randomPos = GetRandomPosition();

        // offsetting the randomPos to be behind the player
        Vector3 startPos = new Vector3(randomPos.x, randomPos.y, 40);
        transform.position = startPos;

        // remove its collider until it reaches its location
        transform.GetChild(0).GetComponent<Collider>().enabled = false;

        // tweening to that location, then enabling the collider
        transform.DOLocalMove(randomPos, 1.5f).SetLink(transform.gameObject).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            transform.GetChild(0).GetComponent<Collider>().enabled = true;
        });

        // setting the box collider's constraints
        float boxExtentsX = transform.GetChild(0).GetComponent<BoxCollider>().size.x / 2;
        float boxExtentsY = transform.GetChild(0).GetComponent<BoxCollider>().size.y / 2;
        float boxExtentsZ = transform.GetChild(0).GetComponent<BoxCollider>().size.z / 2;

        boxExtents = new Vector3(boxExtentsX, boxExtentsY, boxExtentsZ);


        // creating the laser for the lockon
        lockOnLine = gameObject.AddComponent<LineRenderer>();
        lockOnLine.SetPosition(0, transform.position);

        lockOnLine.startWidth = 0.5f;
        lockOnLine.endWidth = 0.5f;

        lockOnLine.material = Resources.Load<Material>("Materials/EnemyMissile");
        lockOnLine.startColor = Color.red;
        lockOnLine.endColor = Color.red;


}

    // Update is called once per frame
    void Update()
    {

        // drone assault ai:
        // pick a random spot in the screen away from the center
        // get a random cooldown time
        // lock onto the player with a line showing a missile is coming
        // remove the line, shoot the missile, then cooldown and prepare to shoot another
        // missile follows the player slowly
        // constantly shapecast forward and try to avoid obstacles

        if (stateMachine.currentState == StateMachine.EnemyState.Idle)
        {
            //print("moving");


            StartCoroutine(AttackCooldown());
        }


        // constantly check if there's an obstacle
        CheckObstacle();
        UpdateLineRenderer();

    }

    private void LateUpdate()
    {

        // clamps their position to the limits defined above
        Vector3 localPos = transform.position;
        Vector3 planePos = enemyPlane.transform.position;
        //transform.localPosition = new Vector3(Mathf.Clamp(localPos.x, -limits.x, limits.x), Mathf.Clamp(localPos.y, -limits.y, limits.y), planePos.z);
    }


    // check if there's an obstacle in front of it
    private void CheckObstacle()
    {
        RaycastHit hit;
        int layerMask = LayerMask.NameToLayer("Obstacle"); 
        LayerMask obstacleMask = (1 << LayerMask.NameToLayer("Obstacle")); 
        //print(layerMask.value);

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float strafeSpeed = 1f;

        //Debug.DrawRay(origin, direction * 100f, Color.yellow, 100f);

        // if there's a collision, get the cross product to see what direction to move in?
        // otherwise just try to move closer to the center of the screen?
        // or just scan rapidly and try to find a good direction to go in next
        // or get the player's direction and try to go in that direction, since the player will likely try to dodge as well?

        //Physics.SphereCast(origin, detectionRadius, direction, out hit, 50f, obstacleMask)

        if (Physics.BoxCast(origin, boxExtents, direction, out hit, quaternion.identity, 50f, obstacleMask))
        {

            Vector3 playerPos = playerShip.transform.position;

            // check the player's position (since the player will most likely be trying to avoid it, otherwise move towards the center of the screen
            //Vector3 avoidanceDirection = Vector3.Cross(direction, Vector3.up);
            Vector3 offsetPlayerPos = new Vector3(playerPos.x, playerPos.y, origin.z);

            Vector3 playerDirection = (offsetPlayerPos - origin).normalized;

            // step it over a bit
            Vector3 pointDirection = origin + (playerDirection.normalized * strafeSpeed);

            Vector3 moveToPlayerPos = new Vector3(pointDirection.x, pointDirection.y, -forwardOffset);
            transform.DOLocalMove(moveToPlayerPos, 0.25f).SetLink(transform.gameObject);



            /*
            RaycastHit stillHit;
            if (Physics.BoxCast(origin, boxExtents, direction, out stillHit, quaternion.identity, 50f, obstacleMask)) {
                print("still collision");

                // move towards the center of the screen on the X axis only if there's still a collision
                Vector3 centerXPos = new Vector3(enemyPlane.transform.position.x, origin.y, origin.z);
                Vector3 centerDirection = (centerXPos - origin).normalized;

                Vector3 toCenterDir = origin + (centerDirection * strafeSpeed);
                Vector3 moveToCenter = new Vector3(toCenterDir.x, toCenterDir.y, -forwardOffset);

                transform.DOLocalMove(moveToCenter, 0.25f).SetLink(transform.gameObject);
            }
            */







        }
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

        if (objectRenderer != null)
            objectRenderer.material.color = color;
    }


    // updates the linerenderer with the player's position
    private void UpdateLineRenderer()
    {
        if (lockingOn == true)
        {
            lockOnLine.SetPosition(0, transform.position);
            lockOnLine.SetPosition(1, playerShip.transform.position);
        } 
        else
        {
            lockOnLine.SetPosition(0, transform.position);
            lockOnLine.SetPosition(1, transform.position);
        }

    }

    private IEnumerator Attack()
    {
        // target the enemy player and shoot a laser at them
        stateMachine.currentState = StateMachine.EnemyState.Attacking;

        // fire a laser at the player
        GameObject firedLaser = Instantiate(missileProjectile, transform.position, Quaternion.identity);
        firedLaser.transform.SetParent(enemyPlane.transform);
        firedLaser.transform.LookAt(playerShip.transform.position);

        // setting the owner + damage
        firedLaser.GetComponent<ProjectileInfo>().projectileOwner = transform.gameObject;
        firedLaser.GetComponent<ProjectileInfo>().projectileDamage = enemyInit.projectileDamage;

        // setting its lifetime
        Destroy(firedLaser, projectileLifetime);

        // homing missile
        StartCoroutine(HomingProjectile(firedLaser, playerShip.transform));

        // setting their state to cooldown
        stateMachine.currentState = StateMachine.EnemyState.Cooldown;
        ChangeColor(Color.grey);

        // clearing the lock on
        lockingOn = false;
        lockOnLine.SetPosition(1, transform.position);

        // waiting for the cooldown
        yield return new WaitForSeconds(Random.Range(minCooldown, maxCooldown));

        stateMachine.currentState = StateMachine.EnemyState.Idle;
    }


    // missile homing
    private IEnumerator HomingProjectile(GameObject projectile, Transform target)
    {
        ProjectileInfo projInfo = projectile.GetComponent<ProjectileInfo>();
        if (projInfo == null)
            Destroy(projectile);

        while (target != null && projectile != null && projInfo.parried != true)
        {
            projectile.transform.LookAt(target);

            // getting the direction between them
            Vector3 direction = (target.position - projectile.transform.position);

            // note: this might work but might cause issues later, maybe have some other way of testing this?
            projectile.transform.position += (direction.normalized * projectileSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }


        if (projectile != null && projInfo.parried == true)
        {
            // projectile was parried
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                //projectile.transform.LookAt(transform);
                //rb.AddForce(projectile.transform.forward * projectileSpeed, ForceMode.Impulse);
                //projectile.transform.rotation = Quaternion.LookRotation(projectile.transform.forward);
            }

        }

    }



    private IEnumerator AttackCooldown()
    {
        stateMachine.currentState = StateMachine.EnemyState.Preparing;

        // brief period before being able to lock on
        yield return new WaitForSeconds(attackPrepTime / 2);
        
        ChangeColor(Color.red);

        // setting the lockon
        lockingOn = true;
        lockOnLine.SetPosition(1, playerShip.transform.position);

        yield return new WaitForSeconds(attackPrepTime);

        StartCoroutine(Attack());
    }


}
