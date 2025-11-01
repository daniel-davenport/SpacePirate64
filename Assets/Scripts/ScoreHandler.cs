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

    }

    // Update is called once per frame
    void Update()
    {
        UpdateScore();
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
            print(difference + " " + playerScore + " " + scaleSize);
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
