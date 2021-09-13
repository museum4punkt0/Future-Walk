using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.Tween;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Threading;

[DisallowMultipleComponent]
public class AudioController : MonoBehaviour, IStoryMessageTarget
{
    //-----------------------------------------
    //-----------------------------------------
    public static AudioController instance;

    public static readonly bool DO_BUFFERED_AUDIO = false;

    public static float PlayAudio(string filename, bool loop = false)
    {
        if (instance)
        {
            // NOTE: buffered audio is disabled for now!
            if (DO_BUFFERED_AUDIO
                && instance.bufferedPlayers.ContainsKey(filename))
            {
                return instance.PlayBuffered(filename);
            }

            return instance.DoPlayAudio(filename, loop);
        }

        Log.e("No AudioController instance");

        return 0;
    }


    public static void StopAudio()
    {
        if (instance)
        {
            instance.DoStopAudio();
        }
    }

    //-----------------------------------------
    //-----------------------------------------

    [SerializeField] GranularSynth synthA;
    [SerializeField] GranularSynth synthB;
    [SerializeField] BufferedAudioPlayer bufferedAudioPrefab;

    float synthMix = 0;
    FloatTween currentSynthTween = null;
    bool synthAWasRunningBeforePause = false;
    bool synthBWasRunningBeforePause = false;
    bool audioWasRunningBeforePause = false;
    Dictionary<string, BufferedAudioPlayer> bufferedPlayers = new Dictionary<string, BufferedAudioPlayer>();

    private AudioSource speakerSource;
    private AudioSource backgroundSource;

    private AsyncOperationHandle<AudioClip> _currentOperationHandle;

    public void Awake()
    {
        instance = this;

        NativeToolkit.IOSSetAVAudioSessionPlayback();

        Log.d("system samplerate: " + AudioSettings.outputSampleRate);

        var audio_sources = GetComponents<AudioSource>();
        foreach (var item in audio_sources)
        {
            if (item.outputAudioMixerGroup.name == "Speaker")
            {
                speakerSource = item;
            }
            else if (item.outputAudioMixerGroup.name == "Synth")
            {
                backgroundSource = item;
            }
        }
    }

    public void Start()
    {
        Debug.Assert(synthA);
        Debug.Assert(synthB);

        synthA.volume = 0f;
        synthB.volume = 0f;
        synthA.running = false;
        synthB.running = false;
    }


    //------------------------------------------------------------------------
    // addressable audio
    //------------------------------------------------------------------------
    public void ReleaseAudio()
    {
        if (_currentOperationHandle.IsValid())
        {
            Addressables.Release(_currentOperationHandle);
            _currentOperationHandle = default;
        }
    }

    public void PlayAudioAddressable(string filename, Action<float> action)
    {
        ReleaseAudio();

        filename = Tools.removeSuffix(filename);

        if (GlobalSettings.instance.IsGerman)
        {
            DoLoadAddressable(filename + "_de", action);
        }
        else
        {
            DoLoadAddressable(filename, action);
        }
    }

    private void DoLoadAddressable(string name, Action<float> action)
    {
        Log.d("load addressable audio: " + name);

        var currentOperationHandle = Addressables.LoadAssetAsync<AudioClip>(name);

        currentOperationHandle.Completed += (AsyncOperationHandle<AudioClip> handle) =>
        {
            _currentOperationHandle = handle;

            if (!handle.Result)
            {
                if (GlobalSettings.instance.IsGerman
                    && name.EndsWith("_de"))
                {
                    // try loading english
                    DoLoadAddressable(name.Replace("_de", ""), action);
                }
                else if (action != null)
                {
                    action(0);
                }
            }
            else
            {
                DoPlayAudioClip(handle.Result);

                if (action != null)
                {
                    action(handle.Result.length);
                }
            }
        };
    }

    private bool DoPlayAudioClip(AudioClip clip, bool loop = false)
    {
        if (!clip) return false;

        var audio_source = GetComponent<AudioSource>();

        if (!audio_source)
        {
            audio_source = gameObject.AddComponent<AudioSource>() as AudioSource;
        }

        if (audio_source)
        {
            if (audio_source.clip)
            {
                audio_source.clip.UnloadAudioData();
            }

            // audio_source.PlayOneShot ?
            audio_source.clip = clip;
            audio_source.loop = loop;
            audio_source.Play();

            return true;
        }

        return false;
    }


    //------------------------------------------------------------------------
    // synth
    //------------------------------------------------------------------------
    public void LoadSynthSettings(string name, float time = 10, float delay = 0)
    {
        // INFO: this has to be done on the UI thread (asset loading, start tween)
        // (mobile screen is on, and app is in foreground)
        var asset = Tools.Load<TextAsset>(name);
        if (asset)
        {
            SharedGrainInfo info = JsonUtility.FromJson<SharedGrainInfo>(asset.text);
            if (info != null)
            {
                float targetValue = 1;
                if (synthMix < 0.5)
                {
                    synthB.setSharedInfo(info);
                }
                else
                {
                    synthA.setSharedInfo(info);
                    targetValue = 0;
                }

                if (currentSynthTween != null)
                {
                    currentSynthTween.Stop(TweenStopBehavior.DoNotModify);
                    currentSynthTween = null;
                }

                // transit
                System.Action<ITween<float>> doFade = (t) =>
                {
                    synthMix = t.CurrentValue;
                    synthA.volume = 1f-t.CurrentValue;
                    synthB.volume = t.CurrentValue;
                };

                System.Action<ITween<float>> doFadeCompleted = (t) =>
                {
                    synthMix = t.CurrentValue;
                    synthA.volume = 1f-t.CurrentValue;
                    synthB.volume = t.CurrentValue;
                    currentSynthTween = null;
                };

                currentSynthTween = gameObject.Tween("fadeSynth",
                                                    synthMix,
                                                    targetValue,
                                                    time,
                                                    TweenScaleFunctions.CubicEaseInOut,
                                                    doFade,
                                                    doFadeCompleted);                

                if (delay > 0)
                {
                    currentSynthTween.Pause();
                    Tools.RunThreadDelayed(delay, () => 
                    {
                        UnityToolbag.Dispatcher.InvokeAsync(() =>
                        {
                            if (currentSynthTween != null)
                            {
                                currentSynthTween.Start();
                            }
                        });
                    });
                }
                
            }
            else
            {
                Log.d("no grain info");
            }

            // cleanup
            Resources.UnloadAsset(asset);
        }
        else
        {
            Log.d("could not load setting: " + name);
        }
    }

    //------------------------------------------------------------------------
    //------------------------------------------------------------------------
    private void CleanupAudio(AudioSource audiosource)
    {
        if (audiosource.clip)
        {
            if (!_currentOperationHandle.IsValid()
                || _currentOperationHandle.Result != audiosource.clip)
            {
                audiosource.clip.UnloadAudioData();
                Resources.UnloadAsset(audiosource.clip);
                audiosource.clip = null;

                Resources.UnloadUnusedAssets();
            }
        }
    }


    public float DoPlayAudio(string filename, bool loop = false)
    {
        filename = Tools.removeSuffix(filename);

        if (GlobalSettings.instance.IsGerman)
        {
            filename += "_de";
        }

        var audio_source = GetComponent<AudioSource>();

        if (!audio_source)
        {
            audio_source = gameObject.AddComponent<AudioSource>() as AudioSource;
        }

		if (audio_source)
		{
            AudioClip clip = Tools.LoadAudioClip(filename);

            if (!clip
                && GlobalSettings.instance.IsGerman)
            {
                filename = filename.Replace("_de", "");
                clip = Tools.LoadAudioClip(filename);
            }

			if (clip)
            {
                CleanupAudio(audio_source);

                // audio_source.PlayOneShot ?
                audio_source.clip = clip;
                audio_source.loop = loop;
                audio_source.Play();

                return clip.length;
			}
        }

        return 0;
    }

    //------------------------------------------------------------------------
    // buffered
    //------------------------------------------------------------------------
    public void ClearBuffered()
    {
        foreach (var item in bufferedPlayers)
        {
            item.Value.setPlaying(false);
            Destroy(item.Value);
        }

        bufferedPlayers.Clear();
    }

    public BufferedAudioPlayer LoadBuffered(string filename, bool loop = false)
    {
        // Log.d("load buffered: " + filename + " - loop: " + loop);

        BufferedAudioPlayer player = Instantiate<BufferedAudioPlayer>(bufferedAudioPrefab);        
        if (player 
            && player.loadAudio(filename))
        {
            player.loop = loop;
            bufferedPlayers[filename] = player;
            return player;
        }
        else if (player)
        {
            Destroy(player);
            Log.d("could not load: " + filename);
        }

        return null;
    }

    public float PlayBuffered(string filename)
    {
        if (bufferedPlayers.ContainsKey(filename))
        {
            bufferedPlayers[filename].setPlaying(true);

            return bufferedPlayers[filename].duration;
        }

        return 0;
    }

    public bool isBuffered(string name)
    {
        return bufferedPlayers.ContainsKey(name);
    }

    public void StopBuffered(string filename)
    {
        if (bufferedPlayers.ContainsKey(filename))
        {
            var player = bufferedPlayers[filename];
            player.setPlaying(false);

            bufferedPlayers.Remove(filename);

            Destroy(player);
        }
    }

    public void StopBuffered(BufferedAudioPlayer player)
    {
        if (player)
        {
            player.setPlaying(false);
            bufferedPlayers.Remove(player.assetName);

            Destroy(player);
        }
    }


    //------------------------------------------------------------------------
    //------------------------------------------------------------------------
    public void DoStopAudio()
    {
        UnityToolbag.Dispatcher.InvokeAsync(() =>
        {
            var audio_source = GetComponent<AudioSource>();
            if (audio_source)
            {
                audio_source.Stop();

                CleanupAudio(audio_source);
            }
        });
    }

    public void Pause(bool state)
    {
        var audio_source = GetComponent<AudioSource>();        
		if (audio_source)
		{
            if (state)
            {
                audioWasRunningBeforePause = audio_source.isPlaying;
                synthAWasRunningBeforePause = synthA.running;
                synthBWasRunningBeforePause = synthB.running;
                audio_source.Pause();
                synthA.running = false;
                synthB.running = false;
            }
            else
            {
                if (synthAWasRunningBeforePause)
                {
                    synthA.running = true;
                }
                if (synthBWasRunningBeforePause)
                {
                    synthB.running = true;
                }
                if (audioWasRunningBeforePause)
                {
                    audio_source.Play();
                }
            }
        }
    }


    public void StopSynth()
    {
        if (synthA) synthA.running = false;
        if (synthB) synthB.running = false;
    }

    //----------------------------------------
    // IStoryMessageTarget
    //----------------------------------------
    public void Reset()
    {
        DoStopAudio();
        StopSynth();
    }
}
