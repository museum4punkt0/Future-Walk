using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class BurgerMenuController : MonoBehaviour
{
    public static BurgerMenuController instance;


    [SerializeField] Canvas burgerMenu;
    [SerializeField] Toggle fontSize;
    [SerializeField] Toggle audioTranscript;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (burgerMenu)
        {
            var rectTransform = burgerMenu.GetComponent<RectTransform>();

            if (rectTransform)
            {
                rectTransform.anchoredPosition = new Vector2(-Screen.width, rectTransform.anchoredPosition.y);
            }
        }

        Reset(false);
    }

    public void Reset(bool notify = true)
    {
        BurgerOut();

        if (fontSize)
        {
            fontSize.isOn = GlobalSettings.instance.AccessibilityEnabled;
        }
        else
        {
            Log.e("no accessibility toggle set!");
        }

        if (audioTranscript)
        {
            if (notify)
            {
                audioTranscript.isOn = GlobalSettings.instance.showAudioTranscript;
            }
            else
            {
                audioTranscript.SetIsOnWithoutNotify(GlobalSettings.instance.showAudioTranscript);
            }
        }
        else
        {
            Log.e("no audio transcript toggle set!");
        }
    }

    public void BurgerOut()
    {
        if (burgerMenu)
        {
            var rectTransform = burgerMenu.GetComponent<RectTransform>();

            if (rectTransform)
            {
                System.Action<ITween<Vector2>> pageTween = (t) =>
                {
                    rectTransform.anchoredPosition = t.CurrentValue;
                };

                System.Action<ITween<Vector2>> pageTweenCompleted = (t) =>
                { 
                    burgerMenu.gameObject.SetActive(false);
                };

                burgerMenu.gameObject.Tween("BurgerPageMove",
                                            rectTransform.anchoredPosition,
                                            new Vector2(-Screen.width, rectTransform.anchoredPosition.y),
                                            0.8f,
                                            TweenScaleFunctions.CubicEaseInOut,
                                            pageTween,
                                            pageTweenCompleted);
            }
        }
    }

    public void BurgerIn()
    {
        if (burgerMenu)
        {
            var rectTransform = burgerMenu.GetComponent<RectTransform>();

            if (rectTransform)
            {
                System.Action<ITween<Vector2>> pageTween = (t) =>
                {
                    rectTransform.anchoredPosition = t.CurrentValue;
                };

                System.Action<ITween<Vector2>> pageTweenCompleted = (t) =>
                { 

                };

                burgerMenu.gameObject.Tween("MainPageMove",
                                            rectTransform.anchoredPosition,
                                            new Vector2(0, rectTransform.anchoredPosition.y),
                                            0.8f,
                                            TweenScaleFunctions.CubicEaseInOut,
                                            pageTween,
                                            pageTweenCompleted);
            }
        }
    }
}
