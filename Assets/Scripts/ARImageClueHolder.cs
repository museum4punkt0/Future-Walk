using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARImageClueHolder : MonoBehaviour
{
    private Image image;
    float ar;

    public void FadeImage(float fadetime, bool fadeIn, float aspectRatio)
    {
        // Rect rect = gameObject.GetComponentInParent<Rect>();
        // rect.width = 0.66f*(float)Screen.width;
        // rect.height = rect.width;
        
        image = gameObject.GetComponent<Image>();
        StartCoroutine(Fade(fadetime, fadeIn, aspectRatio));
    }

    private void Start() 
    {
        image = gameObject.GetComponent<Image>();

        Debug.Log(image);
        Debug.Log("RENDERER COLOR: " + image.color);
        //renderer.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    }

    private void OnEnable()
    {
        image = gameObject.GetComponent<Image>();
    }



    IEnumerator Fade(float fadetime, bool fadeIn, float aspectRatio)
    {
        float t = 0.0f;

        Color a0,a1;

        if(fadeIn)
        {
            a0 = new Color(1.0f,1.0f,1.0f,0.0f);
            a1 = new Color(1.0f,1.0f,1.0f,0.5f);
        }
        else
        {
            a0 = new Color(1.0f,1.0f,1.0f,0.5f);
            a1 = new Color(1.0f,1.0f,1.0f,0.0f);
        }

        image.color = a0;

        ar = aspectRatio;

        gameObject.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;


        while(t < fadetime)
        {
            t += Time.deltaTime;


            image.color = Color.Lerp(a0, a1, t/fadetime);
            yield return null;
        } 
    }

    public void DelayedSetActive(bool active, float fadeTime)
    {
        StartCoroutine(DelayedSetActiveCoroutine(active, fadeTime));
        Fade(fadeTime,false,ar);
    }

    IEnumerator DelayedSetActiveCoroutine(bool active, float fadeTime)
    {
        yield return new WaitForSeconds(fadeTime);
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}
// @ gg_1 passage: room9 change <img src="file"/> for: <img src="GG_03_SCRIPT_AR_Vermeer_Neutron_CLUE"/>
// @ gg_2 passage: room18 change <img src="file"/> for: <img src="GG_02_SCRIPT_Brueghel_CLUE"/>
