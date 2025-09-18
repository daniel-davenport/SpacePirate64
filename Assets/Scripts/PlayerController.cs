using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform playerModel;

    [Header("Settings")]
    public bool usesRawInput = false;

    [Header("Parameters")]
    public float xSpeed = 18;
    public float ySpeed = 18;
    public float lookSpeed = 320;

    public float leanLimit = 80;
    public float leanLerpSpeed = 0.1f;

    [Header("References")]
    public Transform aimTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // gets the player's model under the hitbox
        playerModel = transform.GetChild(0);
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


    }


    // Moves the player locally, vectors are normalized and speed can be variable on X and Y axes
    void LocalMove(float x, float y, float xSpeed, float ySpeed)
    {
        Vector3 normalDirection = new Vector3(x, y, 0).normalized;
        float normalXSpeed = normalDirection.x * xSpeed;
        float normalYSpeed = normalDirection.y * ySpeed;

        transform.localPosition += new Vector3(normalXSpeed, normalYSpeed, 0) * Time.deltaTime;
        //Vector3 moveDirection = new Vector3(normalXSpeed, normalYSpeed, 0);
        //transform.Translate(moveDirection * Time.deltaTime);

        // clamping the player's position
        ClampPosition();

    }


    // Clamps the player's position to the camera viewport, they wont ever be able to exceed the viewport's bounds.
    void ClampPosition()
    {
        Vector3 position = Camera.main.WorldToViewportPoint(transform.position);

        position.x = Mathf.Clamp01(position.x);
        position.y = Mathf.Clamp01(position.y);

        transform.position = Camera.main.ViewportToWorldPoint(position);
    }

    // Makes the player look in the direction they're currently flying
    void AimLook(float h, float v, float speed)
    {
        aimTarget.parent.localPosition = Vector3.zero;
        aimTarget.localPosition = new Vector3(h, v, 1);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(aimTarget.position), speed * Time.deltaTime);

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
