using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public SpawnDirector spawnDirector;
    public LevelDirector levelDirector;

    [Header("Scoring")]
    public int playerScore;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // changing the player's score by X amount
    // allows negatives
    public void ChangePlayerScore(int amount)
    {
        playerScore += amount;
    }



}
