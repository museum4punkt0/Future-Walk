using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ContentSizeFitter))]
public class WidthConstraint : MonoBehaviour
{
    ContentSizeFitter fitter;

    // using canvas scaler reference width = 375
    float maxWidth = 375 - 114;

    private void Awake()
    {
        fitter = GetComponent<ContentSizeFitter>();
    }

    public bool Constrain()
    {
        if (fitter.enabled &&
            fitter.horizontalFit == ContentSizeFitter.FitMode.PreferredSize)
        {
            // check width of cell
            var trans = GetComponent<RectTransform>();

            if ((trans.rect.width + trans.anchoredPosition.x) > maxWidth)
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                trans.sizeDelta = new Vector2(maxWidth, trans.sizeDelta.y);

                // return true to force relayout and wait
                return true;
            }
        }

        return false;
    }
}
