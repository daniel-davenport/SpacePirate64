using UnityEngine;

public class PlayerFor : MonoBehaviour
{
    [Header("Settings")]
    

    [Header("Parameters")]
    public float moveSpeed = 1;

    [Header("References")]
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition += (Vector3.forward * moveSpeed * Time.deltaTime);
    }

}
