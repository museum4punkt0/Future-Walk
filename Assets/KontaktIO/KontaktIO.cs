using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using AOT;

public class KontaktIO : MonoBehaviour
{

    static KontaktIO instance = null;
	static GameObject go; 

	public static event Action<string> OnBeaconEnter;
	public static event Action<string, int> OnBeaconUpdate;
	public static event Action<string> OnBeaconExit;

#if UNITY_IOS

    [DllImport("__Internal")]
	private static extern int startDiscoverBeacons();

    [DllImport("__Internal")]
	private static extern int stopDiscoverBeacons();

	[DllImport("__Internal")]
	private static extern int monitorBeacon(string uid);

	[DllImport("__Internal")]
	private static extern int stopMonitorBeacon(string uid);
	
	[DllImport("__Internal")]
	private static extern int stopMonitorAll();

	[DllImport("__Internal")]
	public static extern void setBeaconCallbacks(BeaconEnterCallback enterCb, BeaconUpdateCallback updateCb, BeaconExitCallback exitCb);

	public delegate void BeaconEnterCallback(string uid);
	public delegate void BeaconUpdateCallback(string uid, int rssi);
	public delegate void BeaconExitCallback(string uid);

	[MonoPInvokeCallback(typeof(BeaconEnterCallback))]
	private static void BeaconEnteredCb(string uid)
	{
        if (OnBeaconEnter != null)
        {
			OnBeaconEnter(uid);
		}
	}
	[MonoPInvokeCallback(typeof(BeaconUpdateCallback))]
	private static void BeaconUpdatedCb(string uid, int rssi)
	{
        if (OnBeaconUpdate != null)
        {
			OnBeaconUpdate(uid, rssi);
		}
	}
	[MonoPInvokeCallback(typeof(BeaconUpdateCallback))]
	private static void BeaconExitedCb(string uid)
	{
        if (OnBeaconExit != null)
        {
			OnBeaconExit(uid);
		}
	}


#elif UNITY_ANDROID

	static AndroidJavaClass obj;

	class BeaconCallback : AndroidJavaProxy
    {
        public BeaconCallback() : base("com.neeeu.audiowalk.BeaconUpdated") { }

		public void beaconEnter(string uid)
        {
			if (OnBeaconEnter != null)
			{
				OnBeaconEnter(uid);
			}
		}
        public void beaconUpdate(string uid, int rssi)
        {
			if (OnBeaconUpdate != null)
			{
				OnBeaconUpdate(uid, rssi);
			}
		}

		public void beaconExit(string uid)
        {
			if (OnBeaconExit != null)
			{
				OnBeaconExit(uid);
			}
		}
	}

#endif

    //=============================================================================
	// Init singleton
	//=============================================================================

	static KontaktIO Instance;

	// public static KontaktIO Instance 
	// {
	// 	get {
	// 		if(instance == null)
	// 		{
	// 			go = new GameObject();
	// 			go.name = "KontaktIO";
	// 			instance = go.AddComponent<KontaktIO>();

    //         #if UNITY_ANDROID

	// 			if(Application.platform == RuntimePlatform.Android)
	// 			{
	// 				obj = new AndroidJavaClass("com.neeeu.audiowalk.MyService");
	// 			}

    //         #endif
	// 		}
			
	// 		return instance; 
	// 	}
	// }


#if UNITY_ANDROID
	private static AndroidJavaClass getObj()
	{
		if (obj == null)
		{
			obj = new AndroidJavaClass("com.neeeu.audiowalk.MyService");
		}

		return obj;
	}
#endif

	void Awake() 
	{
		Instance = this;

#if UNITY_ANDROID
		getObj();
#endif
		// if (instance != null && instance != this) 
		// {
		// 	Destroy(this.gameObject);
		// }
	}

    //=============================================================================
	// Discovery
	//=============================================================================
	
	public static void Start(Action<string> enterCb, Action<string, int> updateCb, Action<string> exitCb)
	{
		// Instance.Awake();
		
#if UNITY_IOS

		if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
			OnBeaconEnter = enterCb;
			OnBeaconUpdate = updateCb;
			OnBeaconExit = exitCb;
			setBeaconCallbacks(BeaconEnteredCb, BeaconUpdatedCb, BeaconExitedCb);

            // startDiscoverBeacons();
        }
		
#elif UNITY_ANDROID

		if(Application.platform == RuntimePlatform.Android) 
		{
			OnBeaconEnter = enterCb;
			OnBeaconUpdate = updateCb;
			OnBeaconExit = exitCb;

			getObj().CallStatic("setBeaconCallback", new BeaconCallback());
			getObj().CallStatic("startBeaconScan");
        }
#endif
	}


	public static void SetCallbacks(Action<string> enterCb, Action<string, int> updateCb, Action<string> exitCb)
	{
#if UNITY_IOS

		if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
			OnBeaconEnter = enterCb;
			OnBeaconUpdate = updateCb;
			OnBeaconExit = exitCb;
			setBeaconCallbacks(BeaconEnteredCb, BeaconUpdatedCb, BeaconExitedCb);
        }
		
#elif UNITY_ANDROID

		if(Application.platform == RuntimePlatform.Android) 
		{
			OnBeaconEnter = enterCb;
			OnBeaconUpdate = updateCb;
			OnBeaconExit = exitCb;
			getObj().CallStatic("setBeaconCallback", new BeaconCallback());
        }
#endif
	}


	public static void StartScan()
	{
#if UNITY_IOS

		if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
			// on ios beacon monitoring is on
        }
		
#elif UNITY_ANDROID

		if(Application.platform == RuntimePlatform.Android) 
		{
			getObj().CallStatic("startBeaconScan");
        }
#endif
	}

	

    public static void StopAll()
	{
#if UNITY_IOS

		if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            stopDiscoverBeacons();
			stopMonitorAll();
        }
		
#elif UNITY_ANDROID

		if(Application.platform == RuntimePlatform.Android) 
		{
			UnityToolbag.Dispatcher.InvokeAsync(() =>
			{
				getObj().CallStatic("stopBeaconScan");
			});
        }		
#endif
	}


	public static void Monitor(string uid)
	{
		if (string.IsNullOrEmpty(uid)) return;
		
#if UNITY_IOS

		if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            monitorBeacon(uid);
        }
		
#elif UNITY_ANDROID

		if(Application.platform == RuntimePlatform.Android) 
		{
			// decativated for now as accessing AndroidJavaClass needs to be don on the main thread
			// we are dealing with beacon monitoring in BeaconController
			// alternatively we would need to use our own JNI bindings

			// UnityToolbag.Dispatcher.InvokeAsync(()=>
			// {
			// 	getObj().CallStatic("monitorBeacon", uid);
			// });
        }		
#endif
	}

	public static void StopMonitor(string uid = "")
	{
		
#if UNITY_IOS

		if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
			if (string.IsNullOrEmpty(uid))
			{
				stopMonitorAll();
			}
			else
			{
				stopMonitorBeacon(uid);
			}        
        }
		
#elif UNITY_ANDROID

		if(Application.platform == RuntimePlatform.Android) 
		{
			UnityToolbag.Dispatcher.InvokeAsync(() =>
			{
				if (string.IsNullOrEmpty(uid))
				{
					getObj().CallStatic("stopMonitorAll");
				}
				else
				{
					getObj().CallStatic("stopMonitorBeacon", uid);
				}
			});
        }		
#endif
	}
}
