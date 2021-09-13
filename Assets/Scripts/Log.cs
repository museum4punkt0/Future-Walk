using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Log
{
    public static string nowstring()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffffK");
    }

    public static void debug(string text)
    {
#if (UNITY_EDITOR)
        Debug.Log(text);        
#elif UNITY_ANDROID
        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", text ?? "NULL" );
#else
        System.Console.WriteLine(nowstring() + " DEBUG: " + text);
#endif
    }

    public static void d(string text)
    {
        debug(text);
    }

    public static void error(string text)
    {
#if (UNITY_EDITOR)
        Debug.LogError(text);
#elif UNITY_ANDROID
        UnityEngine.Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, "{0}", text ?? "NULL" );    
#else
        System.Console.WriteLine(nowstring() + " ERROR: " + text);
#endif
    }
    
    public static void e(string text)
    {
        error(text);
    }
}
