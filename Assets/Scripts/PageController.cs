using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] Button weiterButton;
    [SerializeField] Button backButton;

    void OnEnable()
    {
        if (nextButton)
        {
            nextButton.onClick.AddListener(() => 
            {
                GlobalSettings.instance.SetEnglish();
                MainPageController.Next();
            });            
        }

        if (weiterButton)
        {
            weiterButton.onClick.AddListener(() => 
            {
                GlobalSettings.instance.SetGerman();
                MainPageController.Next();
            });            
        }

        if (backButton)
        {
            backButton.onClick.AddListener(() =>
            {
                MainPageController.instance.Prev();
            });
        }
    }

    void OnDisable()
    {
        if (nextButton) nextButton.onClick.RemoveAllListeners();
        if (weiterButton) weiterButton.onClick.RemoveAllListeners();
        if (backButton) backButton.onClick.RemoveAllListeners();
    }

}
