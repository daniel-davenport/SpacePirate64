using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [Header("References")]
    public PlayerController playerController;
    public WeaponHandler weaponHandler;
    public ScoreHandler scoreHandler;
    public GameObject playerUI;
    public Transform healthHolder;
    public GameObject healthPip;
    public GameObject playerReticle;
    public TextMeshProUGUI scrapText;

    // HUD elements
    public GameObject bottomHud;
    public GameObject[] hudHolders = new GameObject[2];
    public TextMeshProUGUI[] weaponNames = new TextMeshProUGUI[2];
    public TextMeshProUGUI[] levelTexts = new TextMeshProUGUI[2];
    public Slider[] expSliders = new Slider[2];


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform playerPlane = transform.parent.transform.Find("PlayerPlane");
        Transform player = playerPlane.transform.Find("Player");

        playerController = player.GetComponent<PlayerController>();
        scoreHandler = player.GetComponent<ScoreHandler>();

        for (int i = 0; i < 2; i++)
        {
            Transform levelText = hudHolders[i].transform.Find("LevelText");

            if (levelText != null)
            {
                levelTexts[i] = levelText.GetComponent<TextMeshProUGUI>();
            }

        }

    }

    // Update is called once per frame
    void Update()
    {
        // updating the scrap every frame
        UpdateScrap();

        // updating the health every frame
        UpdateHealth(playerController.playerHealth);

        // updating the weapon levels every frame
        UpdateLevel();
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


    // updates the level text and progress bars
    public void UpdateLevel()
    {
        for (int i = 0; i <= 1; i++)
        {
            WeaponInfo info = weaponHandler.weaponInfoArr[i];

            if (info != null)
            {
                // getting the ratio of weapon exp and applying it to the slider
                float expRatio = (float)weaponHandler.weaponEXP[i] / info.maxEXP;
                //expSliders[i].value = expRatio;
                // tweening the value cuz it looks nicer
                expSliders[i].DOValue(expRatio, 0.25f);

                // changing the text to reflect the level
                levelTexts[i].text = "LV " + weaponHandler.weaponLevels[i].ToString();
                weaponNames[i].text = weaponHandler.weaponInfoArr[i].weaponDisplayName;

            }
        }
    }

}
