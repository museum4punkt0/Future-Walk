using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChatEntry
{
    public enum ChatEntryType
    {
        Bot, BotAudio, Andrea, AndreaAudio, Player, Image, Map, Artwork
    }

    public ChatEntryType type;

    // content: used for text-content and image names
    public string content;

    public float height = 0;

    public ChatEntry(ChatEntryType type, string content = "")
    {
        this.type = type;
        this.content = content;
    }
}
