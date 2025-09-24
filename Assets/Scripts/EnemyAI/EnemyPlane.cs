using UnityEngine;

public class EnemyPlane : MonoBehaviour
{
    [Header("Settings")]
    public float forwardDistance;
    public float lerpSpeed;

    [Header("References")]
    public GameObject playerPlane;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        // the EnemyPlane is constantly in front of the player,
        // occasionally being slightly closer or further depending on the player's speed
        MovePlane();
    }

    private void MovePlane()
    {
        Vector3 playerPosition = playerPlane.transform.position;
        Vector3 zOffset = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z + forwardDistance);

        float lerpTime = 1f - Mathf.Pow(1f - lerpSpeed, Time.deltaTime * 30f);

        transform.position = Vector3.Lerp(transform.position, zOffset, lerpTime);
    }



}
