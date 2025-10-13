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
    void Update()
    {
        transform.localPosition += (Vector3.forward * moveSpeed * Time.deltaTime);
    }

}
