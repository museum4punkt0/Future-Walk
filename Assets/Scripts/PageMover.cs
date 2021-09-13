using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class PageMover : MonoBehaviour
{
    public GameObject mainCanvas;
    public GameObject page;
    RectTransform mainCanvasRectTransform;

    public float duration = 0.4F;
    public float width = 100f;

    void Start()
    {
        if (mainCanvas)
        {
            mainCanvasRectTransform = mainCanvas.GetComponent<RectTransform>();
        }

        if (page) page.SetActive(false);

        if (width < 0)
        {
            width = Screen.width;
        }
    }

    public void moveOut()
    {
        if (mainCanvasRectTransform)
        {
            System.Action<ITween<Vector2>> pageTween = (t) =>
            {
                mainCanvasRectTransform.anchoredPosition = t.CurrentValue;
            };

            System.Action<ITween<Vector2>> pageTweenCompleted = (t) =>
            { 

            };

            if (page) page.SetActive(true);

            mainCanvas.gameObject.Tween("MainPageMove",
                                        mainCanvasRectTransform.anchoredPosition,
                                        new Vector2(-width, mainCanvasRectTransform.anchoredPosition.y),
                                        duration,
                                        TweenScaleFunctions.CubicEaseInOut,
                                        pageTween,
                                        pageTweenCompleted);
        }

    }

    public void moveIn()
    {
        if (mainCanvasRectTransform)
        {
            System.Action<ITween<Vector2>> pageTween = (t) =>
            {
                mainCanvasRectTransform.anchoredPosition = t.CurrentValue;
            };

            System.Action<ITween<Vector2>> pageTweenCompleted = (t) =>
            {
                if (page) page.SetActive(false);
            };

            mainCanvas.gameObject.Tween("MainPageMove",
                                        mainCanvasRectTransform.anchoredPosition,
                                        new Vector2(0, mainCanvasRectTransform.anchoredPosition.y),
                                        duration,
                                        TweenScaleFunctions.CubicEaseInOut,
                                        pageTween,
                                        pageTweenCompleted);
        }

    }
}
