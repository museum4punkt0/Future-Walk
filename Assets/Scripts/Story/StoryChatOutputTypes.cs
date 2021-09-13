using System;
using System.Collections.Generic;
using Cradle;

abstract class OutputType
{
	public override string ToString()
	{
		return "OutputType";
	}
}

abstract class OutputText : OutputType
{
	public string text;
	public OutputText(string _text)
	{
		text = _text;
	}

	public override string ToString()
	{
		return "OutputText: " + text;
	}
}

class OutputBubble : OutputText
{
	public ChatEntry.ChatEntryType type;
	public OutputBubble(string _text, ChatEntry.ChatEntryType _type) : base(_text)
	{
		type = _type;
	}

	public override string ToString()
	{
		return "OutputBubble(" + type + "): " + text;
	}
}

class OutputAudio : OutputText
{
	public bool notification;
	public OutputAudio(string file, bool noti) : base(file)
	{
		notification = noti;
	}

	public override string ToString()
	{
		return "OutputAudio: " + text + " : noti: " + notification;
	}
}

class OutputImage : OutputType
{
	public HtmlTag tag;
	public OutputImage(HtmlTag _tag)
	{
		tag = _tag;
	}

	public override string ToString()
	{
		return "OutputImage: " + tag;
	}
}

class OutputArtwork : OutputText
{
	public OutputArtwork(string _text) : base(_text)
	{
	}

	public override string ToString()
	{
		return "OutputArtwork: " + text;
	}
}

class OutputLinks : OutputType
{
	public List<StoryLink> links = new List<StoryLink>();
	public OutputLinks(List<StoryLink> _links)
	{
		links.AddRange(_links);
	}

	public override string ToString()
	{
		return "OutputLinks: (" + links.Count + ")" + String.Join(", ", links);
	}
}
