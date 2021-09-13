using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using Cradle;
using System.Threading;
using System.Web;

[DisallowMultipleComponent]
[RequireComponent(typeof(IStoryVisualizer))]
[RequireComponent(typeof(StoryChat))] // todo remove this use Interface instead
public class StoryController : MonoBehaviour
{
    //-------------------------------------------------------------------
    //-------------------------------------------------------------------
    // static
    public static StoryController instance;
    public static bool debugLogging = true;


    //-------------------------------------------------------------------
    //-------------------------------------------------------------------

    [SerializeField] Toggle pauseToggle;
    [SerializeField] private List<string> firstStoryNames;
    [SerializeField] private bool forceStoryName;
    [SerializeField] GameObject debugOverlay;
    [SerializeField] GameObject debugScenes;
    [SerializeField] GameObject sceneButtonPrefab;
    [SerializeField] GameObject progressIndicator; // showing visited scenes
    [SerializeField] GameObject progressPrefab;
    [SerializeField] ScrollRect burgerHistoryScrollRect;
    [SerializeField] Button backToChoiceButton;

    private bool isPaused = false;
    private Story firstStory = default;
    private StoryChat textPlayer;
    private Dictionary<string, Story> storyMap = new Dictionary<string, Story>();
    public string CurrentScene { get { return _currentScene; } }
    private string _currentScene;

    private StoryHierarchy storyHierarchy;

    private bool hasFocus = true;
    public bool Focus { get { return hasFocus; } }

    List<string> waitingToEnterGeoFenceIds = new List<string>();
    List<string> waitingToLeaveGeoFenceIds = new List<string>();
    List<string> waitingForBeaconIds = new List<string>();
    ProgressItem lastProgressItem = default;

    // interruption
    [SerializeField] private string gpsInterruptionStory;
    [SerializeField] private string timerInterruptionStory;
    [SerializeField] private string manualInterruptionStory;
    
    private string gpsInterruptionFence;

    //-----------------------
    // properties
    public Dictionary<string, Story>.KeyCollection StoryNames
    {
        get
        {
            return storyMap.Keys;
        }
    }

    //-------------------------------------------------------------------
    // MonoBehaviour
    //-------------------------------------------------------------------    
    void Awake()
    {
        instance = this;

        if (pauseToggle)
        {
            pauseToggle.onValueChanged.AddListener(Pause);
        }
    }

    void Start()
    {
        // get StoryChat
        textPlayer = GetComponent<StoryChat>();
        storyHierarchy = GetComponent<StoryHierarchy>();

        if (storyMap.Count == 0
            && firstStoryNames.Count > 0)
		{
            foreach (var item in Tools.FindSubClassesOf<Cradle.Story>())
			{
                // populate StoryHierarchy-progress_indicator dictionary
               if (storyHierarchy) storyHierarchy.AddStoryToDictionary(item.Name);
            }

            var count  = 1;
			foreach (var item in Tools.FindSubClassesOf<Cradle.Story>())
			{
                var story = gameObject.AddComponent(item) as Cradle.Story;
                story.AutoPlay = false;

                if (item.Name == firstStoryNames[0]) firstStory = story;
                else if (item.Name == gpsInterruptionStory) textPlayer.GpsInterruptStory = story;
                else if (item.Name == timerInterruptionStory) textPlayer.TimerInterruptStory = story;
                else if (item.Name == manualInterruptionStory) textPlayer.ManualInterruptStory = story;


                storyMap.Add(item.Name, story);

                // add a button to debug view
                if (debugScenes)
                {
                    var buttonGO = Instantiate(sceneButtonPrefab, debugScenes.transform) as GameObject;
                    Button button = buttonGO.GetComponent<Button>();
                
                    button.onClick.AddListener(() => {
                        FactoryReset(item.Name, false);
                        debugOverlay.SetActive(false);
                    });
                    buttonGO.GetComponentInChildren<Text>(true).text = item.Name;
                }
                count++;
			}
		}

        InitiatePIGameState();

        // attach to geojson controller
        GeoJsonController.instance.AddOnEnterFence(onFenceEnteredCb);
        GeoJsonController.instance.AddOnExitFence(onFenceExitedCb);

        // attach to beacon controller
        BeaconController.instance.AddOnEnterBeacon(onBeaconEnteredCb);
        BeaconController.instance.AddOnExitBeacon(onBeaconExitedCb);
    }

    void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;
    }


    //-------------------------------------------------------------------
    // methods
    //-------------------------------------------------------------------

    // start the story
    // either last saved scene
    // or first story in list
    public void StartStory()
    {
        // get story player
        if (!forceStoryName
            && !string.IsNullOrEmpty(textPlayer.LastSavedScene()))
        {
            if (LoadScene(textPlayer.LastSavedScene())) return;
        }

        // check if we got first story
        if (firstStory)
        {
            if (LoadScene(firstStoryNames[0])) return;
        }

        // load first scene if we did not load yet
        if (storyMap.Count > 0)
        {
            var e = storyMap.Keys.GetEnumerator();
            e.MoveNext();
            if (LoadScene(e.Current)) return;
        }

        // could not load scene
        Log.e("could not load scene!");
    }

    public void Pause(bool state)
    {
        if (isPaused == state) return;

        Log.d("pause: " + state);

        isPaused = state;

        // pause all tool-threads
        Tools.Pause(state);

        // pause all story threads
        textPlayer.PauseCurrentStoryThreads(state);

        // pause audio
        AudioController.instance.Pause(state);
    }

    public void PauseStory()
    {
        textPlayer.PauseStory();
    }

    public void ResumeStory()
    {
        textPlayer.ResumeStory();
    }

    public void AddPostFocusAction(Action action)
    {
        textPlayer.AddPostFocusAction(action);
    }

    public bool StoryGoTo(string name)
    {
        return textPlayer.GoTo(name);
    }

    public Story GetStory()
    {
        return textPlayer.GetStory();
    }

    public void FactoryReset()
    {
        FactoryReset("");
    }

    public void FactoryReset(string scene, bool clearHistory = true)
    {
        Tools.CancelAllThreads();
        AudioController.instance.Reset();
        NotificationManager.instance.StopAll();
        BeaconController.instance.Reset();

        textPlayer.Reset(clearHistory);

        ClearFencesAndBeacons();

        if (string.IsNullOrEmpty(scene))
        {
            GlobalSettings.instance.Reset();
            MainPageController.instance.Reset();
        }
        else
        {
            if (string.IsNullOrEmpty(scene)
                && firstStoryNames.Count > 0)
            {
                scene = firstStoryNames[0];
            }

            LoadScene(scene);
        }

        // burger menu out
        BurgerMenuController.instance.Reset();
    }

    public void LoadMusemChooser()
    {
        LoadScene(GlobalSettings.MUSEUM_CHOOSER);
    }

    public bool LoadScene(string name)
    {
        if (!storyMap.ContainsKey(name))
        {
            Log.d("unknown scene: " + name);
            return false;            
        }
        
        // loaded scene starts right away
        // it may need the current scene name to load resources
        string old_scene = _currentScene;
        _currentScene = name;

        if (textPlayer.LoadStory(storyMap[name], name))
        {
            // cleanup beacon and fences
            ClearFencesAndBeacons();

            textPlayer.BeginStory();

            // update musem-choice button state
            if (backToChoiceButton)
            {
                UnityToolbag.Dispatcher.InvokeAsync(() =>
                {
                    bool enable = !firstStoryNames.Contains(name) && (GlobalSettings.instance.arcDoneCount() < GlobalSettings.ARC_COUNT);
                    int color = 234;
                    if (!enable) color = 140;

                    backToChoiceButton.interactable = enable;
                    Text[] texts = backToChoiceButton.GetComponentsInChildren<Text>(true);
                    foreach (var text in texts)
                    {
                        text.color = Tools.RGBColor(color, color, color);
                    }

                });
            }

            return true;
        }
        else
        {
            _currentScene = old_scene;
            Log.e("could not load scene: " + name);
        }

        return false;
    }

    private void ClearFencesAndBeacons()
    {
        lock (waitingToEnterGeoFenceIds)
        {
            waitingToEnterGeoFenceIds.Clear();
        }

        lock (waitingToLeaveGeoFenceIds)
        {
            waitingToLeaveGeoFenceIds.Clear();
        }

        lock (waitingForBeaconIds)
        {
            waitingForBeaconIds.Clear();
        }
    }

    // manually trigger timeout
    // find button in debugbuttons
    public void TriggerTimeoutTimer()
    {
        // is this correct? we may want to arrive in other fences
        // leave this for now - it is good enough for our purpose

        lock(waitingToEnterGeoFenceIds)
        {
            if (waitingToEnterGeoFenceIds.Count > 0)
            {
                Log.d("loading waitingToEnterGeoFenceIds[0]: " + string.Join(", ", waitingToEnterGeoFenceIds));

                LoadFenceScene(waitingToEnterGeoFenceIds[0]);
                waitingToEnterGeoFenceIds.Clear();
            }
        }

        lock(waitingToLeaveGeoFenceIds)
        {
            if (waitingToLeaveGeoFenceIds.Count > 0)
            {
                Log.d("loading waitingToLeaveGeoFenceIds[0]: " + string.Join(", ", waitingToLeaveGeoFenceIds));

                LoadFenceScene(waitingToLeaveGeoFenceIds[0]);
                waitingToLeaveGeoFenceIds.Clear();
            }
        }

        lock(waitingForBeaconIds)
        {
            if (waitingForBeaconIds.Count > 0)
            {
                Log.d("loading waitingForBeaconIds[0]: " + string.Join(", ", waitingForBeaconIds));

                LoadBeaconScene(waitingForBeaconIds[0]);
                waitingForBeaconIds.Clear();
            }
        }
    }

    //------------------------------------------------------------------
	// geo fence
	//------------------------------------------------------------------

    public void waitForFence(float backupTimerDuration, bool leaving, params string[] ids)
    {
#if UNITY_EDITOR
        // always override timeout in editor
        if (backupTimerDuration > 0)
        {
            backupTimerDuration = 1;
        }
#endif
        
        List<string> idList;

        if (leaving)
        {
            // trigger if leaving the fence
            idList = waitingToLeaveGeoFenceIds;
        }
        else
        {         
            idList = waitingToEnterGeoFenceIds;   
        }

        lock(idList)
        {
            // accumulate fences?
            idList.AddRange(ids);
            if (debugLogging) Log.d("waitForEnteringFence: " + string.Join(", ", idList) + " - " + backupTimerDuration);
            
            // pause story
            textPlayer.PauseStory();

            if (idList.Count > 0)
            {
                string insideId = "";

                if (!leaving)
                {
                    // check if we are already in one of the fences
                    foreach (var id in ids)
                    {
                        if (GeoJsonController.instance.hasEntered(id))
                        {
                            // already in geofence
                            Tools.RunThreadDelayed(0, () => 
                            {
                                // call callback directly
                                onFenceEnteredCb(id);
                            });

                            insideId = id;
                            break;
                        }
                    }
                }

                // start a timer in case we are not in one of the fences
                if (backupTimerDuration > 0
                    && string.IsNullOrEmpty(insideId))
                {
                    Tools.RunThreadDelayed((float)backupTimerDuration, () =>
                    {
                        lock (idList)
                        {                        
                            if (idList.Count > 0)
                            {
                                textPlayer.setBoolVariable("isfence", false);
                                textPlayer.setStringVariable("fenceid", "");
                                LoadFenceScene(idList[0]);
                            }

                            // is this correct? we may want to arrive in other fences
                            // leave this for now - it is good enough for our purpose
                            idList.Clear();
                        }
                    });
                }
            }
        }

    }

    //-------------------------------------------------------------------
    // geojson callbacks
    void onFenceEnteredCb(string id)
    {
        lock(waitingToEnterGeoFenceIds)
        {
            foreach (var fence in waitingToEnterGeoFenceIds)
            {
                if (id == fence)
                {
                    if (debugLogging) Log.d("arrived in fence: " + id);

                    textPlayer.setBoolVariable("isfence", true);
                    textPlayer.setStringVariable("fenceid", id);
                    LoadFenceScene(fence);
                    
                    // is this correct? we may want to arrive in other fences
                    // leave this for now - it is good enough for our purpose
                    waitingToEnterGeoFenceIds.Clear();

                    return;
                }
            }
        }
    }

    void onFenceExitedCb(string id)
    {
        lock(waitingToLeaveGeoFenceIds)
        {
            foreach (var fence in waitingToLeaveGeoFenceIds)
            {
                if (id == fence)
                {
                    if (debugLogging) Log.d("left fence: " + id);

                    textPlayer.setBoolVariable("isfence", true);
                    textPlayer.setStringVariable("fenceid", id);
                    LoadFenceScene(fence);

                    // is this correct? we may want to arrive in other fences
                    // leave this for now - it is good enough for our purpose
                    waitingToLeaveGeoFenceIds.Clear();

                    return;
                }
            }
        }
    }

    private void LoadFenceScene(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        // cancel backuptimer
        Tools.CancelAllThreads();

        if (!LoadScene(id))
        {
            // could not load scene - just continue
            bool was_delay_bubbles = textPlayer.AutoDelayBubbles;
            
            textPlayer.AutoDelayBubbles = false;
            ResumeStory();

            textPlayer.AutoDelayBubbles = was_delay_bubbles;
        }
    }

    //------------------------------------------------------------------
	// interruption fence
	//------------------------------------------------------------------
    public void startInterruptionFence(string id)
    {
        gpsInterruptionFence = id;
        if (!string.IsNullOrEmpty(gpsInterruptionFence))
        {
            GeoJsonController.instance.AddOnExitFence(CheckInterruptionFence);
        }
        else
        {
            GeoJsonController.instance.RemoveOnExitFence(CheckInterruptionFence);
        }
    }

    public void stopInterruptionFence()
    {
        gpsInterruptionFence = "";
        GeoJsonController.instance.RemoveOnExitFence(CheckInterruptionFence);
    }

    private void CheckInterruptionFence(string id)
    {
        if (!string.IsNullOrEmpty(gpsInterruptionFence)
            && id == gpsInterruptionFence)
        {
            // GPS interrupt!
            Log.d("GPS INTERRUPT: " + id);
            StoryChat.instance.GpsInterrupt();
        }
    }

    //------------------------------------------------------------------
	// bluetooth beacon
	//------------------------------------------------------------------

    public void waitForBeacon(float backupTimerDuration, params string[] ids)
    {
#if UNITY_EDITOR
        // always override timeout in editor
        if (backupTimerDuration > 0)
        {
            backupTimerDuration = 1;
        }
#endif

        lock (waitingForBeaconIds)
        {
            waitingForBeaconIds.AddRange(ids);

            if (true || debugLogging) Log.d("waitForEnteringBeacon: " + string.Join(", ", waitingForBeaconIds) + " - " + backupTimerDuration);
            
            if (waitingForBeaconIds.Count > 0)
            {
                // pause story
                textPlayer.PauseStory();

                string insideId = "";

                foreach (var id in waitingForBeaconIds)
                {
                    if (BeaconController.instance.HasEntered(id))
                    {
                        // already in geofence
                        Tools.RunThreadDelayed(0.1F, () => 
                        {
                            // call callback directly
                            onBeaconEnteredCb(id);
                        });

                        insideId = id;
                        break;
                    }
                }

                // start a timer in case we are not in one of the fences
                if (backupTimerDuration > 0
                    && string.IsNullOrEmpty(insideId))
                {
                    Tools.RunThreadDelayed(backupTimerDuration, () =>
                    {
                        lock(waitingForBeaconIds)
                        {
                            if (waitingForBeaconIds.Count > 0)
                            {
                                textPlayer.setBoolVariable("isbeacon", false);
                                textPlayer.setStringVariable("beaconid", "");
                                LoadBeaconScene(waitingForBeaconIds[0]);
                            }
                        }
                    });

                    // start monitoring
                    foreach (var id in waitingForBeaconIds)
                    {
                        BeaconController.instance.Monitor(id);
                    }
                }
            }
        }
    }

    private void LoadBeaconScene(string id)
    {
        BeaconController.instance.Reset();

        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        if (!LoadScene(id))
        {
            // could not load scene - just continue
            ResumeStory();
        }

        // cancel backuptimer
        // Tools.CancelAllThreads();

        lock(waitingForBeaconIds)
        {
            waitingForBeaconIds.Clear();
        }
    }

    //-------------------------------------------------------------------
    // BeaconController callbacks
    void onBeaconEnteredCb(string id)
    {
        lock(waitingForBeaconIds)
        {
            foreach (var fence in waitingForBeaconIds)
            {        
                if (id == fence)
                {
                    if (debugLogging) Log.d("arrived in Beacon: " + id + " - " + Thread.CurrentThread.ManagedThreadId);

                    textPlayer.setBoolVariable("isbeacon", true);
                    textPlayer.setStringVariable("beaconid", fence);
                    LoadBeaconScene(fence);                

                    return;
                }
            }
        }
    }

    void onBeaconExitedCb(string id)
    {
    }


    //------------------------------------------------------------------
	// GPS noise
	//------------------------------------------------------------------

    string noiseFenceId = "";
    BufferedAudioPlayer bufferedAudio;
    BufferedAudioPlayer bufferedNoise;

    public void startGpsNoise(string id, string audio)
    {
        GpsController.instance.StartLocationService();

        noiseFenceId = id;

        // load buffered audio
        if (bufferedAudio)
        {
            AudioController.instance.StopBuffered(bufferedAudio);
            bufferedAudio = null;
        }

        bufferedAudio = AudioController.instance.LoadBuffered(audio, true);
        if (bufferedAudio)
        {
            bufferedAudio.setPlaying(true);
        }


        if (bufferedNoise)
        {
            AudioController.instance.StopBuffered(bufferedNoise);
            bufferedNoise = null;
        }
        bufferedNoise = AudioController.instance.LoadBuffered("/noise", true);
        if (bufferedNoise)
        {
            bufferedNoise.setPlaying(true);
        }

        // set initial volumes
        var dist = GeoJsonController.instance.distanceToNormalized(noiseFenceId, GpsController.instance.currentLocationAsPO());

        // if fence does not exist, only play noise
        if (dist < 0) dist = 1;

        if (bufferedAudio)
        {
            bufferedAudio.setVolume(1F-(float)dist);
        }

        if (bufferedNoise)
        {
            bufferedNoise.setVolume((float)dist);
        }    

        // remove and add event listener
        GpsController.instance.OnLocationUpdated -= this.OnLocationUpdated;
        GpsController.instance.OnLocationUpdated += this.OnLocationUpdated;
    }

    public void PlayNoiseAudio(string audio)
    {
        if (bufferedAudio)
        {
            AudioController.instance.StopBuffered(bufferedAudio);
            bufferedAudio = null;
        }

        bufferedAudio = AudioController.instance.LoadBuffered(audio, true);

        // set initial volumes
        var dist = GeoJsonController.instance.distanceToNormalized(noiseFenceId, GpsController.instance.currentLocationAsPO());

        // if fence does not exist, only play noise
        if (dist < 0) dist = 1;

        if (bufferedAudio)
        {
            bufferedAudio.setVolume(1F-(float)dist);
            bufferedAudio.setPlaying(true);
        }
        if (bufferedNoise)
        {
            bufferedNoise.setVolume((float)dist);
        }
    }

    public void stopGpsNoise()
    {
        GpsController.instance.OnLocationUpdated -= this.OnLocationUpdated;

        noiseFenceId = "";

        if (bufferedAudio)
        {
            AudioController.instance.StopBuffered(bufferedAudio);
            bufferedAudio = null;
        }

        if (bufferedNoise)
        {
            AudioController.instance.StopBuffered(bufferedNoise);
            bufferedNoise = null;
        }
    }


    // interactive fence
    void OnLocationUpdated(GeoJSON.PositionObject pos)
    {
        var dist = GeoJsonController.instance.distanceToNormalized(noiseFenceId, pos);
        
        //Log.d("storycontroller:OnLocationUpdated: " + dist + " - thread: " + Thread.CurrentThread.ManagedThreadId);
        
        if (dist < 0)
        {
            // no fence with id... stop
            stopGpsNoise();
            return;
        }

        if (bufferedAudio)
        {
            bufferedAudio.setVolume(1F-(float)dist);
        }

        if (bufferedNoise)
        {
            bufferedNoise.setVolume((float)dist);
        }        
    }

    //--------------------------------------------------------------------
    // progress indicator
    //--------------------------------------------------------------------

    public void ProgressReveal(int passageIndex, bool highlight = false)
    {
        // this may run on a different thread
        // make sure to run this on the main-thread
        UnityToolbag.Dispatcher.InvokeAsync(() =>
        {
            foreach (Transform child in progressIndicator.GetComponentsInChildren(typeof(Transform)))
            {
                ProgressItem pi = child.GetComponent<ProgressItem>();

                // todo check visited items
                if (pi && pi.index == passageIndex)
                {
                    if (GlobalSettings.instance.IsEnglish)
                    {
                        pi.GetComponentInChildren<Text>(true).text = pi.realnameEN;
                    }
                    else
                    {
                        pi.GetComponentInChildren<Text>(true).text = pi.realnameDE;
                    }

                    if (highlight)
                    {
                        pi.GetComponentInChildren<Text>(true).color = Style.white;
                    }
                    else
                    {
                        pi.GetComponentInChildren<Text>(true).color = Style.actionLight;
                    }
                }
            }
        });
    }

    // check in global settings => appsettings.arcsdone
    public void ProgressCurrent(int passageIndex)
    {
        // this may run on a different thread
        // make sure to run this on the main-thread
        UnityToolbag.Dispatcher.InvokeAsync(() =>
        {
            foreach (Transform child in progressIndicator.GetComponentsInChildren(typeof(Transform)))
            {
                ProgressItem pi = child.GetComponent<ProgressItem>();

                // todo check visited items
                if (pi && pi.index == passageIndex)
                {
                    //pi.GetComponentInChildren<Text>(true).text = Tools.randomString(UnityEngine.Random.Range(10, 16));
                    pi.GetComponentInChildren<Text>(true).color = Style.actionLight;
                }

                if (pi && passageIndex > 0 && pi.index == passageIndex -1)
                {
                    pi.GetComponentInChildren<Text>(true).color = Style.white;
                }
            }
        });
    }


    private void InitiatePIGameState()
    {
        if (progressIndicator)
        {
            List<int> completedStories = CheckForCompletedStories();

            foreach (Transform child in progressIndicator.GetComponentsInChildren(typeof(Transform)))
            {
                ProgressItem pi = child.GetComponent<ProgressItem>();

                if (pi)
                {
                    if (completedStories.Contains(pi.index))
                    {
                        ProgressReveal(pi.index,true);
                    }
                    else
                    {
                        pi.GetComponentInChildren<Text>(true).text = Tools.randomString(UnityEngine.Random.Range(10, 16));
                        pi.GetComponentInChildren<Text>(true).color = Style.darkGray;    
                    }
                }
            }
        }
    }

    private List<int> CheckForCompletedStories()
    {
        List<int> completedStories = new List<int>();

        // check onboarding
        if (StoryChat.instance.ChatLog.lastScene != null)
        {
            completedStories.Add(0);
            completedStories.Add(1);
        }

        // check completed arcs
        if (GlobalSettings.instance.isArcDone(GlobalSettings.NAME_GG_EN) || GlobalSettings.instance.isArcDone(GlobalSettings.NAME_GG_DE))
        {
            completedStories.Add(2);
            completedStories.Add(3);
            completedStories.Add(4);
            completedStories.Add(5);
        }

        if (GlobalSettings.instance.isArcDone(GlobalSettings.NAME_KGM_EN) || GlobalSettings.instance.isArcDone(GlobalSettings.NAME_KGM_DE))
        {
            completedStories.Add(9);
            completedStories.Add(10);
            completedStories.Add(11);
        }

        if (GlobalSettings.instance.isArcDone(GlobalSettings.NAME_MIM_EN) || GlobalSettings.instance.isArcDone(GlobalSettings.NAME_MIM_DE))
        {
            completedStories.Add(6);
            completedStories.Add(7);
            completedStories.Add(8);
        }

        if (GlobalSettings.instance.isArcDone(GlobalSettings.NAME_OUT_EN) || GlobalSettings.instance.isArcDone(GlobalSettings.NAME_OUT_DE))
        {
            completedStories.Add(12);
            completedStories.Add(13);
            completedStories.Add(14);
            completedStories.Add(15);
        }
  
        // check wip arc
        switch(StoryChat.instance.ChatLog.lastScene)
        {
            case "gg_1":
                completedStories.Add(2);
                break;
            case "gg_2":
                completedStories.Add(2);
                completedStories.Add(3);
                break;
            case "kgm_1":
                completedStories.Add(9);
                break;
            case "kgm_2":
                completedStories.Add(9);
                completedStories.Add(10);
                break;
            case "arc4_wurlitzer_success":
                completedStories.Add(6);
                break;
            case "arc4_trautonium":
                completedStories.Add(6);
                break;
            case "arc4_cembalo":
                completedStories.Add(6);
                completedStories.Add(7);
                break;
            case "E_OUTSIDE_3":
                completedStories.Add(12);
                break;
            case "E_OUTSIDE_4":
                completedStories.Add(12);
                completedStories.Add(13);
                break;
            case "E_OUTSIDE_5":
                completedStories.Add(12);
                completedStories.Add(13);
                break;
            case "E_OUTSIDE_6":
                completedStories.Add(12);
                completedStories.Add(13);
                break;
            case "E_OUTSIDE_7":
                completedStories.Add(12);
                completedStories.Add(13);
                break;
            case "E_OUTSIDE_9":
                completedStories.Add(12);
                completedStories.Add(13);
                break;
            case "E_OUTSIDE_10":
                completedStories.Add(12);
                completedStories.Add(13);
                completedStories.Add(14);
                break;
            case "E_OUTSIDE_11":
                completedStories.Add(12);
                completedStories.Add(13);
                completedStories.Add(14);
                completedStories.Add(15);
                break;
            case "E_OUTSIDE_13":
                completedStories.Add(12);
                completedStories.Add(13);
                completedStories.Add(14);
                completedStories.Add(15);
                break; 
            default: break;
        }

        return completedStories;
    }
}
