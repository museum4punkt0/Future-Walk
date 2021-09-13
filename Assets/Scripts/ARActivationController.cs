using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;


// TODO:
//
// Scale not working well

public class ARActivationController : MonoBehaviour, IARSuccessProvider
{
    [Header("UI Settings")]
    [SerializeField] TargetIndicator targetIndicator;
    [SerializeField] ARTextClue textClue;
    [SerializeField] ARProximityWarning proximityWarning;
    //[SerializeField] Canvas ARUI;

    [Header("Game Elements")]
    [SerializeField] GameObject target;
    [SerializeField] Renderer xrayRenderer;
    [SerializeField] bool imageMarkerScene = false;
    [SerializeField] string ImageName;

    ARAudioHints arHints;
    GameObject cam;
    private TargetIndicator _targetIndicator;
    private ARTrackedImageManager _imageManager;
    private ARProximityWarning _proximityWarning;
    private bool activated = false;
    private bool _success = false;

    private AudioSourceLanguage AudioRightPainting;

    // IARSuccessProvider
    public bool Success => _success;    


    private void Awake()
    {
        if (imageMarkerScene)
        {
            var managers = transform.parent.GetComponentsInChildren<ARTrackedImageManager>(true);
            if (managers.Length > 0)
            {
                _imageManager = managers[0];
            }
            else
            {
                Log.e("could not find ARTrackedImageManager");
            }
        }

        arHints = gameObject.GetComponent<ARAudioHints>();

        if (imageMarkerScene)
        {
            var aSources = GetComponents<AudioSourceLanguage>();
            AudioRightPainting = aSources[4];
        }
    }

    public void Init(GameObject camera)
    {
        cam = camera;

        if (imageMarkerScene)
        {
            _imageManager.trackedImagesChanged += OnTrackedImagesChanged;
            _targetIndicator = Instantiate(targetIndicator, cam.transform, false);

            GameObject ARUI = GameObject.Find("UICanvas");
            if(ARUI) {
                _proximityWarning = Instantiate(proximityWarning, ARUI.gameObject.transform, false);
                _proximityWarning.transform.SetAsFirstSibling();
            }

            if (textClue) textClue.SetActive(true);
        }
        else
        {
            if (textClue) textClue.SetActive(false);
        }

        _success = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();

        if (imageMarkerScene)
        {
            _imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            if (textClue) textClue.SetActive(false);
            target.SetActive(false);
        }

        if (_targetIndicator) _targetIndicator.DestroyIndicatorDelayed();
        if (_proximityWarning) _proximityWarning.DestroyDelayed();
       
        activated = false;
        _success = false;

        if (arHints != null)
        {
            arHints.StopHints();
        }
    }

    


    //--------------------------------------------------------------------
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {

        if (_targetIndicator==null && !activated)
        {
             _targetIndicator = Instantiate(targetIndicator, cam.transform, false);
        }

        foreach (var trackedImage in eventArgs.added)
        {
            //trackingStateText.text = trackedImage.trackingState.ToString();
            //Log.d(trackedImage.trackingState.ToString());
            if (trackedImage.trackingState != TrackingState.Tracking)
            {
                return;
            }
            else
            {
                UpdateAsset(trackedImage);
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            //trackingStateText.text = trackedImage.trackingState.ToString();
            //Log.d(trackedImage.trackingState.ToString());
            if (trackedImage.trackingState != TrackingState.Tracking)
            {
                if (_targetIndicator && !_targetIndicator.targetFixed)
                {
                    //_targetIndicator.InterruptAndRestart();
                    _targetIndicator.DestroyIndicatorDelayed();
                    activated = false;
                }

                // Log.d("not tracking");
                // //viewManager = StartCoroutine(FadeOutContent());
                // StartCoroutine(FadeOutContent());
                return;
            }
            else
            {
                if (arHints != null)
                {
                    arHints.StopHints();
                }
                
                UpdateAsset(trackedImage);
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.None)
                StartCoroutine(FadeOutContent());
        }

        // foreach (var trackedImage in eventArgs.added)
        // {
        //     Log.d(trackedImage.trackingState.ToString());
        //     UpdateAsset(trackedImage);
        // }

        // foreach (var trackedImage in eventArgs.updated)
        // {
        //     Log.d(trackedImage.trackingState.ToString());
        //     UpdateAsset(trackedImage);
        // }
    }

    IEnumerator FadeOutContent()
    {
        // TODO: MAKE THIS THENG WORK

        // Log.d("fading out content for good babys");
        
        // Component[] renderers;
        // renderers = GetComponentsInChildren(typeof(Renderer),true);
        

        // foreach(Renderer r in renderers)
        // {

        //     float t = 0.0f;
        //     float tfade =0.5f;
        //     float alpha0 = r.material.color.a;
        //     float alpha1 = 0.0f;

        //     Color c = r.material.color;

        //     while(t < tfade) 
        //     {
        //         t += Time.deltaTime;
        //         c.a = Mathf.Lerp(alpha0, alpha1, t);
        //         yield return null;
        //     }
        // }

        Color c = xrayRenderer.material.color;
        Color cf = new Color(255,255,255,0);
        float t = 0.0f;
        float tf = 0.5f;

        while(t < tf)
        {
            t += Time.deltaTime;
            c = Color.Lerp(c,cf,t);
            yield return null;
        }
    }

    void UpdateAsset(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        if (!imageMarkerScene) return;
        if (imageName != ImageName) return;

        //HACK:also scaling in Y to keep transformations in Z (rotating picture frame) in proportional scale
        target.transform.localScale = new Vector3(trackedImage.size.x, trackedImage.size.x, trackedImage.size.y);

        target.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);


        if (!target.activeSelf && !activated)
        {
            // if (_targetIndicator == null)
            // {
            //     _targetIndicator = Instantiate(targetIndicator, cam.transform, false);
            // }

            if (!_targetIndicator.animationStarted)
            {
                _targetIndicator.StartInterpolation(_targetIndicator.transform, trackedImage.transform);
            }

            if (_targetIndicator.targetFixed)
            {
                

                activated = true;
                if (textClue) textClue.SuccessAnimation();
                //_targetIndicator.TargetOff();

                _success = true;

                StartCoroutine(DelayedActivation());
            }
            
        }
    }

    private void PostActivationAction()
    {
        Log.d("Post Activation Action");
    }

    IEnumerator DelayedActivation()
    {
        yield return new WaitForSeconds(2.0f);
        PostActivationAction();

        if (textClue) textClue.FadeAway();

        if (arHints != null)
        {
            arHints.StopHints();
        }

        //Add audio x.2
        AudioRightPainting.Play();
        yield return new WaitForSeconds(6.5f);

        _targetIndicator.TargetOff();
        target.SetActive(true);
    }
}
