using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAndAnnimationPlayer_MariaUndJesse : MonoBehaviour
{
    Animator AnimatorParticleMesh;
    private AudioSourceLanguage PaintingDataIn;
    private AudioSourceLanguage TargetNotFound;
    private AudioSourceLanguage ParticleFlowStart;

    public GameObject ParticleMesh;
    public GameObject ParticleFlow;

    // Start is called before the first frame update
    void Start()
    {
        AnimatorParticleMesh = ParticleMesh.GetComponent<Animator>();

        var aSources = GetComponents<AudioSourceLanguage>();
        PaintingDataIn = aSources[0];
        TargetNotFound = aSources[1];
        ParticleFlowStart = aSources[2];

        StartCoroutine(InstructionsNoActions());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator InstructionsNoActions()
    {
        AnimatorParticleMesh.SetTrigger("ParticleMeshToPhone");

        yield return new WaitForSeconds(1);
        PaintingDataIn.Play();

        yield return new WaitForSeconds(8);
        ParticleFlow.SetActive(true);

        yield return new WaitForSeconds(2);
        ParticleFlowStart.Play();

        yield return new WaitForSeconds(118);
        TargetNotFound.Play();
    }
}
