using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Transform playerHolder;
    private Rigidbody playerRigidbody;

    [Header("Settings")]
    public bool usesRawInput = false;

    [Header("Parameters")]
    public int playerHealth = 3;
    public int maxHealth = 3;
    public bool isInvincible = false;
    public float iFrames = 1.5f;
    public float obstacleKBForce = 5f;

    public float xSpeed = 18;
    public float ySpeed = 18;
    public float lookSpeed = 320;
    public float lookIntensity = 1.5f;

    public float tiltSpeedBuff = 1.5f; // how much faster you move if tilting in the direction you're moving
    public float tiltSpeedNerf = 2f; // how much slower you move if tilting in the opposite direction you're moving

    public float aileronTime = 0.4f; // how long you're performing an aileron for
    public float perfectParryWindow = 0.15f; // how long the perfect parry window is
    public float parrySpeed = 75f; // how fast a parried projectile returns

    private bool perfectParry = false;

    [Header("Combat Parameters")]
    public float aileronCooldown = 1f;
    public float bombCooldown = 1f;

    // note: lean is for the automatic horizontal leaning for moving horizontally
    //       whereas tilt is for the manual tilting by pressing bumpers or Q/E
    public float leanLimit = 80; 
    public float leanLerpSpeed = 0.1f;

    public float tiltLimit = 60;
    public float tiltLerpSpeed = 0.5f;

    // Arrays that handle charge times and debounces based on weapon
    // note: AttackLeft = slot 0
    //       AttackRight = slot 1
    private float[] chargeTimes = new float[] { 0f, 0f }; // Tracks the current charge level
    public float[] maxChargeTimes = new float[] { 1f, 1f }; // The charge time needed to fire a charged shot
    public float[] firingSpeeds = new float[2];

    public int heldBombs = 1;
    public int maxBombs = 3;

    [Header("References")]
    public Transform aimTarget;
    public GameObject leftWeaponModel;
    public GameObject rightWeaponModel;
    public GameObject playerModel;
    public WeaponHandler weaponHandler;
    public SpawnDirector spawnDirector;
    public ScoreHandler scoreHandler;
    public PlayerUI playerUI;
    public ParticleHandler particleHandler;

    // Tilting Inputs
    InputAction tiltLeftAction;
    InputAction tiltRightAction;

    private Tweener tiltTween;
    private string lastTiltSide;
    private bool doingAileron;
    private bool canAileron = true;

    // Weapon Inputs & debounces
    InputAction attackLeftAction;
    InputAction attackRightAction;
    InputAction bombAction;

    GameObject[] weaponModels = new GameObject[2];

    bool[] attackDebounces = new bool[] { false, false };
    private bool bombDebounce = false;

    // materials
    private Material parriedMaterial;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // gets the player's model under the hitbox
        playerHolder = transform.GetChild(0);

        // getting the player's rigidbody
        playerRigidbody = transform.GetComponent<Rigidbody>();

        // getting the player's keybinds
        attackLeftAction = InputSystem.actions.FindAction("AttackLeft");
        attackRightAction = InputSystem.actions.FindAction("AttackRight");
        bombAction = InputSystem.actions.FindAction("Bomb");

        tiltLeftAction = InputSystem.actions.FindAction("TiltLeft");
        tiltRightAction = InputSystem.actions.FindAction("TiltRight");

        // getting the player's weapons
        weaponModels[0] = leftWeaponModel;
        weaponModels[1] = rightWeaponModel;

        // getting the player's materials
        parriedMaterial = Resources.Load<Material>("Materials/ParriedMaterial");

        // getting references
        weaponHandler = transform.GetComponent<WeaponHandler>();
        playerUI = transform.GetComponent<PlayerUI>();

        // setting their health
        playerUI.UpdateHealth(playerHealth);

        tiltTween.SetAutoKill(false);
    }

    // Update is called once per frame
    void Update()
    {
        // player death
        if (playerHealth <= 0)
            return;

        // note: getaxisraw is basically binary with no smoothing
        //       getaxis has smoothing that could be interpreted as delay, acts like a joystick.
        float hInput, vInput;
        if (usesRawInput)
        {
            hInput = Input.GetAxisRaw("Horizontal");
            vInput = Input.GetAxisRaw("Vertical");
        } 
        else 
        {
            hInput = Input.GetAxis("Horizontal");
            vInput = Input.GetAxis("Vertical");
        }
            

        LocalMove(hInput, vInput, xSpeed, ySpeed);
        AimLook(hInput, vInput, lookSpeed);
        HorizontalLean(playerHolder, hInput, leanLimit, leanLerpSpeed);


        // Attacking with slot 0
        if (attackLeftAction.IsPressed())
        {
            AttackStart(0);
        } else if (attackLeftAction.WasReleasedThisFrame())
        {
            AttackStart(0, chargeTimes[0]);
        }

        // Attacking with slot 1
        if (attackRightAction.IsPressed())
        {
            AttackStart(1);
        }
        else if (attackRightAction.WasReleasedThisFrame())
        {
            AttackStart(1, chargeTimes[1]);
        }

        // Firing a bomb
        if (bombAction.WasPressedThisFrame())
        {
            Bomb();
        }


        // tilting and aeliron
        // note: for whatever reason there seems to be an anti-spam feature built in?
        //       meaning whenever an aileron completes you can't mash to do another one as soon as it's off cooldown.
        //       maybe keep this even though i don't know how to remove it?
        if (tiltLeftAction.WasPressedThisFrame())
        {
            string side = "left";

            if (lastTiltSide == side && canAileron)
            {
                doingAileron = true;
                Aileron(side, hInput);
            } 
            else
            {
                Tilt(side);
            }

        } else if (tiltLeftAction.WasReleasedThisFrame())
        {
            EndTilt(hInput);
        }

        if (tiltRightAction.WasPressedThisFrame())
        {
            string side = "right";
            
            if (lastTiltSide == side && canAileron)
            {
                doingAileron = true;
                Aileron(side, hInput);
            }
            else
            {
                Tilt(side);
            }

        } else if (tiltRightAction.WasReleasedThisFrame())
        {
            EndTilt(hInput);
        }


        /*
         * when a player collides with something it gets shoved backwards in the Z axis which messes up the camera
         * when taking damage, maybe enable IsKinematic on the rigidbody, push the player towards the center of the screen/away from whatever it 
         * 
         */


    }

    private void LateUpdate()
    {
        // clamping the player's position
        // note: this has to be in lateupdate
        ClampPosition();
    }



    // ---------------------------------- Player Actions -------------------------------------------- // 

    // handles taking damage, handles lose states and max health.
    public void TakeDamage(int damage)
    {
        // ignore this function if the player is invincible
        if (isInvincible)
            return;

        // otherwise make them take damage
        playerHealth -= damage;

        // fire to the spawn director that they're playing worse
        spawnDirector.ChangeIntensity(false, damage);
        spawnDirector.HalveIntensity();

        // make them briefly invincible
        StartCoroutine(PlayerInvincibility());

        if (playerHealth <= 0)
        {
            playerHealth = 0;

            // making them explode
            particleHandler.PlayerDeath(playerHolder);

            // hide the reticle

        }
        
        // lose score for taking damage
        scoreHandler.ChangePlayerScore("damage");

        // update their UI
        playerUI.UpdateHealth(playerHealth);


    }

    // makes the player invisible recursively
    // false = invisible, true = visible
    private void PlayerVisibility(Transform transform, bool state)
    {
        // check the base transform first
        MeshRenderer transMR = transform.GetComponent<MeshRenderer>();
        if (transMR)
            transMR.enabled = state;

        // check the children
        foreach (Transform child in transform)
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mr)
                mr.enabled = state;

            if (child.childCount > 0)
            {
                foreach (Transform descendant in child)
                {
                    PlayerVisibility(descendant, state);
                }
                   
            }
        }
    }

    // handles code relating to individual weapon slot debounces and charged attacks
    void AttackStart(int weaponSlot, float chargeTime = 0f)
    {

        if (chargeTime > 0f && chargeTime >= maxChargeTimes[weaponSlot]) // firing a charged shot (ignores debounces)
        {
            Attack(weaponSlot, true);
            chargeTimes[weaponSlot] = 0f;

            return;

            /*
            Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
            objectRenderer.material.color = Color.white;
            */
        }
        else if (chargeTime > 0f && chargeTime < maxChargeTimes[weaponSlot]) // didnt fully charge
        {
            chargeTimes[weaponSlot] = 0f;
            return;
        }
        else if (attackDebounces[weaponSlot] == false && chargeTimes[weaponSlot] <= 0f) // firing a regular shot (note: will not fire if you're charging)
        {
            attackDebounces[weaponSlot] = true;
            Attack(weaponSlot, false);
            chargeTimes[weaponSlot] = 0f;

            /*
            Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
            objectRenderer.material.color = Color.blue;
            */
        }



        // charging up that slot's charged shot
        if (chargeTimes[weaponSlot] >= maxChargeTimes[weaponSlot])
        {
            //print(" ----------- fully charged slot " + weaponSlot + " ----------- ");

            /*
            Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
            objectRenderer.material.color = Color.red;
            */

            // try to lock on when you're fully charged
            weaponHandler.LockOn(weaponSlot);

        }
        else
        {
            chargeTimes[weaponSlot] += Time.deltaTime;
        }
            
    }

    // Fires the specific weapon called, also takes in the argument if it's a charged attack or not.
    void Attack(int weaponSlot, bool isCharged)
    {

        if (isCharged)
        {
            //print("charged shot: " + weaponSlot);
            weaponHandler.FireWeapon(weaponSlot, true);

        }
        else
        {
            //print("firing slot: " + weaponSlot);
            weaponHandler.FireWeapon(weaponSlot, false);

            // reset the weapon's cooldown, this can be checked in other ways later.
            StartCoroutine(ResetAttack(weaponSlot));
        }

    }

    // fires a bomb based on what you have equipped (added later). tracks your held bombs and cooldowns.
    void Bomb()
    {
        if (bombDebounce == false && heldBombs > 0)
        {
            bombDebounce = true;
            heldBombs -= 1;
            print("firing bomb");

            StartCoroutine(ResetBomb(bombCooldown));
        } else if (heldBombs <= 0) 
        {
            print("out of bombs!");
        }
    }


    // tilts the ship as an action and increases your speed in that direction while lowering your speed in the opposite direction.
    void Tilt(string side)
    {

        // debounce to prevent dotween from getting overloaded and from tweens clashing
        if (tiltTween != null || doingAileron == true)
            return;

        int dir = side == "left" ? -1 : 1;
        lastTiltSide = side;

        Vector3 tiltAmount = new Vector3(playerHolder.localEulerAngles.x, playerHolder.localEulerAngles.y, -dir * tiltLimit);

        // stopping the tween when it's completed to keep the leaning
        tiltTween = playerHolder.DOLocalRotate(tiltAmount, tiltLerpSpeed, RotateMode.Fast).SetEase(Ease.OutQuad).SetAutoKill(false)
            .OnComplete(() =>
            {
                tiltTween.Pause();
            });
    }

    // setting the tween for resetting after a tilt/aileron.
    void EndTilt(float axis)
    {
        if (tiltTween != null && !doingAileron)
        {
            // calculating what axis the player should be at when the tilt ends
            float zDest = -axis * leanLimit;
            Vector3 tiltAmount = new Vector3(playerHolder.localEulerAngles.x, playerHolder.localEulerAngles.y, zDest);

            // setting the wings back to the proper axis
            playerHolder.DOLocalRotate(tiltAmount, tiltLerpSpeed / 4, RotateMode.Fast).SetEase(Ease.OutQuad)
                .OnComplete(() => // once this ends you can't do an aileron anymore
                {
                    lastTiltSide = null;
                });

            tiltTween.Kill();
            tiltTween = null;

        }
    }

    // performs an aileron roll. timing it exactly when a projectile hits you will perform a perfect parry.
    void Aileron(string side, float axis)
    {
        canAileron = false;
        perfectParry = true;
        int dir = side == "left" ? -1 : 1;

        StartCoroutine(ResetPerfectParry());

        playerHolder.DOLocalRotate(new Vector3(playerHolder.localEulerAngles.x, playerHolder.localEulerAngles.y, 720 * -dir), aileronTime, RotateMode.LocalAxisAdd).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                doingAileron = false;
                lastTiltSide = null;

                // calculating what axis the player should be at when the aileron ends
                float zDest = -axis * leanLimit;
                Vector3 resetAngle = new Vector3(playerHolder.localEulerAngles.x, playerHolder.localEulerAngles.y, zDest);
                playerHolder.transform.localEulerAngles = resetAngle;

                StartCoroutine(ResetAileron(aileronCooldown));
            });
    }


    // removes your perfect parry window
    IEnumerator ResetPerfectParry()
    {
        yield return new WaitForSeconds(perfectParryWindow);
        perfectParry = false;
    }


    // resets the aileron's cooldown, modifiable cooldown.
    IEnumerator ResetAileron(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canAileron = true;
    }

    // resets the weapon's slot after X amount of time. 
    // note: cooldown time should be pulled from the weapon's live data, for now it's a default value
    IEnumerator ResetAttack(int weaponSlot)
    {
        // wait time until it comes back based on the weapon info
        yield return new WaitForSeconds(firingSpeeds[weaponSlot]);
        attackDebounces[weaponSlot] = false;


        // print("weapon " + weaponSlot + " reset");

        /*
        Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
        objectRenderer.material.color = Color.white;
        */


    }

    // resets your bomb cooldown.
    IEnumerator ResetBomb(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        bombDebounce = false;
    }

    // makes the player blink while invincible
    IEnumerator InvincibleBlink()
    {
        bool visible = true;
        int totalBlinks = 12; // divide this by two
        float timeBetweenBlinks = iFrames / totalBlinks;

        for (int i = 0; i < totalBlinks; i++)
        {
            yield return new WaitForSeconds(timeBetweenBlinks);
            PlayerVisibility(playerModel.transform, !visible);
            visible = !visible;
        }
    }

    // makes the player invincible for their iFrame time
    IEnumerator PlayerInvincibility()
    {
        isInvincible = true;

        // make them blink
        StartCoroutine(InvincibleBlink());

        yield return new WaitForSeconds(iFrames);
        isInvincible = false;

        // make them visible
        PlayerVisibility(playerModel.transform, true);

    }

    // ---------------------------------- Player Movement -------------------------------------------- // 


    // Moves the player locally, vectors are normalized and speed can be variable on X and Y axes
    // affected by tilting (see above)
    void LocalMove(float x, float y, float xSpeed, float ySpeed)
    {
        Vector3 normalDirection = new Vector3(x, y, 0).normalized;
        float normalYSpeed = normalDirection.y * ySpeed;

        // calculating movement boosts when tilting
        if (x > 0) // moving right
        {
            if (lastTiltSide == "right")
            {
                xSpeed *= tiltSpeedBuff;
            } 
            else if (lastTiltSide == "left")
            {
                xSpeed /= tiltSpeedNerf;
            }
        } 
        else if (x < 0) // moving left
        {
            if (lastTiltSide == "right")
            {
                xSpeed /= tiltSpeedBuff;
            }
            else if (lastTiltSide == "left")
            {
                xSpeed *= tiltSpeedNerf;
            }
        }
        
        float normalXSpeed = normalDirection.x * xSpeed;

        transform.localPosition += new Vector3(normalXSpeed, normalYSpeed, 0) * Time.deltaTime;
        //Vector3 moveDirection = new Vector3(normalXSpeed, normalYSpeed, 0);
        //transform.Translate(moveDirection * Time.deltaTime
    }

    // Clamps the player's position to the camera viewport, they wont ever be able to exceed the viewport's bounds.
    void ClampPosition()
    {
        Vector3 position = Camera.main.WorldToViewportPoint(transform.position);

        //position.x = Mathf.Clamp01(position.x);
        //position.y = Mathf.Clamp01(position.y);

        // slightly less forgiving clamp bounds so that the player always fully remains on screen
        position.x = Mathf.Clamp(position.x, 0.1f, 0.9f);
        position.y = Mathf.Clamp(position.y, 0.1f, 0.9f);

        transform.position = Camera.main.ViewportToWorldPoint(position);
    }

    // Makes the player look in the direction they're currently flying
    void AimLook(float h, float v, float speed)
    {
        // this way allows you to rotate in the same smooth manner without it breaking at 0,0,0
        aimTarget.parent.localPosition = new Vector3(transform.position.x, transform.position.y, 0);
        aimTarget.localPosition = new Vector3(h, v, lookIntensity);

        transform.LookAt(aimTarget);

        // note: below only works if you stay on 0,0,0, if you try to move it'll break and you won't rotate anymore.
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(aimTarget.position), speed * Time.deltaTime);
    }


    // realistically leaning the player horizontally when turning 
    void HorizontalLean(Transform target, float axis, float leanLimit, float lerpTime)
    {
        // only applying this if you're not tilting
        if (lastTiltSide == null)
        {
            Vector3 targetEulerAngles = target.localEulerAngles;
            target.localEulerAngles = new Vector3(targetEulerAngles.x, targetEulerAngles.y, Mathf.LerpAngle(targetEulerAngles.z, -axis * leanLimit, lerpTime));
        }
        
    }


    // visualizing the aim target.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(aimTarget.position, 0.5f);
        Gizmos.DrawSphere(aimTarget.position, 0.15f);
    }



    // ---------------------------------- Player Collision -------------------------------------------- // 

    // colliding with different things
    // note: they have to have IsTrigger set to true
    private void OnTriggerEnter(Collider other)
    {
        if (playerHealth <= 0)
            return;

        // checking the layer
        int otherLayer = other.gameObject.layer;

        // colliding with an obstacle
        if (LayerMask.LayerToName(otherLayer) == "Obstacle")
        {
            // deal damage to the player
            TakeDamage(1);

            // push them away from the obstacle 

            // logic:
            // get the collision position along with the midpoint of the obstacle
            // align the collision point with the midpoint's Z axis to ignore the 3rd dimension
            // get a point further out in the direction away from the midpoint, then raycast on this collider to find the most accurate surface
            // get the surface normal of that surface and apply knockback in that direction

            // note: this sometimes gives strange results like when hitting a surface that's offscreen, but there's not much that can be done about that atm.
            //       im moving on to prevent wasting more time on this, it's good enough for now.

            // getting the closest contact point to extrapolate
            Vector3 otherCenter = other.gameObject.transform.position;
            Vector3 playerZLess = new Vector3(transform.position.x, transform.position.y, otherCenter.z);
            Vector3 contactPos = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            Vector3 zlessContact = new Vector3(contactPos.x, contactPos.y, otherCenter.z);

            Vector3 directionFromCenter = (zlessContact - otherCenter).normalized * 10000f; // getting a position sufficiently far enough

            Vector3 pointAwayFromCenter = new Vector3(directionFromCenter.x, directionFromCenter.y, otherCenter.z); // get a point away from the center
            Vector3 directionTowardsContact = zlessContact - pointAwayFromCenter; // get the direction towards the contact point

            //Debug.DrawRay(pointAwayFromCenter, directionTowardsContact, Color.blue, 100f);

            Ray toCenter = new Ray(pointAwayFromCenter, directionTowardsContact);
            RaycastHit hit;
            Vector3 pushDirection = Vector3.zero;

            if (other.Raycast(toCenter, out hit, Mathf.Infinity))
            {
                pushDirection = hit.normal;
                //print(pushDirection);
            } 

            if (pushDirection != Vector3.zero)
            {
                Vector3 destination = transform.position + (pushDirection * obstacleKBForce);
                Vector3 finalPosition = new Vector3(destination.x, destination.y, 0);
                //print(pushDirection + " " + finalPosition);

                // tweening them to their destination
                transform.DOLocalMove(finalPosition, 0.75f).SetEase(Ease.OutQuint);
            }


            // lose score for the collision
            scoreHandler.ChangePlayerScore("playerObstacle");


            // old code and attempts
            /*

            //Vector3 directionToContact = contactPos - other.transform.position; // pushing you left/right away from the contact point
            //Vector3 directionToObstacle = transform.position - other.transform.position; // pushing you up/down away from the other's center


            float xDotProduct = Vector3.Dot(directionToContact, transform.right);
            float yDotProduct = Vector3.Dot(directionToObstacle, transform.up);
            int xDir, yDir = 0;

            xDir = (xDotProduct > 0) ? 1 : -1;
            yDir = (yDotProduct > 0) ? 1 : -1;

            //Vector3 pushDirection = new Vector3(directionToContact.x, directionToContact.y, 0).normalized;
            //Vector3 newLocation = transform.position + (pushDirection * 10f);


            //transform.localPosition += pushDirection.normalized * 10f;

            //Vector3 zless = new Vector3(oppositeDirection.x, oppositeDirection.y, 0);

            //transform.localPosition += zless.normalized * 100f * Time.deltaTime;


            // if they're above the object, then raycast straight down, if they're blow the object, raycast straight up
            // if they're to the left of the object then raycast right, if they're to the left, raycast right
            Vector3 playerToOther = (zlessContact - playerZLess);
            float upDot = Vector3.Dot(playerToOther, transform.up);
            float rightDot = Vector3.Dot(playerToOther, transform.right);

            int isAbove = (upDot > 0) ? 1 : -1;
            int isRight = (rightDot > 0) ? 1 : -1;

            if (isAbove == -1)
                print("player above contact");
            else
                print("player below contact");

            if (isRight == 1)
                print("player right contact");
            else
                print("player left contact");

            */

        } else if (LayerMask.LayerToName(otherLayer) == "EnemyProjectile")
        {
            GameObject enemyProj = other.gameObject;

            if (doingAileron == true)
            {
                // deflect/parry the projectile
                //print("parried projectile");

                // set its layer to playerprojectile
                enemyProj.layer = LayerMask.NameToLayer("PlayerProjectile");

                // get the projectile's owner
                GameObject projOwner = enemyProj.GetComponent<ProjectileInfo>().projectileOwner;

                Rigidbody rb = enemyProj.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero; // resetting its velocity

                if (perfectParry)
                {
                    // change its color
                    Renderer objectRenderer = other.gameObject.GetComponent<Renderer>();
                    objectRenderer.material = parriedMaterial;

                    // send it back to the enemy who hit it
                    if (projOwner != null)
                    {
                        enemyProj.transform.LookAt(projOwner.transform.position);
                    } else
                    {
                        enemyProj.transform.LookAt(-enemyProj.transform.forward);
                    }

                    // doubling the damage
                    enemyProj.GetComponent<ProjectileInfo>().projectileDamage *= 2;

                    // sending it back where it came from
                    if (rb != null)
                        rb.AddForce(enemyProj.transform.forward * (parrySpeed * 1.5f), ForceMode.Impulse);

                    // fire to the spawn director that they're playing better
                    spawnDirector.ChangeIntensity(true, enemyProj.GetComponent<ProjectileInfo>().projectileDamage);

                    // fire to the score handler that they parried
                    scoreHandler.ChangePlayerScore("parry");

                }
                else
                {
                    // get a random direction
                    Vector3 randomDirection = Random.insideUnitCircle.normalized;
                    randomDirection = new Vector3(randomDirection.x, randomDirection.y, 0);

                    enemyProj.transform.LookAt(randomDirection * parrySpeed);

                    // deflect it in a random direction
                    if (rb != null)
                        rb.AddForce(randomDirection * parrySpeed, ForceMode.Impulse);

                    // gain slight score for a deflection
                    scoreHandler.ChangePlayerScore("deflect");

                }




            }
            else
            {
                // they just take damage, no need to knock them around
                TakeDamage(1);
                Destroy(enemyProj);
            }

            
        }

    }





}
