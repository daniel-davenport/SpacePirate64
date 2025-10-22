using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class PickupScript : MonoBehaviour
{
    [Header("Info")]
    public int heldValue;
    private float moveSpeed = 55f;
    private float disappearDistance = 1f;

    public Vector2 limits;
    public Vector2 limitsMult;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(RemoveCollision());   
    }

    // Update is called once per frame
    void Update()
    {
        // making sure it stays clamped
        ClampPosition();
    }

    // on collision, destroy it, the weaponhandler can handle the rest

    // make it a trigger so that it can't hit anything anymore
    private IEnumerator RemoveCollision()
    {
        yield return new WaitForSeconds(0.25f);
        transform.GetComponent<Collider>().isTrigger = true;
    }

    private void ClampPosition()
    {
        Vector3 position = transform.position;

        // hardcoded limits mimicked from the playercontroller
        float xLimit = limits.x * limitsMult.x;
        float yLimit = limits.y * limitsMult.y;

        position.x = Mathf.Clamp(position.x, -xLimit, xLimit);
        position.y = Mathf.Clamp(position.y, -yLimit, yLimit);

        transform.position = new Vector3(position.x, position.y, position.z);
    }

    public void CollectItem(GameObject player)
    {
        StartCoroutine(MoveToPlayer(player));
    }

    // moves the item towards the player then deletes it
    private IEnumerator MoveToPlayer(GameObject player)
    {
        float distance = math.INFINITY;

        while (distance >= disappearDistance)
        {
            // move it towards the player
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            distance = Vector3.Distance(player.transform.position, transform.position);

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }


}
