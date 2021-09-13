using System;
using System.Collections.Generic;
using Cradle;

public interface IStoryPlayer
{
    void Begin();
    void DoLink(StoryLink link);
    void PauseStory();
    void ResumeStory();

    void AudioStarted(float length);
    bool IsStory(Story story);
    void Dispose();
    bool GoTo(string name);
    List<StoryLink> GetLinks();    

    bool IsAndrea{ get; set; }
    StoryState State{ get; }
    double PassageTime{ get;}
}