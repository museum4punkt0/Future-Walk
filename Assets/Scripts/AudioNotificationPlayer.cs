using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NotificationManager))]
public class AudioNotificationPlayer : MonoBehaviour
{
    [SerializeField] string clipName;
    [SerializeField] AudioClip notificationAudio24K;
    [SerializeField] AudioClip notificationAudio32K;
    [SerializeField] AudioClip notificationAudio441K;
    [SerializeField] AudioClip notificationAudio48K;
    [SerializeField] AudioClip notificationAudio24K_DE;
    [SerializeField] AudioClip notificationAudio32K_DE;
    [SerializeField] AudioClip notificationAudio441K_DE;
    [SerializeField] AudioClip notificationAudio48K_DE;
    
    float[] audiodata;
    int position = 0;
    bool playing = false;    
    int channelCount = 1;
    float length = 0;

    void Start()
    {
        var clip = notificationAudio48K;

        if (GlobalSettings.instance.IsGerman)
        {
            clip = notificationAudio48K_DE;
        }

        if (AudioSettings.outputSampleRate == 24000)
        {
            if (GlobalSettings.instance.IsEnglish) clip = notificationAudio24K;
            else clip = notificationAudio24K_DE;
        }
        else if (AudioSettings.outputSampleRate == 32000)
        {
            if (GlobalSettings.instance.IsEnglish) clip = notificationAudio32K;
            else clip = notificationAudio32K_DE;
        }
        else if (AudioSettings.outputSampleRate == 44100)
        {
            if (GlobalSettings.instance.IsEnglish) clip = notificationAudio441K;
            else clip = notificationAudio441K_DE;
        }

        if (clip)
        {
            audiodata = new float[clip.samples * clip.channels];
            channelCount = clip.channels;
            length = clip.length;
            clip.GetData(audiodata, 0);           
        }
        else
        {
            Log.e("no clip for: " + clipName);
        }

        if (notificationAudio24K) notificationAudio24K.UnloadAudioData();
        if (notificationAudio32K) notificationAudio32K.UnloadAudioData();
        if (notificationAudio48K) notificationAudio48K.UnloadAudioData();
        if (notificationAudio24K_DE) notificationAudio24K_DE.UnloadAudioData();
        if (notificationAudio32K_DE) notificationAudio32K_DE.UnloadAudioData();
        if (notificationAudio48K_DE) notificationAudio48K_DE.UnloadAudioData();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (playing)
        {
            int count = 0;
            while (count < data.Length)
            {
                data[count] += audiodata[position];

                if (channelCount < channels)
                {
                    count++;
                    if (count < data.Length)
                    {
                        data[count] += audiodata[position];
                    }
                }

                position++;

                if (position >= audiodata.Length)
                {
                    position = 0;
                    playing = false;
                    return;
                }

                count++;
            }
        }
    }

    public float PlayNotification()
    {
        if (!playing)
        {
            Log.d("start notification: " + clipName);

            // start from beginning
            position = 0;
            playing = true;
        }

        return length;
    }

    public void Stop()
    {
        playing = false;
    }

    public string GetName()
    {
        return clipName;
    }
}
