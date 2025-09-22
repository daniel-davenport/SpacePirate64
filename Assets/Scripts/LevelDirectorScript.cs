using UnityEngine;

public class LevelDirector : MonoBehaviour
{

    [Header("References")]
    public GameObject playerPlane;
    public GameObject StartLine;
    public GameObject FinishLine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCollided()
    {
        print("start line");
        playerPlane.GetComponent<ForwardMovement>().moveSpeed = 30;

    }


    public void FinishCollided()
    {
        print("finish line");
        playerPlane.GetComponent<ForwardMovement>().moveSpeed = 10;

    }


}
