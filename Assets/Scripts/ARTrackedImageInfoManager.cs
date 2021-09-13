using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARTrackedImageInfoManager : MonoBehaviour
{
    [SerializeField]
    TargetIndicator targetIndicator;

    [SerializeField]    
    GameObject indicatorOrigin;

    ARTrackedImageManager m_TrackedImageManager;
    private bool triggered = false;

    Coroutine waitingForTrigger = default;

    private List<Action<ARTrackedImage>> triggers = new List<Action<ARTrackedImage>>();

    //-------------------------------
    // audio-hint
    private int timeoutCount = 0;
    private Coroutine timeoutCR = default;
    
    [SerializeField]
    private int hintTimeout = 60;

    [SerializeField]
    private AudioClip[] hints = new AudioClip[3];

    // image selection
    public string imageName;

    // 
    private bool arPersistent = false;

    public void AddTriggerListener(Action<ARTrackedImage> a)
    {
        if (!triggers.Contains(a))
        {
            triggers.Add(a);
        }

        Debug.Log("TRIGGERs: " + triggers.Count);
    }

    public void RemoveTriggerListener(Action<ARTrackedImage> a)
    {
        if (triggers.Contains(a))
        {
            triggers.Remove(a);
        }
    }

    public void SetARPersistent()
    {
        Log.d("set AR persistnet");
        arPersistent = true;
    }

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;        

        startHints();        
    }

    void OnDisable()
    {
        stopHints();

        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

        triggers.Clear();
        arPersistent = false;
    }

    public void TargetOff()
    {
        if (targetIndicator)
        {
            targetIndicator.gameObject.SetActive(false);
        }
    }

    public void Reset()
    {
        if (targetIndicator)
        {
            //targetIndicator.gameObject.SetActive(true);
            //targetIndicator.ResetOrigin();
        }
        timeoutCount = 0;
        triggered = false;
    }

    IEnumerator startTriggerDelayed(ARTrackedImage trackedImage)
    {
        yield return new WaitForSeconds(1);

        triggered = true;
        //TargetOff();

        if (triggers.Count == 0)
        {
            Debug.Log("no triggers!");
        }
 
        foreach(var trigger in triggers)
        {
            trigger(trackedImage);
        }

        // after it triggered, mark it as persistent?
    }

    void UpdateInfo(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage.name == imageName)
        {
            // found correct image
            stopHints();

            var trackedGO = trackedImage.gameObject;            

            if (trackedImage.trackingState != TrackingState.None)
            {
                // The image extents is only valid when the image is being tracked
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

                if (!triggered && !trackedGO.activeSelf)
                {
                    trackedGO.SetActive(true);

                    // targetIndicator.GetComponent<TargetIndicator>().IndicateTarget(trackedGO, () =>
                    // {
                    //     waitingForTrigger = StartCoroutine(startTriggerDelayed(trackedImage));
                    // });
                }
            }
            else
            {
                // Disable the visual plane if it is not being tracked
                if (!arPersistent)
                {
                    Log.d("not persistent - turn it off");
                    trackedGO.SetActive(false);
                }
            }
        }        
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Debug.Log("added: " + eventArgs.added.Count + " - updated: " + eventArgs.updated.Count + " - removed: "+ eventArgs.removed.Count);
        // Debug.Log("tracked: " + m_TrackedImageManager.trackables.count);

        foreach (var trackedImage in eventArgs.added)
        {
            // initially false?
            trackedImage.gameObject.SetActive(false);
            // UpdateInfo(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            // Debug.Log("update:" + trackedImage.referenceImage.name + " - " + trackedImage.trackingState);            

            // check if it is visible
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdateInfo(trackedImage);
            }
            else
            {
                // Debug.Log("state != tracking!");
                if (trackedImage.gameObject.activeSelf)
                {
                    if (!arPersistent)
                    {
                        trackedImage.gameObject.SetActive(false);
                    }

                    if (!triggered)
                    {
                        if (waitingForTrigger != null)
                        {
                            StopCoroutine(waitingForTrigger);
                            waitingForTrigger = default;
                        }
                        //targetIndicator.GetComponent<TargetIndicator>().IndicateTarget(indicatorOrigin);
                    }
                }
            }
        }

        // if (m_TrackedImageManager.trackables.count == 0)
        // {
        //     targetIndicator.GetComponent<TargetIndicator>().IndicateTarget(indicatorOrigin);
        // }
    }

    private void stopHints()
    {
        if (timeoutCR != null)
        {
            StopCoroutine(timeoutCR);
            timeoutCR = null;
        }
    }

    private void startHints()
    {
        stopHints();
        timeoutCR = StartCoroutine(timeoutForHint(hintTimeout));
    }

    IEnumerator timeoutForHint(int seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (timeoutCount < hints.Length)
        {
            var audio_source = GetComponent<AudioSource>();
            if (audio_source)
		    {
                audio_source.clip = hints[timeoutCount];
                audio_source.Play();
            }

            timeoutCount++;

            if (timeoutCount < hints.Length)
            {
                timeoutCR = StartCoroutine(timeoutForHint(hintTimeout));
            }
        }
    }
}
