using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Transform playerModel;

    [Header("Settings")]
    public bool usesRawInput = false;

    [Header("Parameters")]
    public float xSpeed = 18;
    public float ySpeed = 18;
    public float lookSpeed = 320;
    public float lookIntensity = 1.5f;

    public float leanLimit = 80;
    public float leanLerpSpeed = 0.1f;

    [Header("References")]
    public Transform aimTarget;
    public GameObject leftWeaponModel;
    public GameObject rightWeaponModel;

    // Weapon Inputs & debounces
    InputAction attackLeftAction;
    InputAction attackRightAction;

    // Arrays that handle charge times and debounces based on weapon
    // note: AttackLeft = slot 0
    //       AttackRight = slot 1
    float[] chargeTimes = new float[] { 0f, 0f }; // Tracks the current charge level
    float[] maxChargeTimes = new float[] { 1f, 1f }; // The charge time needed to fire a charged shot
    GameObject[] weaponModels = new GameObject[2];

    bool[] attackDebounces = new bool[] { false, false };



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // gets the player's model under the hitbox
        playerModel = transform.GetChild(0);

        // getting the player's keybinds
        attackLeftAction = InputSystem.actions.FindAction("AttackLeft");
        attackRightAction = InputSystem.actions.FindAction("AttackRight");

        // getting the player's weapons
        weaponModels[0] = leftWeaponModel;
        weaponModels[1] = rightWeaponModel;
    }

    // Update is called once per frame
    void Update()
    {
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
        HorizontalLean(playerModel, hInput, leanLimit, leanLerpSpeed);


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


    }

    // ---------------------------------- Player Actions -------------------------------------------- // 


    // handles code relating to individual weapon slot debounces and charged attacks
    void AttackStart(int weaponSlot, float chargeTime = 0f)
    {

        if (chargeTime > 0f && chargeTime >= maxChargeTimes[weaponSlot]) // firing a charged shot (ignores debounces)
        {
            Attack(weaponSlot, true);
            chargeTimes[weaponSlot] = 0f;

            Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
            objectRenderer.material.color = Color.white;
            return;
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

            Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
            objectRenderer.material.color = Color.blue;
        } 


        
        // charging up that slot's charged shot
        if (chargeTimes[weaponSlot] >= maxChargeTimes[weaponSlot])
        {
            print(" ----------- fully charged slot " + weaponSlot + " ----------- ");

            Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
            objectRenderer.material.color = Color.red;
            
        }
        else
        {
            chargeTimes[weaponSlot] += Time.deltaTime;
        }
            
    }



    // Fires the specific weapon called, also takes in the argument if it's a charged attack or not
    void Attack(int weaponSlot, bool isCharged)
    {

        if (isCharged)
        {
            print("charged shot: " + weaponSlot);
        }
        else
        {
            print("firing slot: " + weaponSlot);
        }


        // weapon's code should go here




        // reset the weapon's cooldown, this can be checked in other ways later.
        StartCoroutine(ResetAttack(weaponSlot));
    }





    // resets the weapon's slot after X amount of time. 
    // note: cooldown time should be pulled from the weapon's live data, for now it's a default value
    IEnumerator ResetAttack(int weaponSlot)
    {
        // wait time until it comes back
        yield return new WaitForSeconds(1f);
        // print("weapon " + weaponSlot + " reset");

        Renderer objectRenderer = weaponModels[weaponSlot].GetComponent<Renderer>();
        objectRenderer.material.color = Color.white;

        attackDebounces[weaponSlot] = false;
    }




    // ---------------------------------- Player Movement -------------------------------------------- // 


    // Moves the player locally, vectors are normalized and speed can be variable on X and Y axes
    void LocalMove(float x, float y, float xSpeed, float ySpeed)
    {
        Vector3 normalDirection = new Vector3(x, y, 0).normalized;
        float normalXSpeed = normalDirection.x * xSpeed;
        float normalYSpeed = normalDirection.y * ySpeed;

        transform.localPosition += new Vector3(normalXSpeed, normalYSpeed, 0) * Time.deltaTime;
        //Vector3 moveDirection = new Vector3(normalXSpeed, normalYSpeed, 0);
        //transform.Translate(moveDirection * Time.deltaTime
    }


    private void LateUpdate()
    {
        // clamping the player's position
        // note: this has to be in lateupdate
        ClampPosition();
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
        Vector3 targetEulerAngles = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngles.x, targetEulerAngles.y, Mathf.LerpAngle(targetEulerAngles.z, -axis * leanLimit, lerpTime));
    }


    // visualizing the aim target.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(aimTarget.position, 0.5f);
        Gizmos.DrawSphere(aimTarget.position, 0.15f);
    }


}
