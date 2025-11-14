using Unity.Mathematics;
using UnityEngine;

public class SpriteSpin : MonoBehaviour
{
    // Makes an object spin based on a specified axis
    public float spinSpeed;
    public Vector3 spinAmount; // each axis should be 1 for spin, 0 for no spin

    public Quaternion startRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {


        /*
        // rotating it
        if (gameObject.TryGetComponent(out Renderer renderer))
        {
            gameObject.transform.RotateAround(renderer.bounds.center, spinAmount, spinSpeed * Time.deltaTime);
        }
        */
        
        transform.Rotate(spinAmount * spinSpeed * Time.deltaTime, Space.Self);
    }
}
