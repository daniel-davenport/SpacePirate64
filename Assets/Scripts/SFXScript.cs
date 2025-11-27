using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SFXScript : MonoBehaviour
{

    public AudioSource SFXSource;
    public List<AudioClip> SFXList;
    public float pitchVariance = 0.05f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // loading all SFX from the SFX folder
        AudioClip[] loadedSFX = Resources.LoadAll<AudioClip>("Audio/SFX");
        SFXList = new List<AudioClip>(loadedSFX);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlaySFX(string sfxName, bool noVariance = false)
    {
        bool foundSFX = false;

        foreach (AudioClip sfx in SFXList)
        {
            if (sfx.name == sfxName)
            {
                //print(sfx.name + " found.");
                foundSFX = true;

                if (noVariance == false)
                {
                    // adding pitch variance
                    float randomPitch = Random.Range(1f - pitchVariance, 1f + pitchVariance);
                    SFXSource.pitch = randomPitch;
                } 
                else
                {
                    SFXSource.pitch = 1;
                }

                    SFXSource.PlayOneShot(sfx);
                break;
            }

        }

        // displaying an error
        if (foundSFX == false)
            print("sfx not found with name: " + sfxName);
    }

}
