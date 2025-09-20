using UnityEngine;

public class LevelEventCollision : MonoBehaviour
{
    public GameObject levelDirector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelDirector = transform.parent.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (transform.gameObject.name == "StartLine")
        {
            levelDirector.GetComponent<LevelDirector>().StartCollided();
        } 
        else if (transform.gameObject.name == "FinishLine")
        {
            levelDirector.GetComponent<LevelDirector>().FinishCollided();
        }
        
    }

}
