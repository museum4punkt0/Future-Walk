using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using DigitalRuby.Tween;

public class ARController : MonoBehaviour
{
    [SerializeField] Image shield;
    [SerializeField] ARCameraManager cameraManager;

    protected void OnEnable()
    {        
        if (cameraManager)
        {
            cameraManager.frameReceived += frameReceived;
        }

        if (shield)
        {
            shield.color = Tools.RGBAColor(0, 0, 0, 255);
        }
        Log.d("ARController: Enable ()");
    }

    protected void OnDisable()
    {
        if (cameraManager)
        {
            cameraManager.frameReceived -= frameReceived;
        }
        if (shield)
        {
            shield.color = Tools.RGBAColor(0, 0, 0, 255);
        }
    }


    //--------------------------------------------------------
    //--------------------------------------------------------

    private void frameReceived(ARCameraFrameEventArgs args)
    {
        cameraManager.frameReceived -= frameReceived;

        StartCoroutine(DelayFade());
    }

    IEnumerator DelayFade()
    {
        yield return null;
        yield return null;
        FadeOut();
    }


    private void FadeOut()
    {
        Log.d("ARController: FadeOut()");
        System.Action<ITween<Color>> tweenUpdate = (t) =>
        {
            shield.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> tweenCompleted = (t) =>
        {
        };

        gameObject.Tween("ARShieldFadeOut",
                        shield.color,
                        Tools.RGBAColor(0, 0, 0, 0),
                        1f,
                        TweenScaleFunctions.CubicEaseInOut,
                        tweenUpdate,
                        tweenCompleted);
    }

}
