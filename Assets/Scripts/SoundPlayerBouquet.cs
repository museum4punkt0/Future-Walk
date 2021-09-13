using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayerBouquet : MonoBehaviour
{
    Animator AnimatorParticleMesh;
    private AudioSourceLanguage PaintingDataIn;
    public GameObject ParticleMesh;

    // Start is called before the first frame update
    void Start()
    {
        AnimatorParticleMesh = ParticleMesh.GetComponent<Animator>();

        PaintingDataIn = GetComponent<AudioSourceLanguage>();
        if (!PaintingDataIn)
        {
            Log.e("no audio source!");
        }
       
        StartCoroutine(InstructionsNoActions());
    }

    private IEnumerator StartMethod(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
 
        StoryChat.instance._AR_success();
    }

    IEnumerator InstructionsNoActions()
    {
        yield return new WaitForSeconds(1);
        AnimatorParticleMesh.SetTrigger("ParticleMeshToPhone");

        yield return new WaitForSeconds(1);
        PaintingDataIn.Play();

        // give it some slack
        yield return new WaitForSeconds(1);
        StartCoroutine(StartMethod(PaintingDataIn.clip.length));
    }
}
