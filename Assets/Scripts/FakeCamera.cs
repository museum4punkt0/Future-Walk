using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeCamera : MonoBehaviour, IARSuccessProvider
{

    [SerializeField] int imageCountThreshold = 4;
    [SerializeField] float idleTimeout = 30;
    [SerializeField] float darkFlashTime = 0.15F;
    [SerializeField] GameObject switchGO;
    [SerializeField] Text imageCountdown;
    [SerializeField] GameObject cameraButton;
    [SerializeField] GameObject sendingText;
    [SerializeField] GameObject firstTimeText;


    AudioSourceLanguage idleTimeoutAudio;
    int imageCount = 0;
    bool isCapturing = false;
    Coroutine timeoutCR = default;

    // IARSuccessProvider
    public bool Success => imageCount >= imageCountThreshold;

    void Awake()
    {
        idleTimeoutAudio = GetComponent<AudioSourceLanguage>();
    }

    void OnEnable()
    {
        imageCount = 0;
        isCapturing = false;

        if (!GlobalSettings.instance.firstTimeFakeCamera)
        {
            timeoutCR = StartCoroutine(IdleTimeoutDetector());
        }

        if (cameraButton) cameraButton.SetActive(true);
        if (sendingText) sendingText.SetActive(false);
        if (firstTimeText) firstTimeText.SetActive(true);

        UpdateCountdownText();
    }

    void OnDisable()
    {
        StopIdleTimeoutCR();
    }

    private void UpdateCountdownText()
    {
        if (imageCountdown)
        {
            imageCountdown.text = imageCount + "/" + imageCountThreshold;
        }
    }

    private void StopIdleTimeoutCR()
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
        if (firstTimeText) firstTimeText.SetActive(false);

        isCapturing = true;
        switchGO.SetActive(false);

        StopIdleTimeoutCR();
        StartCoroutine(CaptureWait());
    }


    IEnumerator IdleTimeoutDetector()
    {
        yield return new WaitForSeconds(idleTimeout);

        idleTimeoutAudio.Play();
        GlobalSettings.instance.firstTimeFakeCamera = true;
    }

    IEnumerator CaptureWait()
    {
        yield return new WaitForSeconds(darkFlashTime);

        switchGO.SetActive(true);
        isCapturing = false;

        UpdateCountdownText();

        if (imageCount >= imageCountThreshold)
        {
            // show "uploading"...
            if (cameraButton) cameraButton.SetActive(false);
            if (sendingText) sendingText.SetActive(true);
            StartCoroutine(DelayedSuccess());
        }
        else if (!GlobalSettings.instance.firstTimeFakeCamera)
        {
            timeoutCR = StartCoroutine(IdleTimeoutDetector());
        }

    }

    IEnumerator DelayedSuccess()
    {
        yield return new WaitForSeconds(1);
        if (StoryChat.instance) StoryChat.instance._AR_success();
    }
}
