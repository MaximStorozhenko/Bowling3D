using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private AudioSource strikeSound;
    private AudioSource strikeSound2;

    void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        strikeSound  = audioSources[0];
        strikeSound2 = audioSources[1];
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name.StartsWith("Kegel"))
        {
            strikeSound.Play();
        }
        else if(other.name.StartsWith("Cube"))
        {
            strikeSound2.Play();
        }
    }
}
