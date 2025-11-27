using UnityEngine;
using UnityEngine.InputSystem;

public class QuitChecker : MonoBehaviour
{
    public static QuitChecker Instance;

    InputAction quitAction;
    public int targetFPS = 60;


    private void Awake()
    {
        // preventing duplication
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this); // prevents this object from being destroyed when changing scenes
            quitAction = InputSystem.actions.FindAction("Quit");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // setting the target framerate
        Application.targetFrameRate = targetFPS;

    }


    // Update is called once per frame
    void Update()
    {
        if (quitAction.WasPressedThisFrame())
        {
            // quit the game
            print("quit button pressed");
            Application.Quit();
        }
    }
}
