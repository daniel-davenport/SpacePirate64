using System.Collections;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    [Header("Info")]
    public int expValue;
    public int scrapValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(RemoveCollision());   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // on collision, destroy it, the weaponhandler can handle the rest

    // make it a trigger so that it can't hit anything anymore
    private IEnumerator RemoveCollision()
    {
        yield return new WaitForSeconds(0.25f);
        transform.GetComponent<Collider>().isTrigger = true;
    }
}
