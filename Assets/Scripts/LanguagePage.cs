using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguagePage : MonoBehaviour
{
    [SerializeField] Toggle germanChoice;
    [SerializeField] Toggle englishChoice;
    [SerializeField] GameObject germanButton;
    [SerializeField] GameObject englishButton;

    private void Awake()
    {
        if (!germanChoice) Log.e("no germanChoice set!");
        if (!englishChoice) Log.e("no englishChoice set!");
        if (!germanButton) Log.e("no germanButton set!");
        if (!englishButton) Log.e("no englishButton set!");
    }

    private void OnEnable()
    {
        if (GlobalSettings.instance.IsEnglish)
        {
            germanChoice.SetIsOnWithoutNotify(false);
            germanButton.SetActive(false);
            englishChoice.SetIsOnWithoutNotify(true);
            englishButton.SetActive(true);
        }
        else
        {
            germanChoice.SetIsOnWithoutNotify(true);
            germanButton.SetActive(true);
            englishChoice.SetIsOnWithoutNotify(false);
            englishButton.SetActive(false);
        }
    }
}
