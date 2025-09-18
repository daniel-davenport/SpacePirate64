using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float xSpeed = 18;
    public float ySpeed = 18;
    public float lookSpeed = 340;

    public Transform aimTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        LocalMove(hInput, vInput, xSpeed, ySpeed);
        AimLook(hInput, vInput, lookSpeed);
    }


    void LocalMove(float x, float y, float xSpeed, float ySpeed)
    {
        Vector3 normalDirection = new Vector3(x, y, 0).normalized;
        float normalXSpeed = normalDirection.x * xSpeed;
        float normalYSpeed = normalDirection.y * ySpeed;

        //transform.localPosition += new Vector3(normalXSpeed, normalYSpeed, 0) * Time.deltaTime;
        Vector3 moveDirection = new Vector3(normalXSpeed, normalYSpeed, 0);
        transform.Translate(moveDirection * Time.deltaTime);

    }

    void AimLook(float h, float v, float speed)
    {
        aimTarget.parent.position = Vector3.zero;
        aimTarget.localPosition = new Vector3(h, v, 1);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(aimTarget.position), Mathf.Deg2Rad * speed * Time.deltaTime);

    }

}
