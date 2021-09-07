Additionally to the default functionality of Harlowe-2.1 some variables, tags and macros can be used.

## Story Variables:

#### isenglish

When a story is loaded the “isenglish” variable is set depending your language settings (german or english).
It can be used like this:

	(if: $isenglish)[
	English text
	](else:)[
	Deutscher Text
	]

#### bl_state

The value of this variable matches the iOS CoreBluetootManager States:

* CBManagerStateUnknown = 0
* CBManagerStateResetting = 1
* CBManagerStateUnsupported = 2
* CBManagerStateUnauthorized = 3
* CBManagerStatePoweredOff = 4
* CBManagerStatePoweredOn = 5

When calling the function `(bluetoothState:)` this variable is set (see function documentation below). It can be used like this:

	got result: $bl_state
	
	(if: $bl_state is 5)[
	BL ok - continue
	](else-if: $bl_state is 4)[
	BL powered off
	](else-if: $bl_state is 3)[
	BL unauthorized
	](else:)[
	some other bluetooth state: $bl_state
	]


## Hacky Tags

#### \<a>
Using the a-tag we can play audio. The href=”” attribute references the audio to play back.
Text in between the opening and closing tag is shown as audio-transcription tags.
The opening and closing tags have to not to have trailing or leading text, meaning they should stand alone in one line.

E.g.:

	This shows a mixture of audio and text-messages
	
	<a href="hear">
	Can you hear anything?
	</a>
	
	Hello, Hello?
	
	<a href="hello">
	Hello, Hello, Hello.
	</a>
	
	Any Sound? Do you hear me?
	
	<a href="through">
	Is there any sound coming, am i getting through to you?
	
	Hello?
	</a>

Wrong:
	
	<a href=”audio”>Text
	More text
	Last line</a>

	Text<a href=”audio”>
	More text
	Last line</a>

## Additional HTML-Tags

#### \<Map src="map_room3"/>

src: image to load

The image shows with a map-icon.


#### \<Art src="Vermeer_AR_cut_rgb"/>

src: id for artwork and image-name to load

Shows ArtInformation Bubble as designed - it looks up necessary information in artworks.json (see below).
When clicked the information is set and the ArtworkInformation page is displayed.


## Scripting Macros

####(isenglish:)
	Returns:
	<bool>: whether the app is set to english


####(setMuseum: "name")
	Parameters:
	name: <string>: the name of the museum
	
	Sets the internal variable: currentMusem to the value of the parameter.
	
####(arcDone: "name")
	Parameters:
	name: <string>: the name of the story arc
	
	Adds the story arc name to the list of finished arcs and stores the application settings.
	
####(iaArcDone: "name")
	Parameters:
	name: <string>: the name of the story arc
	Returns:
	<bool>: wether the story arc could be found in the list of finished story arcs
	
####(arcDoneCount:)
	Returns:
	<int>: the count of completed story arcs
	
####(getMuseum:)
	Returns:
	<string>: the current museum set with macro: (setMuseum: "name")

####(getMuseumName:)
	Returns:
	<string>: the translated string for a musem identifier. Please have a look into StoryMacros.cs and GlobalSettings.cs
	
####(increaseMuseumCount:)
	Increases the variable museumCount by one.

####(getMuseumCount:)
	Returns:
	<int>: the current museum count
	
	
	
### Questionair Macros

There are several macros specifically for the Questionair used in the FutureWalk. Please look at the file StoryMacros.cs for more insight.
	
	
### Game control Macros

#### (loadScene: “name”)
    Parameters:
    name: <string> filename of scene-file in double-quotes

    Loads the scene with name “name” and plays it back

####(passageTime:)
    Return:
	<double> time in seconds

	Returns the amount of time passed when entering this Passage

####(pauseFor: time)
####(delay: time)
	Parameters:
   	time: <double> time in seconds

    Pauses the game for “time” seconds


####(openAR: "name")
	Parameters:
	name: <string> AR identifier string
	
	Opens AR with the specified name. Please look into ARSceneController.cs for further details.


####(setFallbackTimer: time, "name")
	Parameters:
	time: <float> delay time in seconds
	name: <string> the passage to goto after the timeout period.
	
	Goto the passage identified by name after a period of seconds specified by time.

####(clearFallbackTimer:)
	Clear the fallback timer


####(setPassageTag: "name")
	Parameters:
	name: <string> the tag to set
	
	adds the tag to the passage


####(showHint: time, "hint")
	Parameters:
	time: <float> delay time in seconds
	hint: <string> the hint to show
	
	adds a text-bubble to the chat-output after a delay


### Audio Macros

#### (playAudio: “name” [, loop])
    Parameters:
    name: <string> filename in double-quotes
	[loop]: <boolean> optional - true or false. Defaults to false

    Plays audio with filename “name” and loops the file if loop is true


####(stopAudio: “name”)
    Parameters:
    name: <string> filename in double-quotes

    Stops playing audio with name “name”

####(playNotification:)
    Plays the notification sound


### Synth Macros

####(synthPreset: “name”[, time, delay])
    Parameters:
    name: <string> filename in double-quotes
    [time]: <float> optional - transition time in seconds. Defaults to 0.0
    [delay]: <float> optional - delay until transition starts. Defaults to 0.0

    Set a synth-preset with name “name” and fade to it in “delay” seconds. Transition is
    “time” seconds long.


### GPS Macros

####(GPSOn:)
    Turns Location tracking on

####(GPSOff:)
    Turns Location tracking off   

####(getDistance: “fence”)
	Parameters:
	fence: <string> id of the fence
	
	Returns:
	<string>: a string containing the distance and a unit “m” or “km”

	Returns the distance to the geo-fence with id “fence”.
	E.g.: “3.1415 km”


####(waitForFence: timerDuration, "scene1", "fence2Scene", "fence3Scene", "paintingSene")
    Parameters:
    timerDuration: <double> time to wait until player gets prompted anyways
    name: <string> or <array of strings> id of the fence or beacon

	Pauses the Story and waits that a geo-fence is entered. If the specific spot is reached the player gets a prompt asking if they reached the goal. If yes is pressed, the story follows to “followUpScene”. If the spot is not reached within “backupTimerDuration” seconds, the players gets a prompt asking if they reached the goal. If yes is pressed, the story follows to “followUpScene”. If “No” is pressed, the story waits for another 10 seconds (TODO: how do we deal with this?).
		If the followUpScene could not be loaded (it may not exist) - the story is simply continued. It can be used as a advanced (delay: xx) function.
		

####(waitForLeavingFence: timerDuration, "scene1", "fence2Scene", "fence3Scene", "paintingSene")
    Parameters:
    timerDuration: <double> time to wait until player gets prompted anyways
    name: <string> or <array of strings> id of the fence or beacon

	Pauses the Story and waits that a geo- fence is left. If the specific spot is left the player gets a prompt asking if they reached the goal. If yes is pressed, the story follows to “followUpScene”. If the spot is not reached within “backupTimerDuration” seconds, the players gets a prompt asking if they reached the goal. If yes is pressed, the story follows to “followUpScene”. If “No” is pressed, the story waits for another 10 seconds (TODO: how do we deal with this?).
		If the followUpScene could not be loaded (it may not exist) - the story is simply continued. It can be used as a advanced (delay: xx) function.

####(checkFence: “name”)
    Parameters:
    name: <string> id of the geo-fence

    Returns:
    <bool>: true or false

    Checks if user is inside a geo-fence.
    
    


### Bluetooth Macros
    

####(waitForBeacon: backupTimerDuration, "scene1", "fence2Scene", "fence3Scene", "paintingSene")
    Parameters:
    timerDuration: <double> time to wait until player gets prompted anyways
    name: <string> or <array of strings> id of the beacon

	Pauses the Story and waits that a beacon is reached. If the specific spot is reached the player gets a prompt asking if they reached the goal. If yes is pressed, the story follows to “followUpScene”. If the spot is not reached within “backupTimerDuration” seconds, the players gets a prompt asking if they reached the goal. If yes is pressed, the story follows to “followUpScene”. If “No” is pressed, the story waits for another 10 seconds (TODO: how do we deal with this?).
		If the followUpScene could not be loaded (it may not exist) - the story is simply continued. It can be used as a advanced (delay: xx) function.

####(checkBeacon: “name”)
    Parameters:
    name: <string> id of the fence or beacon

    Returns:
    <bool>: true or false

    Checks if user is inside a geo-fence or close to a beacon.

####(startBluetooth:)
    Starts services for beacon tracking

####(stopBluetooth:)
    Stops services for beacon tracking

####(turnBluetoothOn: "name")
    Tuns Bluetooth on, asks for permission if needed. On success it goes to the passage identified by name.


### Phone Macros


####(isAndroid:)
	Returns:
	<bool>: if the device runs android
	
####(isIos:)
	Returns:
	<bool>: if the device runs ios
	
	
####(phoneModel:)
    Returns:
    <string>: the phone model
    

####(vibrate: time)
    Parameters:
    time: <double> duration to vibrate.
    This only works on Android!
        Not implemented yet

    Vibraties the phone. On android: Vibration is on for “time” seconds


### Permission Macros

####(permissionsDone:)


####(checkLocationEnabled:)
    Returns:
    <bool>: true if location is enabled, false otherwise

####(checkLocationPermission:)
    Returns:
    <bool>: true if the app has location permission, false otherwise

####(requestLocationPermission: “okPassage”, “failPassage”)
####(checkNotificationPermission: “okPassage”, “failPassage”)
####(requestNotificationPermission: “okPassage”, “failPassage”)
    Parameters:
    okPassage: <string> name of passage to go to if location permissions are granted
    failPassage: <string> name of passage to go to if location permissions are not granted

	Requests or checks for location/notification permissions. This may deactivate the app. We need to wait for it to gain focus after user input and continue with okPassage or failPassage.


####(checkBluetoothOn:)
	Returns:
	<bool>: if bluetooth in on

####(bluetoothState: “gotoPassage”)
    Parameters:
    gotoPassage: <string> name of passage to go to after we know the bluetooth state

	This may deactivate the app and opens a permission-popup on iOS. We need to wait for it to gain focus after user input and continue with okPassage or failPassage.
	This sets the story-variable “bl_state”.
	
	
####(cameraPermission:)
	Returns:
	<bool>: if camera permission is granted or not.
	
	
### Progress Indicator Macros

####(ProgressCurrent: index)
	Parameters:
	index: <int> the index if the current passage.
	
	Sets the current passage index.
	Please look into StoryController.cs for further details.
	
####(ProgressReveal: index)
	Parameters:
	index: <int> the index if the current passage.
	
	Reveals the passage index.
	Please look into StoryController.cs for further details.



### Other Macros

####(enableTalkingItem: state)
	Parameters:
	state: <bool> if the talking-item should be shown or not. This feature may not be enabled. Please check StoryChat.cs for further details.