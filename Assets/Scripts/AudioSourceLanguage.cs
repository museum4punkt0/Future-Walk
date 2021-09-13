using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class AudioSourceLanguage : MonoBehaviour
{
    [SerializeField] AudioClip english;
    [SerializeField] AudioClip german;
    [SerializeField] AudioMixerGroup output;

    AudioSource audioSource;

    public AudioClip clip { 
        get => audioSource.clip;
    }

    public bool isPlaying { 
        get => audioSource.isPlaying;
    }

    void Awake()
    {
        if (!english && !german)
        {
            Log.e("no language file set!");
        }

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
       
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1;
        audioSource.outputAudioMixerGroup = output;      

        ChooseLanguage();
    }

    void ChooseLanguage ()
    {
        if (GlobalSettings.instance == null || GlobalSettings.instance.IsEnglish)
        {
            if (english) audioSource.clip = english;
            else audioSource.clip = german;
        }
        else
        {
            if (german) audioSource.clip = german;
            else audioSource.clip = english;
        }
    }

    public void Play()
    {        
        audioSource.Play();
    }

    public void Pause()
    {
        if (audioSource)
        {
            audioSource.Pause();
        }
    }

    public void Stop()
    {
        if (audioSource)
        {
            audioSource.Stop();
        }
    }

}
