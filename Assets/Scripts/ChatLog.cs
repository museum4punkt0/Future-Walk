using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChatLog
{
    public string lastScene;
    public List<ChatEntry> entries = new List<ChatEntry>();
}