using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NotificationManager : MonoBehaviour
{
    // static
    static public NotificationManager instance;

    // class fields
    Dictionary<string, AudioNotificationPlayer> audioNotificationPlayers = new Dictionary<string, AudioNotificationPlayer>();

    bool hasFocus = true;

    
    void Awake()
    {
        if (instance)
        {
            Log.e("NotificationManager singleton already set!");
        }
        else
        {
            instance = this;
        }

        // register all AudioNotificationPlayer
        AudioNotificationPlayer[] players = GetComponents<AudioNotificationPlayer>();
        foreach (var player in players)
        {
            audioNotificationPlayers[player.GetName()] = player;
        }
    }

    void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        hasFocus = !pauseStatus;
    }

    public float PlayNotification(string name = "ping")
    {
        if (audioNotificationPlayers.ContainsKey(name))
        {
            float length = audioNotificationPlayers[name].PlayNotification();

            if (name == "ping" && !hasFocus)
            {
                // NOTE: do not use Handheld.Vibrate()
                // it would only with app-focus

                // NOTE: this will not work on android
                // JNI calls do not work if app is in background
                NativeToolkit.Vibrate();
            }

            return length;
        }

        Log.d("notification not found: " + name);

        return 0;
    }

    public void StopAll()
    {
        foreach (var entry in audioNotificationPlayers)
        {
            entry.Value.Stop();
        }
    }

    public bool HasNotification(string name)
    {
        return audioNotificationPlayers.ContainsKey(name);
    }

}
