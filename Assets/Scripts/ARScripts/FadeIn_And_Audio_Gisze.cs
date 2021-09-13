using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeIn_And_Audio_Gisze : MonoBehaviour
{
    Animator AnimatorFadeXRay;
    Animator AnimatorParticleMesh;

    private AudioSourceLanguage PaintingDataIn;
    private AudioSourceLanguage XRay;
    private AudioSourceLanguage TakeALook;

    float dataInLength;
    float xrayLength;
    float takeAlookLength;

    public GameObject ParticleMesh;

    // Start is called before the first frame update
    void Start()
    {
        AnimatorFadeXRay = gameObject.GetComponent<Animator>();
        AnimatorParticleMesh = ParticleMesh.GetComponent<Animator>();

        var aSources = GetComponents<AudioSourceLanguage>();

        PaintingDataIn = aSources[0];
        dataInLength = PaintingDataIn.clip.length;

        XRay = aSources[1];
        xrayLength = XRay.clip.length;

        TakeALook = aSources[2];
        takeAlookLength = TakeALook.clip.length;

        StartCoroutine(InstructionsNoActions());
    }

    private void startXRayFadeIn()
    {
        if (AnimatorFadeXRay)
        {
            AnimatorFadeXRay.SetTrigger("FadeIn_Gisze");
        }
        else
        {
            Log.d("no AnimatorFadeXRay");
        }
    }

    private void startParticleToPhone()
    {
        if (AnimatorParticleMesh)
        {
            AnimatorParticleMesh.SetTrigger("ParticleMeshToPhone");
        }
        else
        {
            Log.d("no AnimatorParticleMesh");
        }
    }

    IEnumerator InstructionsNoActions()
    {
        startParticleToPhone();
        yield return new WaitForSeconds(2); //2 dataInLength + 1.5f
        
        PaintingDataIn.Play();
        yield return new WaitForSeconds(dataInLength + 1.5f); //7
        
        startXRayFadeIn();
        yield return new WaitForSeconds(1f);

        XRay.Play();
        yield return new WaitForSeconds(xrayLength + 1.5f); //3

        TakeALook.Play();
        yield return new WaitForSeconds(takeAlookLength + 1.5f); //33
    }
}
