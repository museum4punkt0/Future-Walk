using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

public class Tools
{
    public static bool _debugLogging = false;
    private static readonly string PROJECT_STR = "project/";
	private static readonly string COMMON_STR = "common";
    private static readonly string AR_STR = "ar";

    private static bool paused = false;
    private static bool assetBundleCached = false;

    private static bool defaultAudioClipLoaded = false;
    private static AssetBundle assetBundle;
    private static AudioClip defaultAudioClip;

    public static IEnumerable<Type> FindSubClassesOf<TBaseType>()
    {   
        var baseType = typeof(TBaseType);
        var assembly = baseType.Assembly;

        foreach (var item in assembly.GetTypes())
        {        
            if (!item.IsAbstract && item.IsSubclassOf(baseType))
            {
                yield return item;
            }            
        }

        yield break;
    }

    public static string removeFolderPref(string name)
    {
        var i = name.LastIndexOf("/");

        if(i>0)
        {
            name = name.Substring(i+1);
        }
        return name;
    }
    public static string removeSuffix(string name)
	{	
		var idx = name.LastIndexOf(".");

		if (idx > 0)
		{
			name = name.Substring(0, idx);
		}

		return name;
	}

    static ThreadedDelayRunner delayRunner = new ThreadedDelayRunner("ToolsDelayRunner");

    public static int ThreadCount()
    {
        return delayRunner.ThreadCount;
    }

    public static void CancelAllThreads()
    {
        delayRunner.CancelAllThreads();
    }

    public static async void RunThreadDelayed(float time, Action func)
    {
        delayRunner.RunThreadDelayed(time, func);
    }

    public static void Pause(bool state)
    {
        delayRunner.PauseThreads(state);
    }

    public static void LoadDefaulAudioClip()
    {
        defaultAudioClip = AudioClip.Create("Empty audioclip",1,1,1000,false);
        defaultAudioClipLoaded = true;
    }

    public static void LoadAssetBundle()
    {
        AssetBundleCache abc = GameObject.FindObjectOfType<AssetBundleCache>();
        assetBundle = abc.assetBundle;
        assetBundleCached = true;
    }

    public static AudioClip LoadAudioClip(string requestedclipname)
    {
        // load from resources
        return Load<AudioClip>(requestedclipname);

        // return defaultAudioClip;
        // if(!assetBundleCached)
        // {
        //     LoadAssetBundle();
        // }

        // if(!defaultAudioClipLoaded)
        // {
        //     LoadDefaulAudioClip();
        // }
        
        // string[] clipnames = assetBundle.GetAllAssetNames();

        // var assetRequest = assetBundle.LoadAssetAsync(requestedclipname);

        // if(assetRequest != null)
        // {
        //     return (AudioClip)assetRequest.asset;
        //     //return clip = (AudioClip)assetRequest.asset;
        // }
        // else 
        // {
        //     Log.d("requested audioclip not found in Asset Bundle");
        //     return defaultAudioClip;
        // }

        // foreach(string clipname in clipnames)
        // {
        //     string _clipname = clipname;
        //     Log.d("Clip names in asset bundle: " + clipname);

        //     _clipname = removeFolderPref(_clipname);
        //     _clipname = removeSuffix(_clipname);

        //     Log.d("Clip names in asset bundle AFTER TRIMMING: " + clipname);

        //     if(_clipname == requestedclipname)
        //     {
        //         //var assetRequest = abc.assetBundle.LoadAssetAsync(requestedclipname);
        //         clip = (AudioClip)assetRequest.asset;
        //         //return clip;
        //     }
        //     else
        //     {
        //         Log.d("requested audioclip not found in Asset Bundle");
        //     }  
        // }
    }

    private static T _DoLoad<T>(string address) where T : UnityEngine.Object
    {
        // try resources
        return Resources.Load<T>(address);
    }

    public static T Load<T>(string filename) where T : UnityEngine.Object
    {
        if (_debugLogging) Log.d("Load: " + filename);    

        T component = null;

        if (filename.StartsWith("/"))
        {
            // load from common
            component = _DoLoad<T>(PROJECT_STR + COMMON_STR + filename);
        }
        else if (filename.Contains("/"))
        {
            // try to load directly
            component = _DoLoad<T>(PROJECT_STR + filename);
        }

        if (!component && StoryController.instance)
        {
            // try to load from scene-folder
            component = _DoLoad<T>(PROJECT_STR + StoryController.instance.CurrentScene + "/" + filename);
        }


        if (!component && !filename.StartsWith("/"))
        {
            // look into common
            component = _DoLoad<T>(PROJECT_STR + COMMON_STR + "/" + filename);
        }

        if (!component && !filename.StartsWith("/"))
        {
            // look into ar
            component = _DoLoad<T>(PROJECT_STR + AR_STR + "/" + filename);
        }

        // check special folders
        if (!component)
        {
            component = _DoLoad<T>(PROJECT_STR + "intro/" + filename);
        }
        if (!component)
        {
            component = _DoLoad<T>(PROJECT_STR + "out/" + filename);
        }
        if (!component)
        {
            component = _DoLoad<T>(PROJECT_STR + "kgm/" + filename);
        }
        if (!component)
        {
            component = _DoLoad<T>(PROJECT_STR + "gg/" + filename);
        }
        if (!component)
        {
            component = _DoLoad<T>(PROJECT_STR + "mim/" + filename);
        }
        if (!component)
        {
            component = _DoLoad<T>(PROJECT_STR + "music/" + filename);
        }
        if (!component)
        {
            component = _DoLoad<T>(PROJECT_STR + "Questionair/" + filename);
        }
        

        if (!component && StoryController.instance)
        {
            // try all subfolders
            foreach (var key in StoryController.instance.StoryNames)
            {
                component = _DoLoad<T>(PROJECT_STR + key + "/" + filename);
                if (component) {
                    return component;
                }
            }

            Debug.LogError("could not load: " + filename);				
        }

        return component;
    }

    private static Array typeItems = Enum.GetValues(typeof(ChatEntry.ChatEntryType));  
    public static ChatEntry randomItem(int i)
    {
        ChatEntry.ChatEntryType type = (ChatEntry.ChatEntryType)typeItems.GetValue((int)(UnityEngine.Random.value * typeItems.Length));
        
        string text = "ice";
        if (type != ChatEntry.ChatEntryType.Image)
        {
            text = $"ZCell {i} and a little bit more text here";

            int c = (int)(UnityEngine.Random.value * 4F);
            int ii=0;
            for (ii=0; ii<c; ii++)
            {
                text += "\n\tline " + ii;
            }
        }

        return new ChatEntry(type, text);
    }

    public static Color RGBAColor(int r, int g, int b, int a)
    {
        return new Color((float)r / 255F, (float)g / 255F, (float)b / 255F, (float)a / 255F);
    }

    public static Color RGBColor(int r, int g, int b)
    {
        return RGBAColor(r, g, b, 255);
    }

    public static Color RGBColor(string hex)
    {
        int rgb = Convert.ToInt32(hex, 16);

        return RGBColor((rgb & 0xff0000) >> 16,
                        (rgb & 0xff00) >> 8,
                        (rgb & 0xff));
    }

    /// get content of html attribute between double-quote
    // e.g.: <img src="image.png"/> - returns "image.png"
    public static string GetHtmlAttributeContent(string text, string attr)
    {
        while (text.Contains(" ="))
        {
            text = text.Replace(" =", "=");
        }
        while (text.Contains("= "))
        {
            text = text.Replace("= ", "=");
        }
        text = text.Replace("'", "\"");

        string[] arr = text.Split(new string[] {attr+"=\""}, StringSplitOptions.RemoveEmptyEntries);
		if (arr.Length == 2)
		{
			arr = arr[1].Split('"');
			return arr[0];
		}

        return "";
    }

    public static string randomString(int length)
    {
        string result = "";

        for (int i = 0; i < length; i++)
        {
            result += (char)UnityEngine.Random.Range(20, 127);
        }

        return result;
    }

    public static void printRectIterate(GameObject obj, string indent = "")
    {
        Log.d(indent + obj.name + " - rect: " + obj.GetComponent<RectTransform>().rect);
        foreach (Transform child in obj.transform)
        {
            printRectIterate(child.gameObject, indent + "  ");
        }
    }
}
