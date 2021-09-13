using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cradle;

[DisallowMultipleComponent]
[RequireComponent(typeof(ChoiceButtonMover))]
public class StoryChat : MonoBehaviour, IStoryPlayerListener, IStoryVisualizer
{
	//-----------------------------------------
	//-----------------------------------------
	public static StoryChat instance;

	public static bool _debugLogging = false;	

	private static string CHATLOG_FILE = "";

	private const string AR_SUCCESS = ChoiceButtonMover.AR_PREFIX + "success";
	private const string AR_CLOSE = ChoiceButtonMover.AR_PREFIX + "close";

	private const string FC_SUCCESS = ChoiceButtonMover.FC_PREFIX + "success";
	private const string FC_CLOSE = ChoiceButtonMover.FC_PREFIX + "close";

	public static bool ShowEntry(ChatEntry entry)
	{
		return !GlobalSettings.instance || (GlobalSettings.instance.showAudioTranscript
			|| (entry.type != ChatEntry.ChatEntryType.AndreaAudio));
	}


	//-----------------------------------------
	//-----------------------------------------

	[SerializeField] GameObject talkingItem;
	[SerializeField] GameObject andreaGlitchOverlay;
	[SerializeField] GameObject mapButton;
	[SerializeField] ImageOverlay imageOverlay;
	[SerializeField] BottomScrollerView scrollView = default; // container for content


	private ChoiceButtonMover choiceButtonMover;

	// NOTE:
	// active story player
	// interrupts (timer/GPS) may start another story setting the
	// the activeStoryPlayer
	//
	// TODO: consider using a stack of stories
	private StoryPlayer activeStoryPlayer;

	// current storyPlayer last loaded story
	private StoryPlayer currentStoryPlayer;

	// cache output if app does not have focus
	// e.g.: app not in foreground or screen is locked
	readonly List<OutputType> cachedOutput = new List<OutputType>();


	// INFO:
	// postFocusActions are used for phone permission settings
	// app may loose focus if system dialogs pop up
	readonly List<Action> postFocusActions = new List<Action>();


	bool audioIsPlaying = false;

	// delay dependend on bubble-content / character-count
	bool autoDelayBubbles = true;
	public bool AutoDelayBubbles {
		get => autoDelayBubbles;
		set => autoDelayBubbles = value;
	}

	private ChatLog chatLog;
	List<ChatEntry> scrollerEntries = new List<ChatEntry>();


	bool hasFocus = true;
	string lastMap = "";

	// interrupt
	private Story _gpsInterruptionStory;
	private Story _timerInterruptionStory;
	private Story _manualInterruptionStory;
	private StoryPlayer interruptStoryPlayer;
	private Timer idleTimer = null;

	public ChatLog ChatLog
	{
		get
		{
			return chatLog;
		} 
	}

	public Story GpsInterruptStory {
		set => _gpsInterruptionStory = value;
	}

	public Story TimerInterruptStory
    {
		set => _timerInterruptionStory = value;
    }

	public Story ManualInterruptStory
    {
		set => _manualInterruptionStory = value;
    }
	

	public bool IsAndrea
	{
		set
		{
			if (activeStoryPlayer != null) activeStoryPlayer.IsAndrea = value;
		}
	}


    //----------------------------------------
	// MonoBehaviour
	//----------------------------------------
	void Awake()
	{
		instance = this;
		CHATLOG_FILE = Application.persistentDataPath + "/chatHistory.json";

		Log.d("CHATLOG_FILE: " + CHATLOG_FILE);

		if (!Application.isPlaying)
			return;

		ClearChoices();

		// load chatlog
		if (File.Exists(CHATLOG_FILE)) 
		{
			chatLog = JsonUtility.FromJson<ChatLog>(File.ReadAllText(CHATLOG_FILE));
		}
		else
		{
			chatLog = new ChatLog();
		}

		if (talkingItem) talkingItem.SetActive(false);		

		if (andreaGlitchOverlay)
		{
            andreaGlitchOverlay.SetActive(false);
            andreaGlitchOverlay.GetComponentInChildren<Button>(true).gameObject.SetActive(false);
        }

		if (mapButton)
        {
			mapButton.SetActive(false);
        }

		choiceButtonMover = GetComponent<ChoiceButtonMover>();
    }


	void Start()
	{
		if (imageOverlay) imageOverlay.Hide();

        //Log.d("start update content!");
        UpdateScrollerEntries();
    }



	void OnApplicationFocus(bool focus)
	{
        hasFocus = focus;

		if (hasFocus)
		{
			//---------------------------
			// execute post focus actions
			foreach (var action in postFocusActions)
			{
				if (action != null) action();
			}
			postFocusActions.Clear();


			//---------------------------
			// show all cached outputs

			bool added_direct = false;
			autoDelayBubbles = false;

			List<ChatEntry> entriesToInsert = new List<ChatEntry>();

			foreach (var output in cachedOutput)
			{
				// Log.d("CACHED: " + output.ToString());

				// add bubbles directly to the scroller
				// do this without animation
				if (output is OutputBubble)
				{
					added_direct = true;
					entriesToInsert.Add(new ChatEntry(((OutputBubble)output).type, (output as OutputBubble).text));
				}
				else if (output is OutputAudio)
				{
					// ignore
				}
				else if (output is OutputImage)
				{
					var tag = (output as OutputImage).tag;

					if (tag.Text.StartsWith("<img"))
					{
						added_direct = true;

						string image_url = Tools.GetHtmlAttributeContent(tag.Text, "src");
						if (!string.IsNullOrEmpty(image_url))
						{
							entriesToInsert.Add(new ChatEntry(ChatEntry.ChatEntryType.Image, image_url));
						}
					}
					else if(tag.Text.StartsWith("<Map"))
					{
						added_direct = true;

						string image_url = Tools.GetHtmlAttributeContent(tag.Text, "src");
						if (!string.IsNullOrEmpty(image_url))
						{
							entriesToInsert.Add(new ChatEntry(ChatEntry.ChatEntryType.Map, image_url));
						}
					}
				}
				else if (output is OutputArtwork)
				{
					added_direct = true;

					string art_id = (output as OutputArtwork).text;

					if (!string.IsNullOrEmpty(art_id)
						&& ArtworkInfoController.instance.Contains(art_id))
					{
						entriesToInsert.Add(new ChatEntry(ChatEntry.ChatEntryType.Artwork, art_id));
					}
				}
				else if (output is OutputLinks)
				{
					// just show
					ShowLinks((output as OutputLinks).links);
				}

			} // foreach cachedOutput


			lock(scrollerEntries)
            {
				lock(chatLog)
				{
					foreach (var item in entriesToInsert)
					{
						chatLog.entries.Insert(chatLog.entries.Count, item);

						if (ShowEntry(item))
						{
							scrollerEntries.Add(item);
						}
					}
				}

				if (added_direct && scrollView)
				{
					Log.d("update on focus");
					scrollView.SetContent(scrollerEntries);
				}
            }


			cachedOutput.Clear();

			autoDelayBubbles = true;

			// NOTE:
			// this is not really correct:
			// we do not know if the story gets resumed in the running task
			if (!audioIsPlaying 
				&& activeStoryPlayer != null 
				&& activeStoryPlayer.ThreadCount == 0
				&& Tools.ThreadCount() == 0)
			{
				ResumeStory();
			}
		}
	}
	
	void OnDestroy()
	{
		if (!Application.isPlaying)
			return;

		CleanupStory();
	}





	public void UpdateScrollerEntries()
    {
		lock(scrollerEntries)
        {
			scrollerEntries.Clear();
		
			lock(chatLog)
			{
				foreach (var item in chatLog.entries)
				{
					if (ShowEntry(item))
					{
						scrollerEntries.Add(item);
					}
				}
			}

			Log.d("try set content: " + scrollView);
			scrollView?.SetContent(scrollerEntries);
        }

	}


	//----------------------------------------
    // implement IStoryPlayerListener
    //----------------------------------------
	public void ShowBubble(string text, bool resume = false)
	{
        Log.d("show text bubble: " + text + " - " + Thread.CurrentThread.ManagedThreadId);

        ResetIdleTimer();

		if (!hasFocus)
		{
			Log.d("add to cache");
			lock (cachedOutput)
			{				
				cachedOutput.Add(new OutputBubble(text, getChatEntryType()));
			}
			return;
		}

		PauseStory();

		if (resume)
		{
			addChatBubble(getChatEntryType(), text, () =>
			{
				// Log.d("resume story after bubble: " + activeStoryPlayer.State + " : " + Thread.CurrentThread.ManagedThreadId);
				ResumeStory();
			});
		}
		else
		{
			// Log.d("!!!! not resuming story after bubble! " + activeStoryPlayer.State);
			addChatBubble(getChatEntryType(), text);
		}
	}

	public void PlayAudio(string audioToPlay, float initialDelay = 0)
	{
		if (string.IsNullOrEmpty(audioToPlay)) return;


		// pause story
        // we resume after audio finished playing
		PauseStory();

		bool audioIsNotification = NotificationManager.instance.HasNotification(audioToPlay);
		Log.d("play audio: " + audioToPlay + " - notification: " + audioIsNotification + " - " + Thread.CurrentThread.ManagedThreadId);

		if (!hasFocus && !audioIsNotification)
		{
			// we can not continue!
			// no audioloading if we do not have the application focus

			// play a notification (default) sound
			NotificationManager.instance.PlayNotification();

			// add a "continue" button
			UnityToolbag.Dispatcher.InvokeAsync(() => 
			{
				choiceButtonMover.AddChoice(GlobalSettings.instance.IsEnglish ? GlobalSettings.BUTTON_CONTINUE_EN : GlobalSettings.BUTTON_CONTINUE_DE, (content) =>
				{
					PlayAudio(audioToPlay);
				});
			});

			// (ab)use this flag to avoid resuming if app becomes focused
			audioIsPlaying = true;

			return;
		}

		// NOTE: 
		// one way to work around unitys non-multithreaded-ness is
		// to preload audio to ram (buffered audio).
		//
		// buffered audio is disabled until we need it.
		// notifications are another way to work around this limitation.
		// look at NotificationManager

		if (audioIsNotification)
		{
			float audioLength = NotificationManager.instance.PlayNotification(audioToPlay);
			AudioStarted(audioLength, initialDelay);
		}
		else
		{
			if (AudioController.DO_BUFFERED_AUDIO
				&& AudioController.instance.isBuffered(audioToPlay))
			{
				// playback buffered audio
				// diasbled for now
				float audioLength = AudioController.instance.PlayBuffered(audioToPlay);
				AudioStarted(audioLength, initialDelay);
			}
			else
			{
				// make sure to call this from the main thread
				UnityToolbag.Dispatcher.InvokeAsync(() =>
				{
					AudioController.instance.PlayAudioAddressable(audioToPlay, (float length) =>
					{
						if (length > 0)
						{
							AudioStarted(length, initialDelay);
						}
						else
						{
							Log.d("!!! - Load audio the old way!");
							// try conventional way
							UnityToolbag.Dispatcher.InvokeAsync(() =>
							{
								float audioLength = AudioController.PlayAudio(Tools.removeSuffix(audioToPlay));
								AudioStarted(audioLength, initialDelay);
							});
						}
					});
				});
			}
		}
	}

	private void AudioStarted(float length, float initialDelay)
	{
		audioIsPlaying = true;
		if (activeStoryPlayer != null) activeStoryPlayer.AudioStarted(length);

		if (initialDelay > 0)
		{
			Tools.RunThreadDelayed(initialDelay, () =>
			{
				// turn on talking-item
				EnableTalkingItem(true);
			});
		}
		else
		{
			// turn on talking-item
			EnableTalkingItem(true);
		}
	}

	public void EnableTalkingItem(bool value)
	{
		if (talkingItem) 
		{
			UnityToolbag.Dispatcher.InvokeAsync(() => 
			{
				string talking_text = "";

				if (activeStoryPlayer == null
					|| !activeStoryPlayer.IsAndrea)
				{
					if (GlobalSettings.instance.IsEnglish)
					{
						talking_text = GlobalSettings.MOTOKO_NAME + GlobalSettings.IS_TALKING_EN;
					}
                    else
                    {
						talking_text = GlobalSettings.MOTOKO_NAME + GlobalSettings.IS_TALKING_DE;
					}
					
				}
				else
				{
					if (GlobalSettings.instance.IsEnglish)
					{
						talking_text = GlobalSettings.ANDREA_NAME + GlobalSettings.IS_TALKING_EN;
					}
					else
					{
						talking_text = GlobalSettings.ANDREA_NAME + GlobalSettings.IS_TALKING_DE;
					}
				}

				talkingItem.GetComponentInChildren<Text>().text = talking_text;
				talkingItem.SetActive(value && !string.IsNullOrWhiteSpace(talking_text));
			});
		}
	}

	public void ShowImage(HtmlTag tag)
	{
		// <img src="Vermeer_AR_cut_rgb"/>
		// <Map src="map_room3"/>

		string image_url = Tools.GetHtmlAttributeContent(tag.Text, "src");

		if (string.IsNullOrEmpty(image_url)) return;

		if (image_url.StartsWith("http"))
		{
			Log.d("can not load image from http...");
			return;
		}

		Log.d("show image bubble: " + image_url + " - " + Thread.CurrentThread.ManagedThreadId);

		// 
		NotificationManager.instance.PlayNotification();

		if (!hasFocus)
		{
			Log.d("add to cache");

			lock (cachedOutput)
			{
				cachedOutput.Add(new OutputImage(tag));
			}
			return;
		}

		if (!string.IsNullOrEmpty(tag.scene))
		{
			image_url = tag.scene + "/" + image_url;
		}

		if (tag.Text.StartsWith("<Map"))
		{
			addChatBubble(ChatEntry.ChatEntryType.Map, image_url, () => {
				if (!audioIsPlaying) ResumeStory();
			});
		}
		else
		{
			addChatBubble(ChatEntry.ChatEntryType.Image, image_url, () => {
				if (!audioIsPlaying) ResumeStory();
			});
		}

		if (!audioIsPlaying) PauseStory();
	}

	public void ShowArtwork(string artworkId)
	{
		if (string.IsNullOrEmpty(artworkId)) return;

		Log.d("show artwork bubble: " + artworkId + " - " + Thread.CurrentThread.ManagedThreadId);

		NotificationManager.instance.PlayNotification();

		if (!hasFocus)
		{
			Log.d("add to cache");

			lock (cachedOutput)
			{				
				cachedOutput.Add(new OutputArtwork(artworkId));
			}
			return;
		}

		addChatBubble(ChatEntry.ChatEntryType.Artwork, artworkId, () => {
			if (!audioIsPlaying) ResumeStory();
		});

		if (!audioIsPlaying) PauseStory();
	}

	public void ShowLinks(List<StoryLink> links)
	{
		if (links.Count == 0) return;

        Log.d("show links: " + string.Join(", ", links) + " - " + Thread.CurrentThread.ManagedThreadId);

        // play notification
        NotificationManager.instance.PlayNotification();

		if (!hasFocus)
		{
			Log.d("add to cache");

			lock (cachedOutput)
			{
				cachedOutput.Add(new OutputLinks(links));
			}

			return;
		}

		UnityToolbag.Dispatcher.InvokeAsync(() => 
		{
			choiceButtonMover.ClearChoices();

			foreach (var item in links)
			{
				var content = item.Text.Trim();
				if (string.IsNullOrEmpty(content)) continue;

				// Log.d("LINK " + item.Text);

				choiceButtonMover.AddChoice(content, (realContent) =>
				{
					// user interaction - feed the watchdog
					ResetIdleTimer();

					// cleanup audio
					AudioController.instance.ReleaseAudio();

					if (ChoiceButtonMover.IsAR(content))
					{
						StopIdleTimer();
					}

					// cancel all delays and other background threads running
					// Tools.CancelAllThreads();

					// addTextLog(ChatEntry.ChatEntryType.Player, content);
					addChatBubble(ChatEntry.ChatEntryType.Player, realContent, () => 
					{
						// unpause the story controller
						StoryController.instance.Pause(false);

						// do not resume the story here - just:
						// execute the link - force it
						if (activeStoryPlayer != null) activeStoryPlayer.DoLink(item);
					});
				});				
			}
		});
	}

	public void AudioOff()
	{
		audioIsPlaying = false;

		if (talkingItem)
		{
			UnityToolbag.Dispatcher.InvokeAsync(() => 
			{
				talkingItem.SetActive(false);
			});
		}
	}

	public void SetAndrea(bool value)
	{
		// MAYBE WE NEED THAT CODE LATER -- LEAVE IT FOR NOW

		// UnityToolbag.Dispatcher.InvokeAsync(() =>
		// {
		// 	if (!GlobalSettings.instance.doDebugButtons && andreaGlitchOverlay && value)
		// 	{
		// 		andreaGlitchOverlay.SetActive(true);
		// 	}
		// 	else
		// 	{
		// 		andreaGlitchOverlay.SetActive(false);
		// 	}
		// });
	}
	

	public void setBoolVariable(string name, bool value)
	{
		if (currentStoryPlayer != null)
		{
			try {	
				currentStoryPlayer.Story.Vars[name] = value;
			} catch(StoryException e) {
				// var may not exist - suppose that is ok
			}
		}
	}

	public void setStringVariable(string name, string value)
	{
		if (currentStoryPlayer != null)
		{
			try {	
				currentStoryPlayer.Story.Vars[name] = value;
			} catch(StoryException e) {
				// var may not exist - suppose that is ok
			}
		}
	}

	//----------------------------------------
    // implement interface StoryPlayer
    //----------------------------------------

	public bool LoadStory(Story newStory, string name)
	{
		if (!newStory) 
		{
			Log.d("could not add story component");
			return false;
		}

		if (true || _debugLogging) Log.d("loadStory: " + name);

		// INFO:
		// Loading a story may be called from another thread
		// this is important so we can continue if the app 
		// does not have the focus, or the screen is turned off.
		// 
		// Don't invoke the following block of code with:
		// UnityToolbag.Dispatcher.InvokeAsync(...)
		//
		// The new scene should be started on the current thread
		// to allow e.g. waiting for a GPS location or a BLE beacon
		// at the beginning of the new story.
		//
		// The game-state is saved everytime a story loads, if the
		// user decides to quit the app the story would continue 
		// e.g.: waiting for a GPS/BLE-Beacon Location
		//
		// Be aware: Loading audiofiles on a background thread is not possible.

		CleanupStory();

		currentStoryPlayer = new StoryPlayer(newStory, name, this);

		if (name != GlobalSettings.QUESTIONAIR_SCENE_STR
			&& (interruptStoryPlayer == null || !interruptStoryPlayer.IsStory(newStory)))
		{
			lock (chatLog)
			{						
				chatLog.lastScene = name;		
				string json = JsonUtility.ToJson(chatLog, true);
				File.WriteAllText(CHATLOG_FILE, json);
			}
		}

		// make sure to clear interrupt
		// it will: activeStory = currentStory
		// it will: ResetIdleTimer();
		BackToStory();

		// loading - ok
		return true;
	}

	public void BeginStory()
	{
		if (currentStoryPlayer != null)
        {
			currentStoryPlayer.Begin();
        }
	}


	public void PauseCurrentStoryThreads(bool state)
    {
		if (activeStoryPlayer != null) activeStoryPlayer.PauseThreads(state);
	}

	public void PauseStory()
	{
		if (activeStoryPlayer != null) activeStoryPlayer.PauseStory();
	}

	public void ResumeStory()
	{
		if (activeStoryPlayer != null) activeStoryPlayer.ResumeStory();
	}

	public void StopAllStoryThreads()
	{
		if (activeStoryPlayer != null)
		{
			activeStoryPlayer.CancelAllThreads();
		}
	}

	public void ShowHint(float delay, string hint)
	{
		if (activeStoryPlayer != null)
		{
			activeStoryPlayer.RunThreadDelayed(delay, () => {
				ShowBubble(hint, false);
			});
		}
	}

	public void GoToDelayed(float delay, string passagename)
	{
		if (activeStoryPlayer != null)
		{
			activeStoryPlayer.RunThreadDelayed(delay, () => {
				GoTo(passagename);
			});
		}
	}

    public void Reset(bool clearHistory = true)
	{
		Tools.CancelAllThreads();	
		StopIdleTimer();
		
		//
		if (currentStoryPlayer != null)
		{
			currentStoryPlayer.Dispose();
			currentStoryPlayer = null;
		}

		if (interruptStoryPlayer != null)
		{
			interruptStoryPlayer.Dispose();
			interruptStoryPlayer = null;
		}
		activeStoryPlayer = null;
	
		audioIsPlaying = false;
		autoDelayBubbles = true;
		cachedOutput.Clear();

		if (talkingItem) talkingItem.SetActive(false);

		if (clearHistory)
        {
			lock (chatLog)
			{
				chatLog.entries.Clear();
				chatLog.lastScene = null;			
			}
			File.Delete(CHATLOG_FILE);

			ClearChatView();
		}

		ClearChoices();

		if (mapButton)
        {
			mapButton.SetActive(false);
        }
		lastMap = "";
	}

	public Cradle.Story GetStory()
	{
		return currentStoryPlayer.Story;
	}

	public bool GoTo(string name)
	{
		if (activeStoryPlayer != null) return activeStoryPlayer.GoTo(name);

		return false;
    }


	//------------------------------------------------------------------
	// story interrupts
	//------------------------------------------------------------------

	public void BackToStory()
	{
		if (interruptStoryPlayer != null)
		{
			interruptStoryPlayer.Dispose();		
		}

		interruptStoryPlayer = null;
		activeStoryPlayer = currentStoryPlayer;

		if (activeStoryPlayer != null)
		{
			ShowLinks(activeStoryPlayer.GetLinks());
		}

		ResetIdleTimer();
	}

	private void ExecuteInterruptStory(Story story, string name)
	{
		if (story)
		{
			// interrupt audio?
			// we can only stop audio in unity-thread
			// so - we think nothing is playing
			// make sure we do not reach interrupt while playing back audio
			if (audioIsPlaying)
			{
				// jak!
				Log.e("we should stop audio, but we can not realiably do that");
			}

			interruptStoryPlayer = new StoryPlayer(story, name, this);

			if (currentStoryPlayer.State == StoryState.Idle)
			{
				// take over
				activeStoryPlayer = interruptStoryPlayer;

				UnityToolbag.Dispatcher.InvokeAsync(() => 
				{
					choiceButtonMover.ClearChoices();
				});

				interruptStoryPlayer.Begin();
			}
		}
		else
		{
			interruptStoryPlayer.Dispose();
			interruptStoryPlayer = null;
		}
	}

	// gps interrupt

	public void GpsInterrupt()
    {
		lock(this)
		{
			if (_gpsInterruptionStory && interruptStoryPlayer == null)
			{
				_gpsInterruptionStory.Reset(true);

				// this might come from a thread - take care
				ExecuteInterruptStory(_gpsInterruptionStory, "gps");
			}
		}
    }

	// timer interrupt

	private void ResetIdleTimer()
    {
		if (!GlobalSettings.doTimeoutInterrupt) return;

		if (idleTimer == null)
		{
        	idleTimer = new Timer(IdleTimerTimeout, idleTimer, GlobalSettings.WATCHDOG_TIMEOUT, 0);
		}
		else
		{
			// feed the watchdog
			idleTimer.Change(GlobalSettings.WATCHDOG_TIMEOUT, 0);
		}
    }

	private void StopIdleTimer()
	{
		if (idleTimer != null)
		{
			idleTimer.Dispose();
			idleTimer = null;
		}
	}

	private void IdleTimerTimeout(object timer)
    {
		Log.d("idle timer");
		// this comes from a different thread - take care
		lock(this)
		{
			if (_timerInterruptionStory && interruptStoryPlayer == null)
			{
				_timerInterruptionStory.Reset(true);
				ExecuteInterruptStory(_timerInterruptionStory, "timer");
			}
			else
			{
				Log.d("no idle timer");
			}
		}
    }

	// manual interrupt
	public void TriggerManualInterrupt()
	{
		// manual - we can freely interact with story
		lock(this)
		{
			if (_manualInterruptionStory && interruptStoryPlayer == null)
			{
				_manualInterruptionStory.Reset(true);
				ExecuteInterruptStory(_manualInterruptionStory, "manual");
			}
		}
	}


	//------------------------------------------------------------------
	// story interrupts DONE
	//------------------------------------------------------------------

	//------------------------------------------------------------------
	// methods
	//------------------------------------------------------------------	
	private void ClearChatView()
	{
		// clear chat view
		lock(scrollerEntries)
        {
			scrollerEntries.Clear();
			scrollView.SetContent(scrollerEntries);
        }
	}

	public string LastSavedScene()
	{
		string scene = "";
		
		lock (chatLog)
		{
			scene = chatLog.lastScene;
		}

		return scene;
	}

	public void AddPostFocusAction(Action action)
	{
		postFocusActions.Add(action);
	}

	public void CloseARWithCheck()
	{
		if (ARSceneController.instance.Success)
		{
			_AR_success();
		}
		else
		{
			_AR_close();
		}
	}

	public void _AR_success()
    {
		ResetIdleTimer();
		ClearChoices();

        MainPageController.instance.mainCanvasIn(() => 
		{
            GoTo(AR_SUCCESS);
		});
    }

    public void _AR_close()
    {
		ResetIdleTimer();
		ClearChoices();
		
		MainPageController.instance.mainCanvasIn( () =>
        {
			GoTo(AR_CLOSE);			
		});
    }

	private void CleanupStory()
	{
		if (currentStoryPlayer != null)
		{
			currentStoryPlayer.Dispose();
		}

		// stop audio
		AudioController.StopAudio();

		// cleanup resources asap
		//UnityToolbag.Dispatcher.InvokeAsync(() =>
		//{
		//	Resources.UnloadUnusedAssets();
		//});
	}
	



	// TODO... ??? 
	public void ClearChoices()
	{
		if (!hasFocus)
		{
			return;
		}

		if (choiceButtonMover)
		{
			UnityToolbag.Dispatcher.InvokeAsync(() =>
			{
				choiceButtonMover.HideChoices(null);
			});
		}
	}

	

	public double passageTime()
	{
		if (activeStoryPlayer != null) return activeStoryPlayer.PassageTime;
		return 0;
	}

	public void showLastMap()
	{
		if (!string.IsNullOrEmpty(lastMap))
		{
            showFullscreen(lastMap);
        }
	}

	private void addChatBubble(ChatEntry.ChatEntryType type, string text, Action finishAction = null)
	{
		if (audioIsPlaying || !autoDelayBubbles)
		{
			_addChatBubble(type, text, finishAction);
		}
		else
		{
#if UNITY_EDITOR
			_addChatBubble(type, text, finishAction);
#else
			Tools.RunThreadDelayed(text.Length * 0.02F, () =>
			{			
				_addChatBubble(type, text, finishAction);
			});
#endif
		}
	}

	private void _addChatBubble(ChatEntry.ChatEntryType type, string text, Action finishAction = null)
    {
		// add to history
		var entry = new ChatEntry(type, text);
		lock(chatLog)
        {
			chatLog.entries.Add(entry);
        }

		if (!ShowEntry(entry))
		{
			return;
		}

		// show this item

		UnityToolbag.Dispatcher.InvokeAsync(() => 
		{
			scrollView.AddItem(entry, () => 
			{
				if (type == ChatEntry.ChatEntryType.Map)
				{
					lastMap = text;

					if (!string.IsNullOrEmpty(lastMap)
						&& mapButton)
					{
						mapButton.SetActive(true);
					}
				}

				Action act = null;				
				if (finishAction != null)
				{
					act = () =>
					{
						Tools.RunThreadDelayed(0.3f, () =>
						{
							UnityToolbag.Dispatcher.InvokeAsync(() =>
							{
								finishAction();
							});

						});
					};
				}

				if (choiceButtonMover)
				{
					choiceButtonMover.UpdateButtons(act);
				}				
				else if (act != null)
				{
					act();
				}
			});

		});
    }

	public void showFullscreen(string filename)
	{
        if (imageOverlay)
        {
            imageOverlay.Show(filename);
        }
    }

	private ChatEntry.ChatEntryType getChatEntryType()
	{
		if (activeStoryPlayer != null 
			&& activeStoryPlayer.IsAndrea)
		{
			if (audioIsPlaying)
			{
				return ChatEntry.ChatEntryType.AndreaAudio;
			}

			return ChatEntry.ChatEntryType.Andrea;
		}

		if (audioIsPlaying)
		{
			return ChatEntry.ChatEntryType.BotAudio;
		}

		return ChatEntry.ChatEntryType.Bot;		
	}

	public void PauseAndResumeInSeconds(float time, Action preFunc = null, Action postFunc = null)
	{
		if (activeStoryPlayer != null) activeStoryPlayer.PauseAndResumeInSeconds(time, preFunc, postFunc);
	}

}
