using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{

    [Header("References")]
    public PlayerController playerController;
    public GameObject FinalScoreHolder;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI playerHighScoreText;
    public GameObject ScoreTitle;
    public GameObject ScoreText;

    private int highScore;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // high scores
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        playerHighScoreText.text = highScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void GibPlayer(Transform player)
    {
        Transform playerModel = playerController.playerModel.transform;

        foreach(Transform child in playerModel)
        {
            // add a rigidbody and launch it in a random direction
            Rigidbody rb = child.AddComponent<Rigidbody>();

            //rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.AddForce(Random.onUnitSphere * 30f, ForceMode.Impulse);

            // spinning it in a random direction too
            float maxAngularVelocity = 10f;

            Vector3 randomAngularVelocity = new Vector3(
                Random.Range(-maxAngularVelocity, maxAngularVelocity),
                Random.Range(-maxAngularVelocity, maxAngularVelocity),
                Random.Range(-maxAngularVelocity, maxAngularVelocity)
            );
            rb.angularVelocity = randomAngularVelocity;

            // clearing the gib'd parts
            Destroy(child.gameObject, 15f);
        }

    }


    // creates a series of explosions on their body before one big explosion
    public void PlayerDeath(Transform player)
    {
        StartCoroutine(ExplosionLoop(player, 3, 10));
    }

    private IEnumerator FinalScore()
    {
        // disabling some aspects and enabling others
        ScoreTitle.SetActive(false);
        ScoreText.SetActive(false);
        FinalScoreHolder.SetActive(true);

        yield return new WaitForSeconds(1);

        float finalScore = int.Parse(ScoreText.GetComponent<TextMeshProUGUI>().text);
        float showingScore = 0;
        int scoreIncrease = 5;

        // updating the high score
        if (finalScore > highScore)
        {
            highScore = (int)finalScore;

            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // making it go faster if your score is really high
        if (finalScore >= 3000)
        {
            scoreIncrease = 50;
        }

        while (showingScore < finalScore)
        {
            showingScore += scoreIncrease;
            showingScore = Mathf.Clamp(showingScore, 0f, finalScore);
            finalScoreText.text = showingScore + "";

            yield return null;
        }

        finalScoreText.text = finalScore + "";
        playerHighScoreText.text = highScore.ToString();

    }

    private IEnumerator ExplosionLoop(Transform player, float duration, int amount)
    {
        GameObject explosionRef = Resources.Load<GameObject>("Particles/explosion");

        float timeBetween = duration / amount;

        // multiple random explosions
        for (int i = 0; i < amount; i++)
        {
            GameObject spawnedParticle = Instantiate(explosionRef, player);

            float bounds = 1f;

            // getting a random offset
            float randomX = Random.Range(-bounds, bounds);
            float randomY = Random.Range(-bounds / 3, bounds / 3);
            float randomZ = Random.Range(-bounds, bounds);

            spawnedParticle.transform.localPosition = new Vector3(randomX, randomY, randomZ);
            spawnedParticle.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-360, 360));
            spawnedParticle.transform.localScale = new Vector3(2, 2, 2);

            playerController.sfxScript.PlaySFX("DeathExplosion");

            yield return new WaitForSeconds(timeBetween);
        }


        // final big explosion
        GameObject bigBoom = Instantiate(explosionRef, player);
        bigBoom.transform.localPosition = Vector3.zero;

        GibPlayer(player);

        playerController.sfxScript.PlaySFX("BiggerBoom");
        playerController.sfxScript.PlaySFX("PlayerDeath", true);

        // showing the final stuff
        StartCoroutine(FinalScore());

    }



    // creates a particle at a given position based on the name
    // optionally allows you to parent it to an object so it follows it
    public void CreateParticle(Vector3 position, string particleName, Transform parent = null)
    {

        GameObject particleRef = Resources.Load<GameObject>("Particles/" + particleName);

        if (particleRef != null)
        {
            if (parent == null)
            {
                GameObject spawnedParticle = Instantiate(particleRef);
                spawnedParticle.transform.position = position;
            }
            else
            {
                GameObject spawnedParticle = Instantiate(particleRef, position, Quaternion.identity, parent);
                spawnedParticle.transform.localPosition = position;
            }
        }


    }
}
