using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSourceLanguage))]
public class SoundPlayerNothingHappening : MonoBehaviour
{
    AudioSourceLanguage notFound90;
    AudioSourceLanguage notFound60;
    AudioSourceLanguage notFoundExit;

    void Start()
    {
        var aSources = GetComponents<AudioSourceLanguage>();

        if (aSources.Length >= 3)
        {
            notFound60 = aSources[0];
            notFound90 = aSources[1];
            notFoundExit = aSources[2];
        }
        else
        {
            Log.e("not enought audio sources!");
        }

        StartCoroutine(InstructionsNoActions());
    }

    public void Stop()
    {
        StopAllCoroutines();

        if (notFound90) notFound90.Stop();
        if (notFound60) notFound60.Stop();
        if (notFoundExit) notFoundExit.Stop();
    }

    IEnumerator InstructionsNoActions()
    {
        yield return new WaitForSeconds(60);
        notFound60.Play();

        yield return new WaitForSeconds(30);
        notFound90.Play();

        yield return new WaitForSeconds(30);
        notFoundExit.Play();
    }
}