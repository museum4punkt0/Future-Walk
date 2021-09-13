using UnityEngine;
using System.Collections;

public class ParticleGravityChange : MonoBehaviour
{
    public float targetLifeTime;
    public float timeToLerp;
    public GameObject EndParticleSystem;
    
    private AudioSource SparkleSound;
    private AudioSource TargetFoundAudio;

    void Start()
    {
        var aSources = GetComponents<AudioSource>();
        SparkleSound = aSources[0];
        TargetFoundAudio = aSources[1];

        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps)
        {
            // ps.Play();
        }
    }

    public void GravityChange(GameObject g)
    {
        StartCoroutine(LerpFunction(targetLifeTime, timeToLerp));

        Debug.Log("play particle change script");

        ParticleSystem ps = GetComponent<ParticleSystem>();
        var em = ps.emission;
        em.enabled = false;
    }

    IEnumerator LerpFunction(float endValue, float duration)
    {
        float time = 0;

        Debug.Log("coroutineStarted");

        SparkleSound.Play();
        

        //Instantiate(EndParticleSystem);

        while (time < duration)
        {
            var x = Mathf.Lerp(0, 1, time / duration);
            time += Time.deltaTime;
      
            ParticleSystem ps = GetComponent<ParticleSystem>();
            var fo = ps.forceOverLifetime;
            fo.enabled = true;

            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(x, -0.15f);
            curve.AddKey(x + 0.2f, 0f);
            fo.y = new ParticleSystem.MinMaxCurve(1.5f, curve);

            yield return null; 
        }  

    }

}
