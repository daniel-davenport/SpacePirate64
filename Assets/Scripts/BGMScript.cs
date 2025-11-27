using UnityEngine;

public class BGMScript : MonoBehaviour
{
    public AudioSource bgmIntro;
    public AudioSource bgmLoop;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // plays the intro before transitioning into the loop
        bgmIntro.Play();
        bgmLoop.PlayDelayed(bgmIntro.clip.length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
