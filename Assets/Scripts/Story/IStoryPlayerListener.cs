using System;
using Cradle;
using System.Collections.Generic;

public interface IStoryPlayerListener
{
    void ShowBubble(string text, bool resume = false);
    void PlayAudio(string file, float initialDelay);
    void ShowImage(HtmlTag tag);
    void ShowArtwork(string artworkId);
    void ShowLinks(List<StoryLink> links);
    void AudioOff();
    void SetAndrea(bool value);
}
