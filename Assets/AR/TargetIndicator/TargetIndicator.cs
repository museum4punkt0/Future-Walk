using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using DigitalRuby.Tween;

public class TargetIndicator : MonoBehaviour
{    
    // [SerializeField]
    // float transitionDuration = 1.0f;
    
    // // [SerializeField]
    // // public GameObject indicatorOrigin;




    // private Vector3 sourceScale = Vector3.one;
    // private Vector3 targetScale = Vector3.one*1.2f;

    // private GameObject targetGameObject;
    // private GameObject sourceGameObject;

    // private ITween<float> targetTween;


    // Start is called before the first frame update
    // void Start()
    // {
    //     if (indicatorOrigin)
    //     {
    //         targetGameObject = indicatorOrigin;
    //         SetTarget(indicatorOrigin);
    //     }
    // }

    // public void IndicateTarget(GameObject newTarget, Action finishCb = null)
    // {


    //     // tracked image transform
    //     // camera transform
    //     // go.transform = interpol

    //     Log.d("------------- Indicate Target: " + newTarget.transform.position);

    //     sourceGameObject = targetGameObject;        
    //     targetGameObject = newTarget;

    //     if (targetTween != null)
    //     {
    //         targetTween.Stop(TweenStopBehavior.DoNotModify);
    //         targetTween = null;
    //     }

    //     //transform.SetParent(targetGameObject.transform, true);
    //     sourceScale = transform.localScale;

    //     System.Action<ITween<float>> moveTween = (t) =>
    //     {
    //         transform.position = Vector3.Lerp(sourceGameObject.transform.position, targetGameObject.transform.position, t.CurrentValue);
    //         transform.rotation = Quaternion.Lerp(sourceGameObject.transform.rotation, targetGameObject.transform.rotation, t.CurrentValue);
    //         transform.localScale = Vector3.Lerp(sourceScale, targetScale, t.CurrentValue);
    //     };

    //     System.Action<ITween<float>> moveTweenCompleted = (t) =>
    //     {
    //         targetTween = null;

    //         if (finishCb != null)
    //         {
    //             finishCb();
    //         }
    //     };

    //     targetTween = gameObject.Tween("ARTargetMove",
    //                                     0f,
    //                                     1f,
    //                                     transitionDuration,
    //                                     TweenScaleFunctions.CubicEaseInOut,
    //                                     moveTween,
    //                                     moveTweenCompleted);
    // }

    // public void SetTarget(GameObject newTarget)
    // {
    //     targetGameObject = newTarget;

    //     //transform.SetParent(targetGameObject.transform, true);
    //     sourceScale = transform.localScale;

    //     transform.position = targetGameObject.transform.position;
    //     transform.rotation = targetGameObject.transform.rotation;
    //     transform.localScale = targetScale;
    // }

    // public void ResetOrigin()
    // {
    //     SetTarget(indicatorOrigin);
    // }

    ///////////////////////////////////////////////////////////////////////////////

    //Coroutine coroutineStorer = default;

    [Header("Animation Timers")]
    [SerializeField]float fadeOutTimer = 1.0f;
    [SerializeField] float movInterpolTime = 3.0f;
    [SerializeField] float rotInterpolTime = 3.0f;

    private Vector3 p0 = new Vector3(0.0f, 0.0f, 2.0f);
    private Quaternion r0 = Quaternion.identity;
    private Vector3 s0 = new Vector3(1.0f, 1.2f, 1.2f);

    private ARTrackedImageManager _imageManager;

    float movSpeed = 0.1f;
    float rotSpeed = 0.1f;

    bool fadein = false;
    public bool targetFixed = false;
    public bool animationStarted = false;

    Material mat;

    private void Awake()
    {
        _imageManager = GameObject.Find("AR Session Origin").GetComponent(typeof(ARTrackedImageManager)) as ARTrackedImageManager;
    }

    private void OnEnable() 
    {
        if(_imageManager)
        {
            _imageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnDisable() 
    {
        if(_imageManager)
        {
            _imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
        StopAllCoroutines();
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.updated)
        {
            if(trackedImage.trackingState == TrackingState.Tracking && targetFixed)
            {
                UpdateTargetIndicator(trackedImage);
            }
        }
    }

    void UpdateTargetIndicator(ARTrackedImage trackedImage)
    {
        transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
    }

    private void Start()
    {
        mat = gameObject.GetComponentInChildren<Renderer>().material;
    }

    public void InterruptAndRestart()
    {
        StartCoroutine(FadeOutIndicator());
        StopAllCoroutines();
        targetFixed = false;
        animationStarted = false;
        fadein = true;
        Restart();
    }


    public void StartInterpolation(Transform origin, Transform end) 
    {
        animationStarted = true;
        StartCoroutine(InterpolPosition(origin, end));
    }

    //IEnumerator InterpolPosition(Transform origin, Transform end) 
    //{
    //    float t = 0.0f;

    //    while(t < movInterpolTime)
    //    {
    //        t += Time.deltaTime;
    //        transform.position = Vector3.Lerp(origin.position,end.position,t * movSpeed);
    //        //
    //        transform.rotation = Quaternion.Lerp(origin.rotation,end.rotation,t * rotSpeed);
    //        transform.localScale = Vector3.Lerp(origin.localScale, end.localScale, t * rotSpeed);
    //        //
    //        yield return null;
    //    }
    //    transform.SetParent(end);
    //    //
    //    targetFixed = true;
    //    //

    //    //StartCoroutine(InterpolRotation(origin,end));
    //}

    IEnumerator InterpolPosition(Transform origin, Transform end)
    {
        float t = 0.0f;

        // TODO CHECK end.extents
        ARTrackedImage ai = end.gameObject.GetComponent<ARTrackedImage>();
        Vector3 endScale = new Vector3(2f*ai.extents.x,1f, 2f * ai.extents.y);

        while (t < movInterpolTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(origin.position, end.position, t * movSpeed);
            //
            transform.rotation = Quaternion.Lerp(origin.rotation, end.rotation, t * rotSpeed);
            //
            transform.localScale = Vector3.Lerp(origin.localScale, endScale, t * rotSpeed);
            //transform.localScale = Vector3.Lerp(origin.localScale, end.localScale, t * rotSpeed);
            //
            yield return null;
        }
        transform.SetParent(end);
        //
        targetFixed = true;
        //

        //StartCoroutine(InterpolRotation(origin,end));
    }

    IEnumerator InterpolRotation(Transform origin, Transform end)
    {
        float t = 0.0f;
        Debug.Log("target fixed start:"+targetFixed+" at "+ Time.time);

        while(t < rotInterpolTime)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(origin.rotation,end.rotation,t * rotSpeed);
            transform.localScale = Vector3.Lerp(origin.localScale, end.localScale, t * rotSpeed);
            yield return null;
        }
        targetFixed = true;

        Debug.Log("target fixed end:"+targetFixed+" at "+ Time.time);
    }

    public void Restart()
    {
        if(!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        targetFixed = false;
        animationStarted = false;

        r0.eulerAngles = new Vector3(-90.0f,0.0f, 0.0f);
            
        transform.SetPositionAndRotation(p0,r0);
        transform.localScale = s0;

        if(fadein)
        {
            StartCoroutine(FadeIn());
            fadein = false;
        }
        else
        {
            Material mat = gameObject.GetComponentInChildren<Renderer>().material;
            mat.color = new Color(255,255,255,255);
        }
    }

    IEnumerator FadeIn()
    {
        Material mat = gameObject.GetComponentInChildren<Renderer>().material;
        Color col1 = mat.color;
        Color col0 = new Color(255,255,255,0);
        float t =0f;

        mat.color = col0;

        while(t < fadeOutTimer)
        {
            t += Time.deltaTime;
            mat.color = Color.Lerp(mat.color,col1,t/fadeOutTimer);
            yield return null;
        }
    }

    public void TargetOff()
    {
        StartCoroutine(FadeOutIndicator());
    }

    IEnumerator FadeOutIndicator()
    {
        Material mat = gameObject.GetComponentInChildren<Renderer>().material;
        Color col0 = mat.color;
        Color col1 = new Color(255,255,255,0);
        float t =0f;

        while(t < fadeOutTimer)
        {
            t += Time.deltaTime;
            mat.color = Color.Lerp(mat.color,col1,t/fadeOutTimer);
            yield return null;
        }

        //gameObject.SetActive(false);
        //mat.color = col0;
    }

    // private void OnDisable()
    // {
    //     StopAllCoroutines();
    // }

    public void DestroyIndicatorDelayed()
    {
        Destroy(gameObject,fadeOutTimer);
    }
}
