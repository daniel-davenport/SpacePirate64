using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Restart()
    {
        print("restart button pressed");
        SceneManager.LoadScene("Endless");
    }

    public void ReturnToMain()
    {
        print("menu button pressed");
        SceneManager.LoadScene("TitleScreen");
    }

    public void QuitGame()
    {
        print("quit button pressed");
        Application.Quit();
    }
}
