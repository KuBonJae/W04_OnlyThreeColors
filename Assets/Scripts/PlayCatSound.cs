using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCatSound : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayCatSounds()
    {
        audioSource.Play();
    }
}
