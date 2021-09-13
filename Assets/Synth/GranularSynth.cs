/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com
*/

#define USE_ADDRESSABLES

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

#if USE_ADDRESSABLES
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

public class GranularSynth : MonoBehaviour
{
    public SharedGrainInfo grainInfo;
    private Channel leftChannel;
    private Channel rightChannel;
    public Sampler sampler;

    public SharedGrainInfo sharedInfo {get {return grainInfo;}}

    public ControlData controlData;

    static private System.Random sharedRandomNr = new System.Random();
    public static int RandomNumber(int min, int max)
    {
        return sharedRandomNr.Next(min, max);
    }

    public bool running = false;
    public float volume = 1f;

    protected void Awake()
    {
        grainInfo = new SharedGrainInfo();

        const bool isLeft = true;
        leftChannel = new Channel(isLeft, this);
        rightChannel = new Channel(!isLeft, this);

        sampler = new Sampler(this);

        controlData = new ControlData(this);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (running && grainInfo.gain > 0 && volume > 0)
        {
            int size = data.Length;
            float dry = 1.0f - sharedInfo.samplerWet;
            double sampleIndex = 0;
            float val = 0;

            for (int i = 0; i < size; i += channels)
            {
                sampleIndex = sampler.getNextSampleIndex();
                val = (float)leftChannel.getNextSample();

                data[i] = (float)(val * sharedInfo.samplerWet + sampler.getInterpolatedSampleValue(sampleIndex, true) * dry) * sharedInfo.gain * volume;

                data[i + 1] = sampler.getSampleChannelCount() == 1 ? data[i] : (float)(rightChannel.getNextSample() * sharedInfo.samplerWet + sampler.getInterpolatedSampleValue(sampleIndex, false) * dry) * sharedInfo.gain * volume;

                // TODO: clip the volume
            }
            controlData.update(size / sampler.getSampleChannelCount());
        }
    }


#if (!UNITY_IOS && !UNITY_ANDROID)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && sharedInfo.filePath != null)
        {
            running = !running;
        }
    }
#endif

    public void setSharedInfo(SharedGrainInfo info)
    {
        grainInfo.gain                 = info.gain;
        grainInfo.playbackSpeed        = info.playbackSpeed;
        grainInfo.samplerWet           = info.samplerWet;
        grainInfo.LFO1Rate             = info.LFO1Rate;
        grainInfo.LFO1Type             = info.LFO1Type;
        grainInfo.LFO2Rate             = info.LFO2Rate;
        grainInfo.LFO2Type             = info.LFO2Type;
        grainInfo.spread               = info.spread;
        grainInfo.grainSize            = info.grainSize;
        grainInfo.grainSizeLFO         = info.grainSizeLFO;
        grainInfo.grainSizeLFOAmount   = info.grainSizeLFOAmount;
        grainInfo.startPos             = info.startPos;
        grainInfo.startPosLFO          = info.startPosLFO;
        grainInfo.startPosLFOAmount    = info.startPosLFOAmount;
        grainInfo.randomStartPos       = info.randomStartPos;
        grainInfo.randomizeMorphAmt    = info.randomizeMorphAmt;
        grainInfo.morphAmount          = info.morphAmount;
        grainInfo.addHarmony           = info.addHarmony;
        grainInfo.harmonyType          = info.harmonyType;
        grainInfo.scanOn               = info.scanOn;
        grainInfo.scanDistance         = info.scanDistance;
        grainInfo.scanTime             = info.scanTime;

        controlData.updateLFOSettings();

        // update audio clip
        if (!string.IsNullOrWhiteSpace(info.filePath))
        {
            StartCoroutine(LoadAudioClipFromPath(info.filePath));
        }
    }


    public double getInterpolatedGrainValue(SampleLocationInfo locationInfo, bool isLeftChannel)
    {
        return sampler.getInterpolatedGrainValue(locationInfo, isLeftChannel);
    }


    private void UpdateClip(AudioClip newClip, string path, string fileName)
    {
        running = false;

        leftChannel.reset();
        rightChannel.reset();

        sampler.setAudioClip(newClip, path);

#if (!UNITY_IOS && !UNITY_ANDROID && !SYNTH_NO_GUI)
        GameObject GUIStuff = GameObject.Find("/Canvas/GUI");
        if (GUIStuff)
        {
            GUIStuff.GetComponent<GUIResponse>().setFileName(fileName);
        }
#endif

        running = true;
    }




#if USE_ADDRESSABLES
    private AsyncOperationHandle<AudioClip> _currentOperationHandle;

    private void ReleaseAudio()
    {
        if (_currentOperationHandle.IsValid())
        {
            Addressables.Release(_currentOperationHandle);
            _currentOperationHandle = default;
        }
    }
#endif


    protected IEnumerator LoadAudioClipFromPath(string path)
    {

        // try to load from resources first
        var arr = path.Split('/');
        string fn = Tools.removeSuffix(arr[arr.Length-1]);

#if USE_ADDRESSABLES

        ReleaseAudio();

        // load the clip
        var currentOperationHandle = Addressables.LoadAssetAsync<AudioClip>(fn);

        // wait for it to be loaded
        yield return currentOperationHandle;

        _currentOperationHandle = currentOperationHandle;

        // get the clip
        AudioClip clip = currentOperationHandle.Result;


#else

        Debug.Log("loading Resources: " + fn);

        AudioClip clip = Tools.Load<AudioClip>(fn);

#endif

        if (clip)
        {
            UpdateClip(clip, path, fn);

            // done with clip - cleanup
            clip.UnloadAudioData();

#if USE_ADDRESSABLES
            ReleaseAudio();
#else
            Resources.UnloadAsset(clip);
#endif

            yield return null;
        }
        else
        {
            // could not load file from resources
            // try loading from file

            Debug.Log("loading file from disk: " + path);

            string FullPath = "file:///" + path;

            AudioType type = AudioType.WAV;
            if (FullPath.ToLower().Contains(".wav"))
            {
                type = AudioType.WAV;
            }
            else if (FullPath.ToLower().Contains(".mp3"))
            {
                type = AudioType.MPEG;
            }
            else if (FullPath.ToLower().Contains(".aif") || FullPath.ToLower().Contains(".aiff"))
            {
                type = AudioType.AIFF;
            }
            else
            {
                yield return null;
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(FullPath, type))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError)
                {
                    Debug.Log(www.error + " path " + path);
                }
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);

                    if (clip)
                    {
                        UpdateClip(clip, path, fn);
                    }
                    else
                    {
                        // todo: if clip is empty, show error message
                        Debug.Log("can't load the file " + FullPath);
                    }
                }
            }
        }
    }

}
