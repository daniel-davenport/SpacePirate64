using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{

    [Header("References")]
    public PlayerController playerController;
    public ScoreHandler scoreHandler;
    public GameObject playerUI;
    public Transform healthHolder;
    public GameObject healthPip;
    public GameObject playerReticle;
    public TextMeshProUGUI scrapText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        scoreHandler = gameObject.GetComponent<ScoreHandler>();

    }

    // Update is called once per frame
    void Update()
    {
        // updating the scrap every frame
        UpdateScrap();

        // updating the health every frame
        UpdateHealth(playerController.playerHealth);
    }


    // clears the health up
    private void RemoveHealthPip()
    {
        foreach(Transform child in healthHolder)
        {
            Destroy(child.gameObject);
        }
    }

    // sets the health bars based on current health
    public void UpdateHealth(int health)
    {
        if (healthHolder.transform.childCount != health)
        {
            RemoveHealthPip();

            if (health > 0)
            {
                for (int i = 0; i < health; i++)
                {
                    GameObject hp = Instantiate(healthPip, healthHolder);
                }
            }
            else
            {
                // hiding the reticle when you die
                playerReticle.SetActive(false);
            }
        }

    }


    // updating the scrap UI
    public void UpdateScrap()
    {
        scrapText.text = playerController.heldScrap.ToString();
    }

}
