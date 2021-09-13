using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAccessibilitySettings : MonoBehaviour
{
    [SerializeField] private int regularFontSize = 17; // make sure this is the same as in ChatCell.prefab
    [SerializeField] private int accessibleFontSize = 24;

    private Text[] textComponents;

    void Start()
    {
        textComponents = gameObject.GetComponentsInChildren<Text>(true);  // TODO component[] GetComponentInChildren(Type type, bool includeInactive = true);
        if (textComponents.Length == 0)
        {
            Debug.Log("This Game Object doesn't have a Text Component");
        }

        // initially set it
        if (GlobalSettings.instance)
        {
            SwitchAccessibility(GlobalSettings.instance.AccessibilityEnabled);
        }

        GlobalSettings.OnAccessibilityToggle += SwitchAccessibility;
    }

    private void OnDestroy()
    {
        GlobalSettings.OnAccessibilityToggle -= SwitchAccessibility;
    }

    void ActivateAccessibility () 
    {
        foreach(var t in textComponents)
        {
            t.fontSize = accessibleFontSize; //TODO DETERMINE BEST ACCESSIBLE SETTINGS
            t.resizeTextMaxSize = accessibleFontSize;
            t.resizeTextMinSize = accessibleFontSize;
        }
        // textComponent.fontSize = accessibleFontSize;
        // Debug.Log("doing compatibility thingy");
    }

    void DeactivateAccessibility()
    {
        foreach(var t in textComponents)
        {
            t.fontSize = regularFontSize; //TODO DETERMINE BEST ACCESSIBLE SETTINGS
            t.resizeTextMaxSize = regularFontSize;
            t.resizeTextMinSize = regularFontSize;
        }
        // textComponent.fontSize = regularFontSize;
        // Debug.Log("UNDOING compatibility thingy");
    }

    void SwitchAccessibility(bool on)
    {
        if (on)
        {
            ActivateAccessibility();
            //Debug.Log("activitating accessibility, so result should be + but is " + accessibilityEnabled);
        }
        else
        {
            DeactivateAccessibility();
            //Debug.Log("DISABLING accessibility, so result should be - but is " + accessibilityEnabled);
        }
        Canvas.ForceUpdateCanvases();
    }
}
