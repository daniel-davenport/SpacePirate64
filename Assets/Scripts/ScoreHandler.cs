using JetBrains.Annotations;
using System.IO;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ScoreHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public SpawnDirector spawnDirector;
    public LevelDirector levelDirector;
    public GameObject playerUI;
    public TextMeshProUGUI playerScoreText;
    public GameObject styleGrid;
    public GameObject scoreText;
    public GameObject scoreTitle;

    [Header("Scoring")]
    public float playerScore;
    private float currentScore;
    public int maxScoreDifference;

    [Header("Style List")]
    public TextAsset styleListJson;

    // setting up enemy .json file
    [System.Serializable]
    public class Style
    {
        public string name;
        public string displayName;
        public int amount;
    }

    [System.Serializable]
    public class StyleList
    {
        public Style[] styles;
    }

    [SerializeField]
    public StyleList allStyleList = new StyleList();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // reading style info from the physical json instead of a compiled one
        string styleFilePath = Path.Combine(Application.streamingAssetsPath, "StyleData.json");
        if (File.Exists(styleFilePath))
        {
            string fileContent = File.ReadAllText(styleFilePath);
            styleListJson = new TextAsset(fileContent);

            allStyleList = JsonUtility.FromJson<StyleList>(styleListJson.text);
        }
        else
        {
            print("ERROR: STYLE DATA FILE NOT FOUND.");
        }

        /*
        GameObject scoreObject = scoreText.gameObject;

        if (scoreObject != null)
            playerScoreText = scoreObject.GetComponent<TextMeshProUGUI>();
        */

        playerScore = 0;

        // heartbeat effect
        StartCoroutine(ScoreBeat());

    }

    // Update is called once per frame
    void Update()
    {
        UpdateScore();
    }

    // cool visual effect to give the score meter a heartbeat effect based on current intensity
    private IEnumerator ScoreBeat()
    {
        while (playerController.playerHealth > 0)
        {
            if (spawnDirector == null)
                break;

            float intensity = spawnDirector.intensity;
            float maxIntensity = spawnDirector.maxIntensity;
            float ratio = (intensity / maxIntensity);

            if (scoreTitle != null)
            {
                //float ratio = (((intensity / maxIntensity) + 1) / 2) - 0.5f; // gets it on a scale of 0-0.5
                float maxIncrease = 0.2f;
                float beatSize = (ratio * maxIncrease) + 1; // getting the increase ratio
                beatSize = Mathf.Clamp(beatSize, 1f, maxIncrease + 1);
                //print(beatSize);

                scoreTitle.transform.localScale = new Vector3(beatSize, beatSize, beatSize);
                scoreTitle.transform.DOScale(Vector3.one, 0.5f);
            }

            float fastestBeat = 0.5f;
            float beatTime = 1.25f - (fastestBeat * ratio);

            yield return new WaitForSeconds(beatTime);
        }

    }

    private void UpdateScore()
    {
        if (currentScore != playerScore)
        {
            // make it tick up faster if the difference is too great
            float difference = Mathf.Abs(playerScore - currentScore);
            float ratio = currentScore / playerScore;

            int negate = 1;

            // allowing the function to tick down
            if (currentScore > playerScore)
                negate = -1;

            //currentScore += 1;
            currentScore += negate * (Mathf.Round(1 / ratio));
            currentScore = Mathf.Clamp(currentScore, 0f, playerScore);
            //print(currentScore);

            playerScoreText.text = currentScore.ToString();
        }
    }

    public int FindStyleIndex(string styleName)
    {
        int index = -1;

        for (int i = 0; i < allStyleList.styles.Length; i++)
        {
            if (allStyleList.styles[i].name == styleName) 
            { 
                index = i; 
                break; 
            }
        }

        return index;
    }

    // changing the player's score by reading a json file and seeing if there's an applicable score index for it.
    public void ChangePlayerScore(string styleName)
    {
        int index = FindStyleIndex(styleName);

        // greater than 0 means it was found
        if (index >= 0)
        {

            string mod = "+";

            if (allStyleList.styles[index].amount < 0)
                mod = "-";

            string displayText = " " + mod + allStyleList.styles[index].displayName;

            playerScore += allStyleList.styles[index].amount;

            //print(displayText);

            // instantiate ScoreText and change the text to match displayText
            GameObject newScore = Instantiate(scoreText, styleGrid.transform);

            // scaling effect
            newScore.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            newScore.transform.DOScale(Vector3.one, 0.5f);

            // scaling the total score too 
            float difference = playerScore - currentScore;
            float scaleSize = 1 + (difference / maxScoreDifference); // any score diff greater than this is clamped
            //print(difference + " " + playerScore + " " + scaleSize);
            scaleSize = Mathf.Clamp(scaleSize, 0, 2);

            

            playerScoreText.transform.localScale = new Vector3(scaleSize, scaleSize, scaleSize);
            playerScoreText.transform.DOScale(Vector3.one, 1f);


            newScore.GetComponent<TextMeshProUGUI>().text = displayText;

            // then distroy it after 3 seconds or so
            Destroy(newScore, 3.5f);

        }

        //playerScore += amount;
    }



}
