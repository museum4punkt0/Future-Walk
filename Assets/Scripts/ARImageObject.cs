using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class ARImageObject : MonoBehaviour
{
    [SerializeField]
    string imageName;

    [SerializeField]
    ARTrackedImageInfoManager manager;

    [SerializeField]
    Texture overlayImage;

    void OnEnable()
    {
        if (manager)
        {
            manager.AddTriggerListener((ARTrackedImage trackedImage) =>
            {
                Log.d("trigger: imageName: " + imageName);

                if (trackedImage.referenceImage.name == imageName)
                {
                    // show the plane inside tracked-image                    
                    var planeGo = trackedImage.GetComponentInChildren<MeshRenderer>(true).gameObject;
                    planeGo.SetActive(true);

                    // The image extents is only valid when the image is being tracked
                    // trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

                    // Set the texture
                    var material = planeGo.GetComponentInChildren<MeshRenderer>().material;

                    if (overlayImage)
                    {
                        material.mainTexture = overlayImage;
                    }
                    else if (trackedImage.referenceImage.texture)
                    {
                        material.mainTexture = trackedImage.referenceImage.texture;
                    }
                    else
                    {
                        planeGo.SetActive(false);
                    }
                }
            });
        }

    }

    // void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    // {
    //     // Log.d("ARImageObject: " + eventArgs.added.Count + " - updated: " + eventArgs.updated.Count);

    //     foreach (var trackedImage in eventArgs.added)
    //     {
    //         if (trackedImage.referenceImage.texture == markerImage)
    //         {
    //             // Give the initial image a reasonable default scale
    //             trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

    //             UpdateInfo(trackedImage);
    //         }
    //     }

    //     foreach (var trackedImage in eventArgs.updated)
    //     {
    //         if (trackedImage.referenceImage.texture == markerImage)
    //         {
    //             UpdateInfo(trackedImage);
    //         }
    //     }
    // }

    // void UpdateInfo(ARTrackedImage trackedImage)
    // {
    //     // Set canvas camera
    //     var planeGo = trackedImage.GetComponentInChildren<MeshRenderer>(true).gameObject;

    //     // Disable the visual plane if it is not being tracked
    //     if (trackedImage.trackingState != TrackingState.None)
    //     {
            // planeGo.SetActive(true);

            // // The image extents is only valid when the image is being tracked
            // trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

            // // Set the texture
            // var material = planeGo.GetComponentInChildren<MeshRenderer>().material;

            // if (overlayImage)
            // {
            //     material.mainTexture = overlayImage;
            // }
            // else if (trackedImage.referenceImage.texture)
            // {
            //     material.mainTexture = trackedImage.referenceImage.texture;
            // }
            // else
            // {
            //     planeGo.SetActive(false);
            // }
    //     }
    //     else
    //     {
    //         planeGo.SetActive(false);
    //     }
    // }

}
