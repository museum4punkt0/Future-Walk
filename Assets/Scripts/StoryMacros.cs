using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cradle;
using System;
using System.Text.RegularExpressions;


public class StoryMacros : Cradle.RuntimeMacros
{
	// NOTE:
	// Be aware that any macro might run on a different thread than the
	// Unity main thread.
	// Unity API is not thread-safe, calls on different threads fail
	// silently.
	// For macros calling into Unity make sure to wrap it in:
	// UnityToolbag.Dispatcher.InvokeAsync()
	// or:
	// UnityToolbag.Dispatcher.Invoke()


	[Cradle.RuntimeMacro]
	public bool isenglish()
	{
		if (GlobalSettings.instance) return GlobalSettings.instance.IsEnglish;

		// default to english
		return true;
	}

	//----------------------------------------
	// global story variables
	//----------------------------------------
	private static string currentMuseum = ""; 	// this is used in interrupt scenes
	private static int museumCount = 0; 		// this is used for inbetween scenes

	[Cradle.RuntimeMacro]
	public void setMuseum(string museum)
	{
		currentMuseum = museum;

		if (GlobalSettings.DO_GPS_INTERRUPT)
		{
			if (currentMuseum == "mim" || currentMuseum == "kgm" || currentMuseum == "gg")
			{
				// TODO: implement
				//startInterruptionFence(currentMuseum);
			}
			else
			{
				// TODO: implement
				//stopInterruptionFence();
			}
		}
	}

	[Cradle.RuntimeMacro]
	public void arcDone(string museum)
	{
		if (GlobalSettings.instance) GlobalSettings.instance.arcDone(museum);
	}

	[Cradle.RuntimeMacro]
	public bool isArcDone(string museum)
	{
		if (GlobalSettings.instance) return GlobalSettings.instance.isArcDone(museum);
		return false;
	}

	[Cradle.RuntimeMacro]
	public int arcDoneCount()
	{
		if (GlobalSettings.instance) return GlobalSettings.instance.arcDoneCount();
		return 0;
	}

	[Cradle.RuntimeMacro]
	public string getMuseum()
	{
		return currentMuseum;
	}

	[Cradle.RuntimeMacro]
	public string getMuseumName()
	{
		if (string.IsNullOrEmpty(currentMuseum)) return "";

		if (currentMuseum.ToLower() == "mim")
		{
			if (GlobalSettings.instance && GlobalSettings.instance.IsEnglish)
			{
				return GlobalSettings.NAME_MIM_EN;
			}
			return GlobalSettings.NAME_MIM_DE;
		}

		if (currentMuseum.ToLower() == "kgm")
		{
			if (GlobalSettings.instance && GlobalSettings.instance.IsEnglish)
			{
				return GlobalSettings.NAME_KGM_EN;
			}
			return GlobalSettings.NAME_KGM_DE;
		}

		if (currentMuseum.ToLower() == "gg")
		{
			if (GlobalSettings.instance && GlobalSettings.instance.IsEnglish)
			{
				return GlobalSettings.NAME_GG_EN;
			}
			return GlobalSettings.NAME_GG_DE;
		}

		if (currentMuseum.ToLower() == "out")
		{
			if (GlobalSettings.instance && GlobalSettings.instance.IsEnglish)
			{
				return GlobalSettings.NAME_OUT_EN;
			}
			return GlobalSettings.NAME_OUT_DE;
		}

		// something else
		return currentMuseum;
	}

	[Cradle.RuntimeMacro]
	public void increaseMuseumCount()
	{
		museumCount++;
	}

	[Cradle.RuntimeMacro]
	public int getMuseumCount()
	{
		return museumCount;
	}

	//----------------------------------------
	// questionaire
	//----------------------------------------
	private static int questionairCount = 0;
	private static string questionairFollowScene = "";

	[Cradle.RuntimeMacro]
	public void loadQuestionaire(int count, string followUpScene)
	{
		questionairCount = count;
		questionairFollowScene = followUpScene;

		if (StoryController.instance)
		{
			StoryController.instance.LoadScene(GlobalSettings.QUESTIONAIR_SCENE_STR);
		}
	}

	[Cradle.RuntimeMacro]
	public void cancelQuestionaire()
	{
		if (!string.IsNullOrWhiteSpace(questionairFollowScene))
		{
			loadScene(questionairFollowScene);
			questionairFollowScene = "";
		}
        else
        {
			loadScene(GlobalSettings.MUSEUM_CHOOSER);
        }
	}

	[Cradle.RuntimeMacro]
	public int qcount()
	{
		questionairCount--;

		if (questionairCount <= 0)
		{
			string scene = questionairFollowScene;
			if (string.IsNullOrWhiteSpace(scene))
			{
				scene = GlobalSettings.MUSEUM_CHOOSER;
			}

			Tools.RunThreadDelayed(0.5f, () =>
			{
				UnityToolbag.Dispatcher.InvokeAsync(() =>
				{
					loadScene(scene);
					questionairFollowScene = "";
				});
			});
		}

		return questionairCount;
	}

	// andrea questions
	[Cradle.RuntimeMacro]
	public int andreaQuestion()
	{
		if (GlobalSettings.instance) return GlobalSettings.instance.andreaQuestion;
		return 0;
	}

	[Cradle.RuntimeMacro]
	public void setAndreaQuestion(int value)
	{
		if (GlobalSettings.instance) GlobalSettings.instance.andreaQuestion = value;
	}

	[Cradle.RuntimeMacro]
	public int plantsQuestion()
	{
		if (GlobalSettings.instance) return GlobalSettings.instance.plantsQuestion;
		return 0;
	}

	[Cradle.RuntimeMacro]
	public void setPlantsQuestion(int value)
	{
		if (GlobalSettings.instance) GlobalSettings.instance.plantsQuestion = value;
	}

	[Cradle.RuntimeMacro]
	public int calamityQuestion()
	{
		if (GlobalSettings.instance) return GlobalSettings.instance.calamityQuestion;
		return 0;
	}

	[Cradle.RuntimeMacro]
	public void setCalamityQuestion(int value)
	{
		if (GlobalSettings.instance) GlobalSettings.instance.calamityQuestion = value;
	}

	[Cradle.RuntimeMacro]
	public int antiphoneQuestion()
	{
		if (GlobalSettings.instance) return GlobalSettings.instance.antiphoneQuestion;
		return 0;
	}

	[Cradle.RuntimeMacro]
	public void setAntiphoneQuestion(int value)
	{
		if (GlobalSettings.instance) GlobalSettings.instance.antiphoneQuestion = value;
	}

	//----------------------------------------
	// game control
	//----------------------------------------

	[Cradle.RuntimeMacro]
	public void loadScene(string name)
	{
		if (StoryController.instance) StoryController.instance.LoadScene(name);
	}

	[Cradle.RuntimeMacro]
	public double passageTime()
	{
		if (StoryChat.instance) return StoryChat.instance.passageTime();
		return 0D;
	}

	[Cradle.RuntimeMacro]
	public void pauseFor(double time)
	{		
		if (StoryChat.instance) StoryChat.instance.PauseAndResumeInSeconds((float)time);
	}

	[Cradle.RuntimeMacro]
	public void delay(double time)
	{		
		if (StoryChat.instance) StoryChat.instance.PauseAndResumeInSeconds((float)time);
	}

	[Cradle.RuntimeMacro]
	public void openAR(string name)
	{
		if (!string.IsNullOrEmpty(name) && ARSceneController.instance)
        {
			var type = ARSceneController.ARType.None;

			switch (name.ToLower())
			{
				case ARSceneController.AR_SCENE_GEORGE:
					type = ARSceneController.ARType.ARImageGeorg;
					break;
				case ARSceneController.AR_SCENE_BOUQUET:
					type = ARSceneController.ARType.ARImageBouquet;
					break;
				case ARSceneController.AR_SCENE_VERMEER:
					type = ARSceneController.ARType.ARImageVermeer;
					break;

				case "wurlitzer":
					type = ARSceneController.ARType.ARWurlitzer;
					break;
				case "trautonium":
					type = ARSceneController.ARType.ARTrautonium;
					break;
				case "cembalo":
					type = ARSceneController.ARType.ARCembalo;
					break;

				case "camera":
					type = ARSceneController.ARType.FakeCamera;
					break;			
			}

			// make sure to run this on the main thread
			// blocking call
			UnityToolbag.Dispatcher.Invoke(() =>
			{
				ARSceneController.instance.ShowType(type);
			});
        }
	}

	[Cradle.RuntimeMacro]
	public void setFallbackTimer(float time, string passageName)
	{
		// TODO: implement
		// can we set a timed notification on ios and android?
		// if so we should do that... in case the app closes while the timer is running
#if UNITY_EDITOR
		time = time/10.0F;
		if (time < 1) time = 1;
#endif

		if (StoryChat.instance) StoryChat.instance.GoToDelayed(time, passageName);
	}

	[Cradle.RuntimeMacro]
	public void clearFallbackTimer()
	{
		if (StoryChat.instance) StoryChat.instance.StopAllStoryThreads();
	}

	[Cradle.RuntimeMacro]
	public void setPassageTag(string tag)
	{
		if (StoryChat.instance) StoryChat.instance.IsAndrea = !tag.Contains("Bot");
	}

	[Cradle.RuntimeMacro]
	public void showHint(float delay, string hint)
	{
#if UNITY_EDITOR
		delay = delay/10.0F;
		if (delay < 1) delay = 1;
#endif

		if (StoryChat.instance) StoryChat.instance.ShowHint(delay, hint);
	}


	//----------------------------------------
	// audio
	//----------------------------------------

	// just play an audio file
    [Cradle.RuntimeMacro]
	public void playAudio(string filename, bool loop = false)
	{
		// load audio from resources
		UnityToolbag.Dispatcher.Invoke(() =>
		{
			AudioController.PlayAudio(filename, loop);
		});
	}

	[Cradle.RuntimeMacro]
	public void stopAudio()
	{
		AudioController.StopAudio();
	}

	[Cradle.RuntimeMacro]
	public void playNotification(string name = "ping")
	{
		if (NotificationManager.instance
			&& !string.IsNullOrEmpty(name))
		{
			NotificationManager.instance.PlayNotification(name);
		}
	}

	//----------------------------------------
	// Synth
	//----------------------------------------

	[Cradle.RuntimeMacro]
	public void synthPreset(string name, float time = 0, float delay = 0)
	{
		if (AudioController.instance)
		{
			UnityToolbag.Dispatcher.InvokeAsync(() => 
			{
				AudioController.instance.LoadSynthSettings(name, time, delay);
			});
		}
	}

	//----------------------------------------
	// GPS
	//----------------------------------------

	[Cradle.RuntimeMacro]
	public void GPSOn()
	{
		if (GpsController.instance) GpsController.instance.StartLocationService();
	}

	[Cradle.RuntimeMacro]
	public void GPSOff()
	{
		if (GpsController.instance) GpsController.instance.StopLocationService();
	}

	[Cradle.RuntimeMacro]
	public string getDistance(string id)
	{
		if (!GpsController.instance) return "?";

		var dist = GpsController.instance.distanceTo(id);
		if (dist < 0)
		{
			return "?";
		}

		return dist < 1000.0 ? (dist.ToString("F2") + " m") : ((dist / 1000.0).ToString("F2") + " km");
	}

	[Cradle.RuntimeMacro]
	public void waitForFence(float backupTimerDuration, params string[] ids)
	{
		if (GpsController.instance) GpsController.instance.StartLocationService();
		if (StoryController.instance) StoryController.instance.waitForFence(backupTimerDuration, false, ids);
	}

	[Cradle.RuntimeMacro]
	public void waitForLeavingFence(float backupTimerDuration, params string[] ids)
	{	
		if (GpsController.instance) GpsController.instance.StartLocationService();
		if (StoryController.instance) StoryController.instance.waitForFence(backupTimerDuration, true, ids);
	}

	[Cradle.RuntimeMacro]
	public bool checkFence(string id)
	{
#if UNITY_EDITOR
		// always true in editor
		return true;
#endif

		if (GeoJsonController.instance) return GeoJsonController.instance.hasEntered(id);
		return false;
	}
	
	[Cradle.RuntimeMacro]
	public void startNoiseFence(string fenceId, string audio)
	{
		if (StoryController.instance) StoryController.instance.startGpsNoise(fenceId, audio);
	}

	[Cradle.RuntimeMacro]
	public void playNoiseAudio(string audio)
	{
		if (StoryController.instance) StoryController.instance.PlayNoiseAudio(audio);
	}

	[Cradle.RuntimeMacro]
	public void stopNoiseFence()
	{
		if (StoryController.instance) StoryController.instance.stopGpsNoise();
	}

	[Cradle.RuntimeMacro]
	public void backToStory()
	{
		if (StoryChat.instance) StoryChat.instance.BackToStory();
	}

	

	//----------------------------------------
	// BLE
	//----------------------------------------

	[Cradle.RuntimeMacro]
	public void waitForBeacon(float backupTimerDuration, params string[] ids)
	{
		if (BeaconController.instance) BeaconController.instance.StartScan();
		if (StoryController.instance) StoryController.instance.waitForBeacon(backupTimerDuration, ids);
	}

	[Cradle.RuntimeMacro]
	public bool checkBeacon(string id)
	{
		if (BeaconController.instance) return BeaconController.instance.HasEntered(id);
		return false;
	}

	[Cradle.RuntimeMacro]
	public void startBluetooth()
	{
		if (BeaconController.instance) BeaconController.instance.StartScan();
	}

	[Cradle.RuntimeMacro]
	public void stopBluetooth()
	{
		if (BeaconController.instance) BeaconController.instance.StopAll();
	}

	[Cradle.RuntimeMacro]
	public void turnBluetoothOn(string passageName)
	{
		if (StoryController.instance)
		{
			StoryController.instance.PauseStory();

			PermissionManager.TurnBluetoothOn((bool state) =>
			{
				Log.d("Bluetooth turned on: " + state);

				if (state)
				{
					// continue to passage
					StoryController.instance.StoryGoTo(passageName);
				}
                else
                {
					// TODO
                }
			});
		}
	}


	//----------------------------------------
	// Phone
	//----------------------------------------

	[Cradle.RuntimeMacro]
	public bool isAndroid()
	{
#if UNITY_ANDROID
		return Application.platform == RuntimePlatform.Android;
#endif
		return false;
	}

	[Cradle.RuntimeMacro]
	public bool isIos()
	{
#if UNITY_IOS
		return Application.platform == RuntimePlatform.IPhonePlayer;
#endif
		return false;
	}

	[Cradle.RuntimeMacro]
	public string phoneModel()
	{
		return GlobalSettings.SystemInfoDeviceModel;
	}

	[Cradle.RuntimeMacro]
	public void vibrate(double time)
	{
		// NOTE:
		// vibration time only possible on android
		// rather use haptics on ios and android

		// NOTE: this call may fail on a different thread than the main thread
		// this may be ok:
		// calling this later when app has focus and mainthread is exectuted again,
		// may vibrate the device without obvious reason.
		Handheld.Vibrate();
	}

	//----------------------------------------
	// permissions
	//----------------------------------------
	[Cradle.RuntimeMacro]
	public void permissionsDone()
	{
		if (GlobalSettings.instance) GlobalSettings.instance.permissionsDone = true;
	}

	[Cradle.RuntimeMacro]
	public bool checkLocationEnabled()
	{
#if UNITY_EDITOR
		return true;
#endif

		bool result = false;

		// invoke on main thread and block - wait for the result
		UnityToolbag.Dispatcher.Invoke(() =>
		{
        	result = Input.location.isEnabledByUser;        
		});		

        return result;
    }

	[Cradle.RuntimeMacro]
	public bool checkLocationPermission()
	{
		bool result = false;

		// invoke on main thread and block - wait for the result
		UnityToolbag.Dispatcher.Invoke(() =>
		{
        	result = PermissionManager.CheckLocationPermission(true);
		});

        return result;
    }

	[Cradle.RuntimeMacro]
	public void requestLocationPermission(string okPassage, string failPassage)
	{
		if (StoryController.instance)
        {
			UnityToolbag.Dispatcher.InvokeAsync(() =>
			{
				StoryController.instance.PauseStory();

				PermissionManager.RequestLocationPermission(true, (bool state) =>
				{
					Action action = () =>
					{
						if (state)
						{
							StoryController.instance.StoryGoTo(okPassage);
						}
						else
						{
							StoryController.instance.StoryGoTo(failPassage);
						}
					};

					if (StoryController.instance.Focus)
					{
						action();
					}
					else
					{					
						// we should only continue the story after the app re-gained the focus
						StoryController.instance.AddPostFocusAction(action);
					}
				});			
			});
        }
    }



	[Cradle.RuntimeMacro]
	public void checkNotificationPermission(string okPassage, string failPassage)
	{
		if (StoryController.instance)
        {
			StoryController.instance.PauseStory();

			PermissionManager.CheckNotificationPermission((bool state) =>
			{			
				if (state)
				{
					StoryController.instance.StoryGoTo(okPassage);
				}
				else
				{
					StoryController.instance.StoryGoTo(failPassage);
				}
			
			});
        }
    }

	[Cradle.RuntimeMacro]
	public void requestNotificationPermission(string okPassage, string failPassage)
	{
		if (StoryController.instance)
		{
			StoryController.instance.PauseStory();

			PermissionManager.RequestNotificationPermission((bool state) =>
			{
				Action action = () =>
				{
					if (state)
					{
						StoryController.instance.StoryGoTo(okPassage);
					}
					else
					{
						StoryController.instance.StoryGoTo(failPassage);
					}
				};

				if (StoryController.instance.Focus)
				{
					action();
				}
				else
				{
				// we should only continue the story after the app re-gained the focus
				StoryController.instance.AddPostFocusAction(action);
				}
			});
		}
    }


	[Cradle.RuntimeMacro]
	public bool checkBluetoothOn()
	{
#if UNITY_EDITOR
		return true;
#endif

		if (GlobalSettings.instance.permissionsDone)
        {
			// this may pop up a dialog on ios
			return PermissionManager.CheckBluetoothOn();
        }

		return false;
	}

	[Cradle.RuntimeMacro]
	public void bluetoothState(string gotoPassage)
	{
		/*
		ios: 
		CBManagerStateUnknown = 0,
		CBManagerStateResetting,
		CBManagerStateUnsupported,
		CBManagerStateUnauthorized,
		CBManagerStatePoweredOff,
		CBManagerStatePoweredOn,
		*/

		if (StoryController.instance)
        {
			UnityToolbag.Dispatcher.InvokeAsync(() =>
			{
				StoryController.instance.PauseStory();

				PermissionManager.GetBluetoothState((int status) =>
				{
					Log.d("----!!! ---- " + status);

					Story story = StoryController.instance.GetStory();
					if (story)
					{
						try {
							story.Vars["bl_state"] = status;
						} catch(StoryException e) {
							// nop
						}					
					}

					Action action = () =>
					{
						// delay this a bit
						Tools.RunThreadDelayed(0.3f, () =>
						{
							UnityToolbag.Dispatcher.InvokeAsync(() =>
							{
								StoryController.instance.StoryGoTo(gotoPassage);
							});
						});
					};

#if UNITY_IOS
				
					// we should only continue the story after the app re-gained the focus
					// StoryController.instance.AddPostFocusAction(() =>
					// {
					// 	UnityToolbag.Dispatcher.InvokeAsync(() =>
					// 	{					
					// 	});
					// });

					if (status == 5)
					{
						// everything ok - no pop up
						action();
					}
					else if (status == 4)
					{
						// this will be a popup the first time
						// if we check this again, just call the action
						if (GlobalSettings.instance.permissionsDone)
						{
							action();
						}
						else
						{
							StoryController.instance.AddPostFocusAction(action);
						}
					}
					else if (status == 3)
					{
						// no auth - no popup
						action();
					}
					else if (status == 0)
					{
						// unknown - popup
						// StoryController.instance.AddPostFocusAction(action);
					}
					else {
						// CBManagerStateResetting,
						// CBManagerStateUnsupported
						action();
					}

#else

					action();

#endif
			
				});
			});
        }
	}


	[Cradle.RuntimeMacro]
	public bool cameraPermission()
	{
#if UNITY_EDITOR
		return false;
#endif

		if (!PermissionManager.IsCameraAuthorizationDetermined())
		{
			return true;
		}

		bool result = false;

		// invoke on main thread and block - wait for the result
		UnityToolbag.Dispatcher.Invoke(() =>
		{
			result = Application.HasUserAuthorization(UserAuthorization.WebCam);
		});

		return result;
	}



	//----------------------------------------
	// progress indicator
	//----------------------------------------
	[Cradle.RuntimeMacro]
	public void ProgressCurrent(int passageIndex)
	{
		if (StoryController.instance)
		{
			StoryController.instance.ProgressCurrent(passageIndex);
			//Log.d("MACRO progress current: " + passageIndex);
		}
	}

	[Cradle.RuntimeMacro]
	public void ProgressReveal(int passageIndex)
	{
		if (StoryController.instance)
		{
			StoryController.instance.ProgressReveal(passageIndex);
			//Log.d("MACRO progress REVEAL: " + passageIndex);
		}
	}



	//----------------------------------------
	// utilities
	//----------------------------------------

	[Cradle.RuntimeMacro]
	public void enableTalkingItem(bool value)
    {
		if (StoryChat.instance) StoryChat.instance.EnableTalkingItem(value);
	}


}
