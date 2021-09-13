using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRuby.Tween;
using UnityEngine;

[RequireComponent(typeof(BottomScrollerView))]
public class MoveIn : MonoBehaviour
{
    BottomScrollerView scroller;

    private void Awake()
    {
        scroller = GetComponent<BottomScrollerView>();
    }

    void Start()
    {
        if (scroller)
        {
            scroller.SetEnableScrolling = false;
        }

        StartCoroutine(Delayed(0.2f));
    }

    IEnumerator Delayed(float time)
    {
        yield return new WaitForSeconds(time);


        if (MainPageController.instance &&
            GlobalSettings.instance &&
            GlobalSettings.instance.Onboarded)
        {
            MainPageController.instance.FadeoutImages();
        }


        float y = GetComponent<RectTransform>().anchoredPosition.y;

        Action<ITween<float>> setPos = (t) =>
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(t.CurrentValue, y);
        };

        Action<ITween<float>> setPosDone = (t) =>
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(t.CurrentValue, y);

            if (MainPageController.instance)
            {
                MainPageController.instance.DoStartStory();
            }

            if (scroller)
            {
                scroller.SetEnableScrolling = true;
            }
        };

        gameObject.Tween("moveIn",
            GetComponent<RectTransform>().anchoredPosition.x,
            0,
            1.3f,
            TweenScaleFunctions.CubicEaseInOut,
            setPos,
            setPosDone);
    }

}
