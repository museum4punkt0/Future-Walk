using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARTextClue : MonoBehaviour
{
    // Start is called before the first frame update
    string de0 = "Richten Sie Ihre Kamera auf das Kunstwerk";
    string de1 = "Scan erfolgreich!";
    string en0 = "Point your camera towards the artwork";
    string en1 = "Scan successful!";

    string failEn = "It seems there are some issue with the device";
    string failDe = "Anscheinend gibt es Probleme mit dem Gerät";

    Color cText = new Color(1.0f,1.0f,1.0f,1.0f);
    Color transp = new Color(1.0f,1.0f,1.0f,0.0f);

    float fadetime = 0.25f;
    float fadeRate = 0.1f;
    
    Text text;

    void Awake()
    {
        text = gameObject.GetComponent<Text>();
        ResetText();
    }

    private void OnEnable()
    {
        text.color = cText;
        ResetText();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }


    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
        ResetText();
    }

    private void ResetText()
    {
        if(GlobalSettings.instance.IsEnglish)
        {
            text.text = en0;
        }
        else
        {
            text.text = de0;
        }
    }

    private void SetSucessText()
    {
        if(GlobalSettings.instance.IsEnglish)
        {
            text.text = en1;
        }
        else
        {
            text.text = de1;
        }
    }

    private void SetFailText()
    {
        if(GlobalSettings.instance.IsEnglish)
        {
            text.text = failEn;
        }
        else
        {
            text.text = failDe;
        }
    }

    public void FailAnimation()
    {
        StartCoroutine(fade(cText,transp));
        SetFailText();
        StartCoroutine(fade(transp,cText));
    }

    public void SuccessAnimation()
    {
        //StartCoroutine(fade(false));
        StartCoroutine(fade(cText,transp));
        SetSucessText();
        //StartCoroutine(fade(true));
        StartCoroutine(fade(transp,cText));
    }

    public void FadeAway()
    {
        Log.d("Fading away...");
        StartCoroutine(fade(cText, transp));
        //StartCoroutine(fade(false));
    }



    // IEnumerator fade(bool appear)
    // {
    //     Color c0, c1;
    //     if(appear)
    //     {
    //         c0 = transp;
    //         c1 = cText;
    //     }
    //     else
    //     {
    //         c0 = cText;
    //         c1 = transp;
    //     }

    //     text.color = c0;
    //     Log.d(text.color);

    //     float t = 0.0f;

    //     while(t < fadetime)
    //     {
    //         t += Time.deltaTime;
    //         text.color = Color.Lerp(c0, c1, t*fadeRate);
    //         yield return null;
    //     } 
    // }

    IEnumerator fade(Color c0, Color c1)
    {
        text.color = c0;
        Log.d(text.color.ToString());

        float t = 0.0f;

        while(t < fadetime)
        {
            t += Time.deltaTime;
            text.color = Color.Lerp(c0, c1, t/fadetime);
            yield return null;
        } 
    }
}

