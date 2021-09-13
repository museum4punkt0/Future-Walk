using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class ChoiceButtonMover : MonoBehaviour
{
    public const string AR_PREFIX = "_AR_";
    public const string ARG_PREFIX = "_ARG_"; // ar camera georg
    public const string ARV_PREFIX = "_ARV_"; // ar camera vermeer
    public const string FC_PREFIX = "_FC_"; // fake camera
    public const string SPECIAL_PREFIX = "__special"; // a pink button
    public static ChoiceButtonMover instance;

    public static bool IsAR(string text)
    {
        return text.StartsWith(AR_PREFIX) 
                || text.StartsWith(ARG_PREFIX)
                || text.StartsWith(ARV_PREFIX)
                || text.StartsWith(FC_PREFIX);
    }

    [SerializeField] GameObject choiceScroller;
    [SerializeField] GameObject choiceArea;

    [SerializeField] GameObject choiceButtonPrefab;
	[SerializeField] GameObject arChoiceButtonPrefab;
    [SerializeField] GameObject specialButtonPrefab;

    RectTransform scrollerRectTransform;

    float choiceOriginalY = 0;
    float choiceOffY = 0;
    public float animTime = 0.4F;

    ITween<Vector2> choiceTween;

    void Awake()
    {
        instance = this;
        ClearChoices();
    }

    void Start()
    {
        if (choiceScroller)
        {
            scrollerRectTransform = choiceScroller.GetComponent<RectTransform>();
            choiceOriginalY = scrollerRectTransform.anchoredPosition.y;
            choiceOffY = choiceOriginalY - 100;

            // set hidden
            scrollerRectTransform.anchoredPosition = new Vector2(scrollerRectTransform.anchoredPosition.x, choiceOffY);
        }
    }

    public void UpdateButtons(Action act = null)
    {
        bool hasVisibleChildren = false;
        int count = 0;
        foreach (Transform child in choiceArea.transform)
        {
            count++;
            if (child.gameObject.activeSelf)
            {
                hasVisibleChildren = true;
                break;
            }
        }

        if (!hasVisibleChildren)
        {            
            HideChoices(act);
        }
        else if (act != null)
        {
            act();
        }
    }


    public void ClearChoices()
    {
        //Log.d("clear choices");

        // remove all choice buttons
        var children = new List<GameObject>();
        foreach (Transform child in choiceArea.transform) 
        {
            if (child.gameObject.activeSelf) children.Add(child.gameObject);
        }
        
        children.ForEach(child => Destroy(child));
    }

    public void AddChoice(string content, Action<string> clickAction)
    {
        //Log.d("add choice: " + content + " - " + Thread.CurrentThread.ManagedThreadId);

        var prefab = choiceButtonPrefab;

        ShowChoices();

        UnityEngine.Events.UnityAction act = null;

        ARSceneController.ARType type = ARSceneController.ARType.None;

        if (content.StartsWith(ARG_PREFIX))
        {
            if (arChoiceButtonPrefab) prefab = arChoiceButtonPrefab;
            content = content.Substring(ARG_PREFIX.Length);
            
            act = () => HideChoices(() => ARSceneController.instance.ShowType(ARSceneController.ARType.ARImageGeorg));
        }
        if (content.StartsWith(ARV_PREFIX))
        {
            if (arChoiceButtonPrefab) prefab = arChoiceButtonPrefab;
            content = content.Substring(ARV_PREFIX.Length);
            
            act = () => HideChoices(() => ARSceneController.instance.ShowType(ARSceneController.ARType.ARImageVermeer));
        }        
        else if (content.StartsWith(FC_PREFIX))
        {
            if (arChoiceButtonPrefab) prefab = arChoiceButtonPrefab;
            content = content.Substring(FC_PREFIX.Length);
            
            act = () => HideChoices(() => ARSceneController.instance.ShowType(ARSceneController.ARType.FakeCamera));
        }
        else
        {
            if (content.StartsWith(SPECIAL_PREFIX))
            {
                if (specialButtonPrefab) prefab = specialButtonPrefab;
                content = content.Substring(SPECIAL_PREFIX.Length);
            }

            act = () =>
            {
                HideChoices(() =>
                {
                    
                });

                if (clickAction != null)
                {
                    clickAction(content);
                }
            };
        }
        
        var buttonGO = Instantiate(prefab, choiceArea.transform);
        buttonGO.GetComponentInChildren<Text>(true).text = content;
        buttonGO.GetComponent<Button>().onClick.AddListener(act);
    }

    private void StopChoiceTween()
    {
        if (choiceTween != null)
        {
            choiceTween.Stop(TweenStopBehavior.DoNotModify);
            choiceTween = null;
        }
    }

    public void ShowChoices()
    {
        StopChoiceTween();

        if (scrollerRectTransform)
        {
            System.Action<ITween<Vector2>> choiceTweenUpdate = (t) =>
            {
                scrollerRectTransform.anchoredPosition = t.CurrentValue;
            };

            System.Action<ITween<Vector2>> choiceTweenCompleted = (t) =>
            {
                choiceTween = null;
            };

            choiceTween = choiceScroller.gameObject.Tween("ChoiceMove",
                                        scrollerRectTransform.anchoredPosition,
                                        new Vector2(scrollerRectTransform.anchoredPosition.x, choiceOriginalY),
                                        animTime,
                                        TweenScaleFunctions.CubicEaseInOut,
                                        choiceTweenUpdate,
                                        choiceTweenCompleted);
        }
    }

    public void HideChoices(Action endAction = null)
    {
        if (scrollerRectTransform)
        {
            System.Action<ITween<Vector2>> choiceTweenUpdate = (t) =>
            {
                scrollerRectTransform.anchoredPosition = t.CurrentValue;
            };

            System.Action<ITween<Vector2>> choiceTweenCompleted = (t) =>
            {
                ClearChoices();
                choiceTween = null;

                if (endAction != null)
                {
                    endAction();
                }
            };

            choiceTween = choiceScroller.gameObject.Tween("ChoiceMove",
                                        scrollerRectTransform.anchoredPosition,
                                        new Vector2(scrollerRectTransform.anchoredPosition.x, choiceOffY),
                                        animTime,
                                        TweenScaleFunctions.CubicEaseInOut,
                                        choiceTweenUpdate,
                                        choiceTweenCompleted);
        }

    }

}
