using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class TitleScreenScript : MonoBehaviour
{
    private Camera mainCam;
    private bool transitioning = false;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public Transform cameraFinalPos;
    public Transform playerPosition;

    private Color baseColor;
    private Color transColor;

    public float timePerChar = 0.05f;
    public float fadeTime = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;

        StartCoroutine(TypewriterText("- SPACE PIRATE 64 -"));

        subtitleText.overrideColorTags = true;
        baseColor = subtitleText.color;

        transColor = baseColor;
        transColor.a = 0;

        subtitleText.color = transColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (transitioning)
            mainCam.transform.LookAt(playerPosition);
    }

    private void SubtitleTrans(float fadeTime)
    {
        float lerp = 0;
        DOTween.To(() => lerp, x => lerp = x, 1, fadeTime).SetEase(Ease.OutQuad).OnUpdate(() =>
        {
            transColor.a = lerp;
            subtitleText.color = transColor;
        });
    }


    // typerwriter effect for the title screen
    IEnumerator TypewriterText(string message)
    {
        titleText.text = "";

        foreach (char c in message)
        {
            titleText.text += c;

            if (c != ' ') 
                yield return new WaitForSeconds(timePerChar);
        }

        SubtitleTrans(fadeTime);
    }

    public void StartGame()
    {
        print("start button pressed");

        // do a camera transition to get behind the player before doing this
        Camera mainCam = Camera.main;
        transitioning = true;

        // transition the skybox into another one

        mainCam.transform.DOMove(cameraFinalPos.position, 2f).OnComplete(() => 
        {
            SceneManager.LoadScene("Endless");
        });

        

        
    }

    public void QuitGame()
    {
        print("quit button pressed");
        Application.Quit();
    }
}
