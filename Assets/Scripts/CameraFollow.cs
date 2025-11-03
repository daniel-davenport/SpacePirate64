using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [Header("Target")]
    public Transform target;

    public Vector3 offset = Vector3.zero;

    public Vector2 limits = new Vector2(4, 3);
    public Vector2 limitMult = new Vector2(4, 3);

    [Header("Smooth Damp Time")]
    [Range(0, 1)]
    public float smoothTime;

    private Vector3 velocity = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
            transform.localPosition = offset;
        }

        FollowTarget(target);
    }


    void LateUpdate()
    {
        Vector3 localPos = transform.localPosition;

        // clamps the camera based on the defined limits above
        transform.localPosition = new Vector3(Mathf.Clamp(localPos.x, -limits.x, limits.x), Mathf.Clamp(localPos.y, -limits.y, limits.y), localPos.z);
    }


    // causes the camera to smoothly follow the player based on defined limits
    public void FollowTarget(Transform target)
    {
        Vector3 localPos = transform.localPosition;
        Vector3 targetLocalPos = target.transform.localPosition;
        Vector3 targetWorldPos = target.transform.position;

        //transform.localPosition = Vector3.SmoothDamp(localPos, new Vector3(targetLocalPos.x + offset.x, targetLocalPos.y + offset.y, targetWorldPos.z + offset.z), ref velocity, smoothTime);
        //transform.localPosition = Vector3.Lerp(localPos, new Vector3(targetLocalPos.x + offset.x, targetLocalPos.y + offset.y, targetWorldPos.z + offset.z), smoothTime);
        transform.localPosition = Vector3.Lerp(localPos, new Vector3(targetLocalPos.x + offset.x, targetLocalPos.y + offset.y, localPos.z), smoothTime);

    }


    // visualizing the camera's bounds
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-limits.x, -limits.y, transform.position.z), new Vector3(limits.x, -limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(-limits.x, limits.y, transform.position.z), new Vector3(limits.x, limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(-limits.x, -limits.y, transform.position.z), new Vector3(-limits.x, limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(limits.x, -limits.y, transform.position.z), new Vector3(limits.x, limits.y, transform.position.z));
    }

}
