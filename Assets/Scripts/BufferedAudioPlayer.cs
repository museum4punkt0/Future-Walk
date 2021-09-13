using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferedAudioPlayer : MonoBehaviour
{
    float[] audiodata;
    int position = 0;
    bool playing = false;
    float _volume = 1F;
    float volume { 
        get {return _volume;} 
        set {_volume = value;}
    }

    bool _loop;
    public bool loop {
        get {return _loop;}
        set {_loop = value;}
    }

    int samplerate = 0;
    int channels = 0;

    int samplerateRatio = 1;
    int interSampleCounter = 0;



    float _duration = 0F;
    public float duration {get {return _duration;}}

    string _assetName;
    public string assetName {get {return _assetName;}}


    public bool loadAudio(string asset)
    {
        //AudioClip clip = Tools.Load<AudioClip>(asset);
        AudioClip clip = Tools.LoadAudioClip(asset);

        if (clip)
        {
            audiodata = new float[clip.samples * clip.channels];

            clip.GetData(audiodata, 0);
            
            samplerate = (int)(clip.samples / clip.length);
            channels = clip.channels;
            _duration = clip.length;
            _assetName = asset;

            samplerateRatio = (3 - clip.channels) * (int)(((float)AudioSettings.outputSampleRate / (float)samplerate) + 0.5);

            clip.UnloadAudioData();
            Resources.UnloadAsset(clip);

            return true;
        }

        return false;
    }

    public void setVolume(float v)
    {
        if (v > 1F) v = 1f;
        else if (v < 0F) v = 0F;
        volume = v;
    }

    public void setPlaying(bool b)
    {
        playing = b;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (playing
            && volume > 0F)
        {
            int count = 0;
            while (count < data.Length)
            {
                data[count] += _volume * audiodata[position];

                if (samplerateRatio == 1)
                {
                    position++;
                }
                else
                {
                    interSampleCounter++;
                    if (interSampleCounter == samplerateRatio)
                    {
                        interSampleCounter = 0;
                        position++;
                    }
                }

                if (position >= audiodata.Length)
                {
                    if (_loop)
                    {
                        position = 0;
                        interSampleCounter = 0;
                    }
                    else
                    {    
                        playing = false;
                        return;
                    }                
                }

                count++;
            }
        }
    }
}
