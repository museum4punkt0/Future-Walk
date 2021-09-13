using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARProximityWarning : MonoBehaviour
{
    [SerializeField] Image panel;
    [SerializeField] Image warning;

    [SerializeField] float _distance;

    [SerializeField] float threshold;
    Camera _cam;

    Color white100 = Color.white;
    Color white0 = new Color(1.0f,1.0f,1.0f,0.0f);
    Color white50 = new Color(1.0f,1.0f,1.0f,0.5f);
    
    private ARTrackedImageManager _imageManager;
    bool visible = false;
    float timer = 0.1f;

    private void Awake() 
    {
        visible = false;
        _imageManager = GameObject.Find("AR Session Origin").GetComponent(typeof(ARTrackedImageManager)) as ARTrackedImageManager;
        _cam = GameObject.Find("AR Camera").GetComponent(typeof(Camera)) as Camera;

        if(panel != null && warning != null)
        {
            panel.color = white0;
            warning.color = white0;
        }


    }

    private void OnEnable()
    {
        visible = false;
        if(panel != null && warning != null)
        {
            panel.color = white0;
            warning.color = white0;
        }

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

    private void OnDestroy()
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
            UpdateWarning(trackedImage); 
        }
    }

    void UpdateWarning(ARTrackedImage trackedImage)
    {
        float distance = Vector3.Distance(_cam.transform.position,trackedImage.transform.position);

        _distance = distance;

        if(!visible && distance <=threshold)
        {
            ShowWarning();
        }

        if(visible && distance > threshold)
        {
            HideWarning();
        }
    }

    private void ShowWarning()
    {
        visible = true;
        StartCoroutine(FadeInWarning());
    }

    private void HideWarning()
    {
        visible = false;
        StartCoroutine(FadeOutWarning());
    }

    IEnumerator FadeInWarning()
    {
        float t =0f;

        while(t < timer)
        {
            t += Time.deltaTime;
            panel.color = Color.Lerp(white0,white50,t/timer);
            warning.color = Color.Lerp(white0,white100,t/timer);
            yield return null;
        }
    }

    IEnumerator FadeOutWarning()
    {
        float t =0f;

        while(t < timer)
        {
            t += Time.deltaTime;
            panel.color = Color.Lerp(white50,white0,t/timer);
            warning.color = Color.Lerp(white100,white0,t/timer);
            yield return null;
        }
    }

    public void DestroyDelayed()
    {
        Destroy(gameObject,timer);
    }
}
