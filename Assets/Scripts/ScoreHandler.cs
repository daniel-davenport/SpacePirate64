using JetBrains.Annotations;
using System.IO;
using TMPro;
using UnityEngine;
using static SpawnDirector;

public class ScoreHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public SpawnDirector spawnDirector;
    public LevelDirector levelDirector;
    public GameObject playerUI;
    public TextMeshProUGUI playerScoreText;

    [Header("Scoring")]
    public int playerScore;

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

        GameObject scoreObject = playerUI.transform.Find("ScoreText").gameObject;

        if (scoreObject != null)
            playerScoreText = scoreObject.GetComponent<TextMeshProUGUI>();


        playerScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
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

            string displayText = mod + allStyleList.styles[index].displayName;

            playerScore += allStyleList.styles[index].amount;

            //print(displayText);

            playerScoreText.text = playerScore.ToString();

            

        }

        //playerScore += amount;
    }



}
