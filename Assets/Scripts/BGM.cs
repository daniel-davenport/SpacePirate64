using UnityEngine;

public class BGM : MonoBehaviour
{
    public static BGM Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton: only one BGM survives
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // kill duplicates in other scenes
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // keep this object when switching scenes

        audioSource = GetComponent<AudioSource>();

        // If not already playing, start the music
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
