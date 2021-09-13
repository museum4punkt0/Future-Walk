using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARAudioHints : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] ARImageClueHolder imgHolder;
    [SerializeField] ARTextClue textHolder;
    [SerializeField] float fadeTime = 1.0f;

    [Header("Hints")]
    [SerializeField] Sprite imgHint;
    [SerializeField] Sprite imgHintVermeer;
    [SerializeField] AudioSourceLanguage[] audioclips;
    [SerializeField] int [] waitingTimes;

    float aspectRatio = 1;
    AudioSourceLanguage currentAudio;

    void Awake()
    {
        if (imgHint)
        {
            aspectRatio = (float)imgHint.rect.width / (float)imgHint.rect.height;
        }
    }

    void Start()
    {
        int clue = 0;
        StartCoroutine(PlayHints(clue));
    }

    IEnumerator PlayHints (int clue)
    {
        //Log.d("clue is: " + clue + " - waiting for: " + waitingTimes[clue] + " seconds");

        yield return new WaitForSeconds(waitingTimes[clue]);

        currentAudio = audioclips[clue];
        currentAudio.Play();
        
        if (clue == 1 && imgHolder && imgHintVermeer != null)
        {
            yield return new WaitForSeconds(currentAudio.clip.length);
            imgHolder.gameObject.SetActive(true);
            imgHolder.GetComponent<Image>().sprite = imgHintVermeer;
            imgHolder.FadeImage(fadeTime,true,aspectRatio);

            //yield return new WaitForSeconds(currentAudio.clip.length);
            
        }

        // if clue 2: image
        if (clue == 2)
        {
            //Log.d("activating image");
            if (imgHolder)
            {
                if(imgHintVermeer != null)
                {
                    imgHolder.FadeImage(fadeTime,false, aspectRatio);
                    imgHolder.DelayedSetActive(false,fadeTime);
                }
                yield return new WaitForSeconds(currentAudio.clip.length);

                imgHolder.gameObject.SetActive(true);
                imgHolder.GetComponent<Image>().sprite = imgHint;
                imgHolder.FadeImage(fadeTime,true,aspectRatio);
            }
        }

        // if clue 3: cover the cam and show text
        if (clue == 3)
        {
            imgHolder.FadeImage(fadeTime,false, aspectRatio);
            imgHolder.DelayedSetActive(false,fadeTime);
            // yield return new WaitForSeconds(fadeTime);
            // imgHolder.gameObject.SetActive(false);
            //Log.d("showing not working text");
            textHolder.FailAnimation();
        }


        clue++;

        if (clue < audioclips.Length)
        {
            StartCoroutine(PlayHints(clue));
        }
    }

    public void StopHints()
    {
        StopAllCoroutines();
        if (currentAudio)
        {
            currentAudio.Stop();
            currentAudio = default;
        }

        if (imgHolder.gameObject.activeSelf)
        {
            imgHolder.FadeImage(fadeTime,false, aspectRatio);
            imgHolder.DelayedSetActive(false, fadeTime);
            //imgHolder.gameObject.SetActive(false);
        }
    }
}
