using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class DetectHeadset
{
	#if UNITY_IOS && !UNITY_EDITOR
		
		[DllImport ("__Internal")]
		static private extern bool _Detect();

	#elif UNITY_ANDROID

		static AndroidJavaClass obj;

	#endif
		
	static public bool CanDetect()
	{
		#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
			return true;
		#endif
			
		return false;
	}


	static public bool Detect()
	{
#if UNITY_IOS && !UNITY_EDITOR

			return _Detect();

#elif UNITY_ANDROID && !UNITY_EDITOR

			if (obj == null)
			{
				obj = new AndroidJavaClass("com.neeeu.audiowalk.MainActivity");
			}

			if (obj != null)
			{
				return obj.CallStatic<bool>("_Detect");
			}
			else
			{
				Debug.Log("no obj!!");
			}

			return false;

			// using (var javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			// {
			// 	Debug.Log("DETECT");
			// 	using (var currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
			// 	{
			// 		Debug.Log("DETECT - CREATE CLASS");

			// 		using (var androidPlugin = new AndroidJavaObject("com.davikingcode.DetectHeadset.DetectHeadset", currentActivity))
			// 		{
			// 			Debug.Log("DETECT - CALL");

			// 			return androidPlugin.Call<bool>("_Detect");
			// 		}
			// 	}
			// }

#else

		return true;

#endif
	}
}
