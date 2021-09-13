using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BottomScrollerView))]
public class BottomScrollerTester : MonoBehaviour
{
    BottomScrollerView scroller;

    private void Awake()
    {
        scroller = GetComponent<BottomScrollerView>();
    }

    public void ResetContent()
    {
        List<ChatEntry> content = new List<ChatEntry>();
        for (int i = 0; i < 100; i++)
        {
            content.Add(CreateRandomEntry(i));
        }
        scroller?.SetContent(content);
    }


    public void AddRandom()
    {
        scroller?.AddItem(CreateRandomEntry(-1));
    }


    private ChatEntry CreateRandomEntry(int count)
    {
        int i = (int)(UnityEngine.Random.value * 3.0F);

        ChatEntry.ChatEntryType type = ChatEntry.ChatEntryType.Andrea;

        if (i % 3 == 0)
        {
            type = ChatEntry.ChatEntryType.Andrea;
            return new ChatEntry(type, "TEST " + count);
        }
        else if (i % 3 == 1)
        {
            type = ChatEntry.ChatEntryType.Map;
            return new ChatEntry(type, "gg03");
        }
        else if (i % 3 == 2)
        {
            type = ChatEntry.ChatEntryType.Image;
            return new ChatEntry(type, "GG_02_SCRIPT_Brueghel_CLUE");
        }

        return default;
    }
}
