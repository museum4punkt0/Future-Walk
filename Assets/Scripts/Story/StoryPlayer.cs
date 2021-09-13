using System.Collections.Generic;
using System;
using System.Threading;
using Cradle;
using Cradle.StoryFormats.Harlowe;

public class StoryPlayer : ThreadedDelayRunner, IStoryPlayer
{
    //------------------------------------------------------------
    // static
    //------------------------------------------------------------
    private static bool _debugLogging = false;


    //------------------------------------------------------------
    // class fields
    //------------------------------------------------------------
    long PassageEntered = 0;
    StoryPassage currentPassage;
    bool commentOpen = false;
    bool aTagOpen = false;
    string bubbleText = "";
    string audioToPlay = "";
    int audioFallbackDuration = 0;
    StoryOutput lastOutput;

    readonly List<StoryOutput> audioContent = new List<StoryOutput>();
    readonly List<StoryLink> links = new List<StoryLink>();

    bool _isAndrea = false;

    private readonly Story _story;
    public Story Story
    {
        get { return _story; }
    }

    public StoryState State
    {
        get { return _story ? _story.State : StoryState.Idle; }
    }

    public double PassageTime
    {
        get { return ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - PassageEntered) / 1000.0; }
    }

    private readonly IStoryPlayerListener listener;

    //------------------------------------------------------------
    // constructor
    //------------------------------------------------------------
    public StoryPlayer(Story story, string name, IStoryPlayerListener _listener) : base(name)
    {
        this._story = story;
        listener = _listener;

        if (this._story)
        {
            this._story.OnPassageEnter += Story_OnPassageEnter;
            this._story.OnOutput += Story_OnOutput;
            this._story.OnStateChanged += Story_OnStateChange;
            this._story.OnPassageDone += Story_OnPassageDone;
        }
    }

    //------------------------------------
    // implement IStoryPlayer
    //------------------------------------
    public bool IsAndrea
    {
        get
        {
            return _isAndrea;
        }
        set
        {
            if (aTagOpen)
            {
                audioContent.Add(new TagSwitch(value));
            }
            else
            {
                _isAndrea = value;
                if (listener != null) listener.SetAndrea(value);
            }
        }
    }

    public void Begin()
    {
        if (_story)
        {
            _story.Reset(true);

            try {
				_story.Vars["isenglish"] = GlobalSettings.instance.IsEnglish;
			} catch(StoryException e) {
                // NOTE:
                // it just means $isenglish is never used in the story
                // this is fine
				// nop
			}

            _story.Begin();
        }
    }
    public void DoLink(StoryLink link)
    {
        CancelAllThreads();
        _story.DoLink(link, true);
    }

    public void PauseStory()
    {
        if (_story)
        {
            try
            {
                _story.Pause();
            }
            catch (InvalidOperationException e)
            {
                // Log.d(e.Message);
            }
        }
    }

    public void ResumeStory()
    {
        if (_story)
        {
            try
            {
                _story.Resume();
            }
            catch (InvalidOperationException e)
            {
                Log.d(e.Message);
            }
        }
    }

    public void AudioStarted(float length)
    {
        PostAudioStart(length);
    }

    public bool IsStory(Story story)
    {
        return this._story == story;
    }

    public void Dispose()
    {
        CancelAllThreads();

        _story.OnPassageEnter -= Story_OnPassageEnter;
        _story.OnOutput -= Story_OnOutput;
        _story.OnStateChanged -= Story_OnStateChange;
        _story.OnPassageDone -= Story_OnPassageDone;

        _story.Reset(true);
    }

    public bool GoTo(string name)
    {
        try {
			_story.GetPassage(name);
            ResumeStory();
            _story.GoTo(name);
            return true;
        }
		catch (StoryException e)
		{
            // not found!!
            // nop - be silent
            Log.d("could not goto: " + e);
        }
		catch (InvalidOperationException e)
		{
			//
		}

        return false;
    }

    public List<StoryLink> GetLinks()
    {
        return links;
    }

    //------------------------------------------------------------
    // story functions
    //------------------------------------------------------------
    private void Story_OnStateChange(StoryState state)
	{
        // Log.d("state changed: " + state);

        if (state == StoryState.Idle
            && listener != null
            && links.Count > 0)
        {            
            listener.ShowLinks(links);
        }
        else
        {
            links.Clear();
        }
	}

    private void Story_OnPassageEnter(StoryPassage passage)
	{
		// if (_debugLogging) Log.d("Story_OnPassageEnter: " + passage.Name + " : " + Thread.CurrentThread.ManagedThreadId);

		currentPassage = passage;
		PassageEntered = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		
		bubbleText = "";	
		audioToPlay = "";
		audioContent.Clear();
		lastOutput = null;
        links.Clear();

		IsAndrea = !Array.Exists<string> (currentPassage.Tags, element => element == "Bot");

		// INFO:
		// Do not clear choices here!
		// If the current story was loaded from a background thread it would
		// remove the choices which are added before any other text
	}

    private void Story_OnPassageDone(StoryPassage passage)
    {
        // needed?
    }

    private void Story_OnOutput(StoryOutput output)
	{
        DisplayOutput(output);
    }


    //------------------------------------------------------------
    // call listener to show output
    //------------------------------------------------------------
    private void DisplayOutput(StoryOutput output, bool resume = true)
	{
        // Log.d("** DisplayOutput: *" + output + "* ---" + Thread.CurrentThread.ManagedThreadId);

        // check for comments
        // just in case...
        if (!string.IsNullOrEmpty(output.Text))
        {
            if (output.Text.Trim().StartsWith("<!--"))
            {					
                commentOpen = !(output.Text.Trim().EndsWith("-->"));
                return;
            }
            else if (commentOpen && output.Text.Trim().EndsWith("-->"))
            {
                commentOpen = false;
                return;
            }
        }

        // reject comments
        if (commentOpen)
        {
            return;
        }

        // ignore double linebreak
        if (output is LineBreak 
            && lastOutput is LineBreak)
        {
            return;
        }
        lastOutput = output;

        // add to audio content
        // ignore </a> and <a
        if (aTagOpen
            && (
                !(output is HtmlTag)
                || (
                    !output.Text.StartsWith("</a>") 
                    && !output.Text.StartsWith("<a")
                )
            ))
        {
            // Log.d("add to audio: " + output.Text);
            audioContent.Add(output);
            return;
        }

        //
        // check output type
        //

        if (output is StoryText)
        {
            var tt = (output as StoryText);

            if (!string.IsNullOrEmpty(tt.Text))
            {
                string content = tt.Text.Trim() + (tt.Text.EndsWith(" ") ? " " : "");
                if (content != " ")
                {
                    bubbleText += content;
                }
            }
        }
        else if (output is LineBreak)
        {
            if (!String.IsNullOrWhiteSpace(bubbleText))
            {
                // Log.d("DisplayOutput TEXT: *" + bubbleText + "*");

                var text = bubbleText.Trim();

                if (listener != null) listener.ShowBubble(text, resume);
                else Log.e("no listener!");

                bubbleText = "";
            }
        }
        else if (output is HtmlTag)
        {
            var tag = output as HtmlTag;

            // Log.d("HTML ------ " + tag.Text);

            // just ignore these
            if (tag.Text.StartsWith("</Map>")
                || tag.Text.StartsWith("</Art>"))
            {
                return;
            }

            if (tag.Text.StartsWith("<img")
                || tag.Text.StartsWith("<Map"))
            {                
                if (listener != null) listener.ShowImage(tag);
            }
            else if (tag.Text.StartsWith("<_audiooff"))
            {
                AudioOff();
            }
            else if (tag.Text.StartsWith("<a"))
            {
                aTagOpen = true;
                audioContent.Clear();

                // get audio to play
                audioToPlay = Tools.GetHtmlAttributeContent(tag.Text, "href");                

                // get fallback duration
                audioFallbackDuration = 0;
                int.TryParse(Tools.GetHtmlAttributeContent(tag.Text, "duration"), out audioFallbackDuration);
            }
            else if (tag.Text.StartsWith("</a>"))
            {
                aTagOpen = false;

                if (_debugLogging) Log.d("closing a-tag - audioToPlay: " + audioToPlay + " : " + Thread.CurrentThread.ManagedThreadId + " audio content: " + audioContent.Count);

                // remove all leading <br>
                while (audioContent[0].GetType() == typeof(LineBreak))
                {
                    audioContent.RemoveAt(0);
                }

                if (!string.IsNullOrEmpty(audioToPlay))
                {
                    float initialDelay = 0;
                    if (audioContent.Count > 0 && (audioContent[0] is AudioDelay))
                    {
                        initialDelay = (audioContent[0] as AudioDelay).delayTime;
                    }

                    if (listener != null) listener.PlayAudio(audioToPlay, initialDelay);
                }
                else
                {
                    // nothing to play... check if we have some content to show
                    if (audioContent.Count > 0)
                    {
                        // what to do??
                        Log.e("this was not planned - :/");
                    }
                }
            }
            else if (tag.Text.StartsWith("<b>"))
            {
                bubbleText += "<b>";
            }
            else if (tag.Text.StartsWith("</b>"))
            {
                bubbleText += "</b>";
            }
            else if (tag.Text.StartsWith("<i>"))
            {
                bubbleText += "<i>";
            }
            else if (tag.Text.StartsWith("</i>"))
            {
                bubbleText += "</i>";
            }
            else if (tag.Text.StartsWith("<Art"))
            {
                // <Art src="ice"/>
                var image_src = Tools.GetHtmlAttributeContent(tag.Text, "src");

                if (!string.IsNullOrEmpty(image_src)
                    && ArtworkInfoController.instance.Contains(image_src))
                {
                    if (!string.IsNullOrEmpty(tag.scene))
                    {
                        image_src = tag.scene + "/" + image_src;
                    }

                    if (listener != null) listener.ShowArtwork(image_src);
                }
            }
        }
        else if (output is StoryLink)
        {
            links.Add(output as StoryLink);
        }
        else if (output is OutputGroup)
        {
            // ignore

            // Add an empty indicator to later positioning
            // var groupMarker = new GameObject();
            // groupMarker.name = output.ToString();
            // AddToUI(groupMarker.AddComponent<RectTransform>(), output, uiInsertIndex);
        }
        else if (output is HarloweLive) 
        {
            if (aTagOpen)
            {
                // ignore harlove-live in audio...?
                // TODO: figure out a way to do this
                // audioContent.Add(output);
                return;
            }

            PauseAndResumeAbsolute(((HarloweLive)output).seconds);
        }
        else if (output is AudioDelay)
        {
            // ignore
        }
        else if (output is TagSwitch) 
        {
            // ignore
        }
	}

    private void AudioOff()
	{
		if (listener != null) listener.AudioOff();

		ResumeStory();
	}

    private void PauseAndResumeAbsolute(float time)
	{
		var timeToWait = (long)(time*1000F) - ((long)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - PassageEntered);
		
		if (timeToWait > 0)
		{
			PauseAndResumeInSeconds(timeToWait / 1000F);
		}
	}

	public void PauseAndResumeInSeconds(float time, Action preFunc = null, Action postFunc = null)
	{
		if (aTagOpen)
		{
			// AudioDelay is handled only when playing back audio bubbles with focus
			audioContent.Add(new AudioDelay(time));
			return;
		}

		PauseStory();

		RunThreadDelayed(time, () => 
		{
			if (preFunc != null)
			{
				preFunc();
			}

			ResumeStory();

			if (postFunc != null)
			{
				postFunc();
			}
		});
	}

    // audio content handling
    private void PostAudioStart(float audioLength)
	{
		// Log.d("audioLength: " + audioLength);

		// TODO: improve timing
		// TODO: output delay while audio is running
		var timeForOneChar = 0F;
        var initialTime = 0F;

		// remove initial linebreaks
		while (audioContent[0].GetType() == typeof(LineBreak))
		{
            audioContent.RemoveAt(0);
        }

		if (audioLength <= 0)
		{
			// if no length is given set one second per entry or 1 second in total
			audioLength = audioContent.Count > 0 ? audioContent.Count : 1;
		}

        if (audioContent.Count > 0)
		{
            var numChars = string.Join("", audioContent.FindAll(i => i is StoryText)).Length;

			// each htmlTag -> one second delay
			var numHtml = audioContent.FindAll(i => i is HtmlTag).Count;
			// not sure about this... figure it out later

			float length_for_chars = audioLength - numHtml;

			var audiodelays = audioContent.FindAll(i => i is AudioDelay);
			foreach(var delay in audiodelays)
			{
				length_for_chars -= (delay as AudioDelay).delayTime;
			}


            // count characters
            timeForOneChar = (length_for_chars / numChars) * 1.01f;

			if (audioContent.Count > 0 && audioContent[0] is StoryText)
			{
            	initialTime = audioContent[0].Text.Length * timeForOneChar;
			} 
			else if (audioContent.Count > 0 && audioContent[0] is AudioDelay)
			{
				initialTime = (audioContent[0] as AudioDelay).delayTime;
			}
        }

        // Log.d("audio content: " + string.Join("", audioContent));
        RunThreadDelayed(initialTime, () => 
		{
			ShowAudioContent(audioContent, timeForOneChar, audioLength, initialTime);
		});
	}

	private void ShowAudioContent(List<StoryOutput> list, float characterTime, float length, float sum)
	{
		lock(this)
		{
			if (list.Count > 0)
			{
                // get first element
				var first = list[0];
				list.RemoveAt(0);

				// show output
				DisplayOutput(first, false); // (list.Count > 1)

				if (list.Count > 0)
				{
					// get next
					first = list[0];
					
					if (first is AudioDelay)
					{
						var time = (first as AudioDelay).delayTime;						

                        // TODO:
                        // not sure about this!
						// foreach (var item in list)
						// {
						// 	if (item is StoryText)
						// 	{
						// 		time += item.Text.Length * characterTime;
						// 		break;
						// 	}
						// }

                        // Log.d("showFirstAudioContent in: " + time);
                        
						RunThreadDelayed(time, () => 
						{
							ShowAudioContent(list, characterTime, length, (sum+time));
						});
					}
					else if (first is TagSwitch)
					{
						IsAndrea = ((first as TagSwitch).isAndrea);
						ShowAudioContent(list, characterTime, length, sum);
					}
					else if (first is StoryText)
					{
						var time = first.Text.Length * characterTime;

                        // Log.d("showFirstAudioContent in: " + time);
						RunThreadDelayed(time, () => 
						{
							ShowAudioContent(list, characterTime, length, (sum+time));
						});
					}
					else if (first is HtmlTag)
					{
                        float time = 1;

                        // Log.d("showFirstAudioContent in: " + time);
						RunThreadDelayed(time, () => 
						{
							ShowAudioContent(list, characterTime, length, (sum+time));
						});
					}
					else
					{
						// linebreak
						// and everything else
						// just show
						ShowAudioContent(list, characterTime, length, sum);
					}
				}
				else
				{					
					ShowAudioContent(list, characterTime, length, sum);
				}
			}
			else
			{
				var lastTime = length - sum;
                if (lastTime < 0.2f) lastTime = 0.2f;

				RunThreadDelayed(lastTime, () => 
				{
                    // can't we just call audiooff() ?
					DisplayOutput(new HtmlTag("<_audiooff/>"));
				});
			}
		}
	}

}