using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceCamera : MonoBehaviour
{
    [SerializeField]    
    int imageCountThreshold = 5;

    [SerializeField]
    float timeout = 15;

    [SerializeField]
    float darkFlashTime = 0.15F;

    [SerializeField]
    GameObject storyController;
    StoryChat chat;

    WebCamTexture webCamTexture;

    int imageCount = 0;
    bool isCapturing = false;
    bool initial = true;
    Coroutine timeoutCR = default;

    void Awake()
    {
        if (storyController)
        {
            chat = storyController.GetComponent<StoryChat>();
        }
    }

    void createCamera()
    {
        if (!webCamTexture)
        {
            //WebCamDevice[] devices = WebCamTexture.devices;
            webCamTexture = new WebCamTexture();
            GetComponent<RawImage>().texture = webCamTexture;
        }
    }

    void OnEnable()
    {
        createCamera();
        imageCount = 0;
        isCapturing = false;

        if (webCamTexture)
        {
            webCamTexture.Play();

            if (initial && webCamTexture.videoRotationAngle != 0)
            {
                initial = false;

                int mult = webCamTexture.videoRotationAngle < 0 ? -1 : 1;

                transform.Rotate(Vector3.forward * -webCamTexture.videoRotationAngle);

                float aspect = (float)Screen.width / (float)Screen.height;
                transform.localScale = new Vector3(1/aspect, mult*aspect, 1);
            }
        }

        timeoutCR = StartCoroutine(timeoutDetector());
    }

    void OnDisable()
    {
        if (webCamTexture)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }

        stopTimeoutCR();
    }

    private void stopTimeoutCR()
    {
        if (timeoutCR != null)
        {
            StopCoroutine(timeoutCR);
            timeoutCR = null;
        }
    }

    public void Capture()
    {
        if (isCapturing) return;

        imageCount++;
        isCapturing = true;    

        GetComponent<RawImage>().enabled = false;

        stopTimeoutCR();
        StartCoroutine(captureWait());
    }

    IEnumerator timeoutDetector()
    {
        yield return new WaitForSeconds(timeout);

        if (chat)
        {
            chat._AR_close();
        }
    }

    IEnumerator captureWait()
    {
        yield return new WaitForSeconds(darkFlashTime);

        GetComponent<RawImage>().enabled = true;
        isCapturing = false;

        if (imageCount >= imageCountThreshold)
        {
            if (chat) chat._AR_success();
        }
        else
        {            
            timeoutCR = StartCoroutine(timeoutDetector());
        }
    }

}
