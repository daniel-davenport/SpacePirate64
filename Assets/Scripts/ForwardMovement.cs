using UnityEngine;

public class ForwardMovement : MonoBehaviour
{
    [Header("Settings")]
    

    [Header("Parameters")]
    public float moveSpeed = 1;

    [Header("References")]
    public GameObject player;
    public LevelDirector levelDirector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveSpeed = levelDirector.outLevelSpeed;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // note: later maybe do stuff the other way around so the player doesnt jitter so much
        // lerping this
        Vector3 targetPos = ((transform.position + (Vector3.forward * moveSpeed * Time.deltaTime)));
        transform.localPosition = Vector3.Lerp(transform.position, targetPos, moveSpeed);
    }

}
