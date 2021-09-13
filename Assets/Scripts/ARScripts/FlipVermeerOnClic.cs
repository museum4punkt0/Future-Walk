using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipVermeerOnClic : MonoBehaviour
{
    Animator AnimatorFlip;
    Animator AnimatorFadeXRay;
    Animator AnimatorParticleMesh;

    private AudioSourceLanguage FullAudio;

    public GameObject XRayGO;
    public GameObject ParticleMesh;

    public Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        AnimatorFlip = gameObject.GetComponent<Animator>();
        AnimatorFadeXRay = XRayGO.GetComponent<Animator>();
        AnimatorParticleMesh = ParticleMesh.GetComponent<Animator>();

        var aSources = GetComponents<AudioSourceLanguage>();
        FullAudio = aSources[0];

        //rend = GetComponent<Renderer>();
        rend.enabled = false;
        
        StartCoroutine(InstructionsNoActions());
    }

    // Update is called once per frame
    void Update()
    {
       /* if (Input.GetMouseButtonDown(1))
        {
            AnimatorFlip.SetTrigger("VermeerTriger");
            Debug.Log("keyDown");
        }*/
    }

    IEnumerator InstructionsNoActions()
    {
        yield return new WaitForSeconds(3);
        FullAudio.Play();
        AnimatorParticleMesh.SetTrigger("ParticleMeshToPhone");

        yield return new WaitForSeconds(14);
        AnimatorFadeXRay.SetTrigger("VermeerFadeXRay");

        
        yield return new WaitForSeconds(40);
        rend.enabled = true;
        AnimatorFlip.SetTrigger("VermeerTriger");

    }
}
