# Project Folder Structure and content

The root-folder of the project contains the scene-files, subfolders for each scene with the same name as the scene-file and one common assets folder.

Assets may be located in a folder managed by the addressable system e.g.: `Resources_moved`

Script examples are notated in Harlowe 2.1.0

Example:

	project-folder/
   		common/                (common asset folder)
       	# common assets
			commonbleep.mp3
        	BasePreset.grain        

    	A1_Scene.html            (the scene script)

    	A1_Scene/                (the scene folder)
        	SynthPreset1.grain
        	SynthPreset2.grain
        	audio.mp3
        	Image.png
        	Video.mp4
    	   	…


## Scene script and story format

We import Twine projects (html files) directly with the Unity-Addon “Cradle” version 2.0.1. [https://github.com/daterre/Cradle/releases](https://github.com/daterre/Cradle/releases)

Storyformat: Harlowe 2.1.0 [https://twine2.neocities.org/2.html](https://twine2.neocities.org/2.html)

With the following restrictions:

* no local variable (_localvariable)

The name of the exported script shall not contain a space and has to start with a letter.
To order the scenes chronologically the following lettering scheme can be used:

* A0_Intro
* A1_Scene1
* A2_Scene2
* B3_anotherScene
* ...
* X9_scene_x9


Additionally to the Harlowe functions the script can use defined game-functions and reference files in its scene-folder or in the common assets folder. To reference a file omit the file-suffix. The function determines which file-type it expects. In the example below the engine expects an audio-file (mp3, wav, aiff, ...) with the name “audio”.

e.g.:<br>
a script called “A1_Scene” may call this function to play audio:
(playAudio: “audio”)

The game-engine tries to load the file in the following order:<br>
If the filename does not start with, but contains a “/” it tries to load the file directly with its name. If the filename does not contain a “/” or if the file can not be found the engine tries to load the file from the scene folder “A1_Scene/audio”. If the file can not be found the engine looks up the file in the common assets folder: “common/audio”. If the file can not be found in the common asset folder the function-call gets discarded.


### Line Break

For any text to show it has to end with a line-break.


### Passage Tags

The following Tags can be used on Passages:

* Bot
	* Makes the bot speak instead of Andrea
* Silent
	* Does not play the “tick” sound for the whole passage


### AR Experience

Any choice starting with “_AR_” is turned into a Button to start the AR Camera (without the  _AR_ prefix)<br>
e.g.: `[[_AR_Start]]` creates a Button labeled: **“Start”**

To handle success or close the story needs to contain 2 Passages with the names:

* “_AR_success”
* “_AR_close”



## Common Assets

Assets in the common asset folder can be referenced in the story-scripts with a leading “/”. This allows for slightly faster lookups and makes it clear where the asset should be found.

e.g.:

	(playAudio: "/commonbleep")



## Scene Folder

For each scene-script one scene-folder with the same filename as the scene-script (without the suffix .html) can exist.

The script references other scenes by their script-name (without the file suffix):

e.g.:

	(loadScene: "A1_Scene1")

The script references assets the asset’s filename:
(showImage: "Image")


The script can set a preset in the synthesizer by calling:

e.g.:

	(synthPreset: “SynthPreset1”, transition_time_in_seconds)

Preset files are generated with the Synth-App made by Mei-Fang Liau.


## Artworks.json

Artwork information is provided in the form of a json file where the id is the image-name (without suffix):

	{"artworks":
	 [
	   {
	     "id": "ice",
	     "title_en": "Icy River",
	     "title_de": "Gefrorener Fluss",
	     "author_en": "A Photographer",
	     "author_de": "Eine Photographin",
	     "year": "2000 - 2001",
	     "content_en": "Long content very short.",
	     "content_de": "Bildbeschreibung Text",
	     "footer_en": "Line 1\nLine 2\nLine 3\nLine 4\nLine 5",
	     "footer_de": "Zeile 1\nZeile 2\nZeile 3\nZeile 4\nZeile 5"
	   },
	   {
	     ...
	   }
	 ]
	}




## Setting GeoFences

This tool can be used to generate geofences: [https://geojson.net/#18/52.50893/13.36699](https://geojson.net/#18/52.50893/13.36699)
