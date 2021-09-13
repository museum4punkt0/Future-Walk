using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class AssetBundleLoadFromToolsTest : MonoBehaviour
{   
    AudioSource audioSource;
    Button button;
    public AudioClip audioClip;

    public string clipName = "AB0";
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(StartPlayingFromBundle);
    }

    void StartPlayingFromBundle()
    {
        AudioController.PlayAudio(clipName);
    }

}
