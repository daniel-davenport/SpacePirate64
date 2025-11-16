using UnityEngine;

public class LockOnBillboard : MonoBehaviour
{

    [SerializeField]
    Camera mainCamera;
    public GameObject followingObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);

        if (followingObject != null)
        {
            transform.position = followingObject.transform.position;
        }
    }
}
