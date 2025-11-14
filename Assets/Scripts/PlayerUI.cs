using DG.Tweening;
using System.Collections;
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
    public BombScript bombScript;

    // HUD elements
    public GameObject bottomHud;
    public GameObject bombHolder;
    public GameObject bombPip;

    public TextMeshProUGUI bombName;
    public GameObject missileWarningText;

    public GameObject[] hudHolders = new GameObject[2];
    public TextMeshProUGUI[] weaponNames = new TextMeshProUGUI[2];
    public TextMeshProUGUI[] levelTexts = new TextMeshProUGUI[2];
    public Slider[] expSliders = new Slider[2];

    [Header("Stats")]
    private Vector3 healthHolderBaseScale = new Vector3(0.02f, 0.02f, 0.02f); // magic number unfortunately
    private int lastScrapAmount = 0;
    private int lastBombAmount = 0;
    private bool MWSDebounce = false;

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

        // updating the bomb info every frame
        UpdateBombs();
    }


    private void LateUpdate()
    {
        // showing the MWS if there's a missile incoming
        if (playerController.inDanger == true && MWSDebounce == false)
        {
            MWSDebounce = true;
            StartCoroutine(MissileWarning());
        } else if (playerController.inDanger == false)
        {
            MWSDebounce = false;
        }

    }


    // clears the health up
    private void RemoveHealthPip()
    {
        // shrink effect to emphasize getting hurt
        healthHolder.localScale = healthHolderBaseScale / 1.5f;
        healthHolder.DOScale(healthHolderBaseScale, 0.5f);

        foreach(Transform child in healthHolder)
        {
            Destroy(child.gameObject);
        }
    }

    private void RemoveBombPips()
    {
        foreach (Transform child in bombHolder.transform)
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
        // doing a little bounce when your scrap increases
        if (lastScrapAmount < playerController.heldScrap)
        {
            float increaseAmount = 1.25f;
            lastScrapAmount = playerController.heldScrap;
            scrapText.transform.localScale = new Vector3(increaseAmount, increaseAmount, increaseAmount);
            scrapText.transform.DOScale(Vector3.one, 0.5f);
        }

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


    public void UpdateBombs()
    {
        int bombAmount = bombScript.heldBombs;
        
        // updating the bomb pips
        if (lastBombAmount != bombAmount)
        {
            lastBombAmount = bombAmount;

            RemoveBombPips();

            for (int i = 0; i < bombAmount; i++)
            {
                GameObject bp = Instantiate(bombPip, bombHolder.transform);
            }


        }

        // updating the bomb text
        bombName.text = bombScript.bombDisplayName;

    }


    private IEnumerator MissileWarning()
    {

        while (playerController.inDanger == true)
        {
            missileWarningText.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            missileWarningText.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }

        missileWarningText.SetActive(false);
        

    }
}
