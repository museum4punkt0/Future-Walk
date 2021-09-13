using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;

public class PermissionManager
{
    public static event Action<bool> OnLocationPermissionChanged;
    public static event Action<bool> OnNotificationPermissionChanged;
	public static event Action<int> OnBluetoothStatusChanged;
	public static event Action<bool> OnBluetoothStateChanged;

#if UNITY_IOS

    public delegate void PermissionCallback(bool state);

    // location

    [DllImport("__Internal")]
    public static extern void locationPermissionInit(bool background);
    
    [DllImport("__Internal")]
    public static extern bool checkLocationPermission();

    [DllImport("__Internal")]
    public static extern void requestLocationPermission();
    
    [DllImport("__Internal")]
    public static extern void setLocationPermissionCallback(PermissionCallback callback);

    [MonoPInvokeCallback(typeof(PermissionCallback))]
	private static void LocationPermissionChangedCallback(bool state)
	{
		if (OnLocationPermissionChanged != null)
		{
            OnLocationPermissionChanged(state);
            OnLocationPermissionChanged = null;
        }
    }

    // notification

    [DllImport("__Internal")]
    public static extern void checkNotificationPermission();

    [DllImport("__Internal")]
    public static extern void setNotificationStatusCallback(PermissionCallback callback);

    [MonoPInvokeCallback(typeof(PermissionCallback))]
	private static void NotificationStatusCallback(bool state)
	{
		if (OnNotificationPermissionChanged != null)
		{
            OnNotificationPermissionChanged(state);
            OnNotificationPermissionChanged = null;
        }
    }

	[DllImport("__Internal")]
    public static extern void requestNotificationPermission();

    [DllImport("__Internal")]
    public static extern void setNotificationPermissionCallback(PermissionCallback callback);

    [MonoPInvokeCallback(typeof(PermissionCallback))]
	private static void NotificationPermissionCallback(bool state)
	{
		if (OnNotificationPermissionChanged != null)
		{
            OnNotificationPermissionChanged(state);
            OnNotificationPermissionChanged = null;
        }
    }


    // bluetooth

    [DllImport("__Internal")]
    public static extern bool bluetoothState();

    [DllImport("__Internal")]
    public static extern bool checkBluetoothOn();

    [DllImport("__Internal")]
    public static extern bool checkBluetoothPermission();

	public delegate void CBMStateUpdatedCallback(int state);

	[DllImport("__Internal")]
	public static extern void setBluetoothStateUpdatedCallback(CBMStateUpdatedCallback callback);
	
	[MonoPInvokeCallback(typeof(CBMStateUpdatedCallback))]
	private static void CBMStateCallback(int state)
	{
		//Debug.Log("--- CBM state update: " + state);

		if (OnBluetoothStatusChanged != null)
		{
			OnBluetoothStatusChanged(state);
		}
    }


	// camera

	[DllImport("__Internal")]
	public static extern int cameraAuthorizationStatus();

	[DllImport("__Internal")]
	public static extern bool isCameraAuthorizationDetermined();


#elif UNITY_ANDROID

	static AndroidJavaClass obj = default;

    class LocationPermissionChangedCallback : AndroidJavaProxy
    {
        public LocationPermissionChangedCallback() : base("com.neeeu.audiowalk.PermissionRequestCallback") { }

        public void onPermissionChanged(bool state)
        {
			if (OnLocationPermissionChanged != null)
			{
				OnLocationPermissionChanged(state);
			}
		}
	}

	class BluetoothStateChangedCallback : AndroidJavaProxy
	{
		public BluetoothStateChangedCallback() : base("com.neeeu.audiowalk.BluetoothStateCallback") { }

		public void onStateChanged(bool state)
		{
			if (OnBluetoothStateChanged != null)
			{
				OnBluetoothStateChanged(state);
			}
		}
	}

	private static AndroidJavaClass JavaObj()
    {
        if (obj == null)
        {
            obj = new AndroidJavaClass("com.neeeu.audiowalk.MainActivity");
        }

        return obj;
    }
#endif


	//=============================================================================
	// Location
	//=============================================================================

	//------------------------------------------
	//------------------------------------------
	public static bool CheckLocationPermission(bool bg = false)
    {
		//Debug.Log("PermissionManager - CheckLocationPermission");

        if (!Input.location.isEnabledByUser)
		{
			//Debug.Log ("PermissionManager - Location service disabled");
			return false;
		}

#if UNITY_IOS

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            locationPermissionInit(bg);
            return checkLocationPermission();
        }

#elif UNITY_ANDROID

        if (Application.platform == RuntimePlatform.Android)
        {
            bool r = JavaObj().CallStatic<bool>("checkLocationPermission");

            Debug.Log("PermissionManager - CheckLocationPermission - android call: " + r);

            return r;
        }

#endif

        Debug.Log("PermissionManager - CheckLocationPermission - something wrong");
        return false;
    }

	//------------------------------------------
	//------------------------------------------
    public static void RequestLocationPermission(bool bg = false, Action<bool> cb = null)
    {
		Debug.Log("PermissionManager - RequestLocationPermission");

#if UNITY_IOS

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (cb != null)
            {
                OnLocationPermissionChanged = cb;
            	setLocationPermissionCallback(LocationPermissionChangedCallback);
            }

            locationPermissionInit(bg);
            requestLocationPermission();
			return;
        }

#elif UNITY_ANDROID

		if (Application.platform == RuntimePlatform.Android)
        {
			if (cb != null)
			{
				OnLocationPermissionChanged = cb;
				JavaObj().CallStatic("setLocationPermissionCallback", new LocationPermissionChangedCallback());
			}
			
			JavaObj().CallStatic("requestLocationPermission");

			return;
		}

#endif

		if (cb != null)
		{
			cb(true);
		}
    }


    //=============================================================================
	// Notification
	//=============================================================================

	//------------------------------------------
	//------------------------------------------
    public static void CheckNotificationPermission(Action<bool> cb = null)
	{

#if UNITY_IOS

		if (Application.platform == RuntimePlatform.IPhonePlayer) 
		{
			if (cb != null)
			{
                OnNotificationPermissionChanged = cb;
            	setNotificationStatusCallback(NotificationStatusCallback);
            	checkNotificationPermission();
            }
			return;
        }
		
#elif UNITY_ANDROID	

		// is notification permission always true on android?
		if (Application.platform == RuntimePlatform.Android) 
		{
		}        

#endif

		if (cb != null)
		{
			cb(true);
		}
	}

	//------------------------------------------
	//------------------------------------------
	public static void RequestNotificationPermission(Action<bool> cb = null)
	{
		
#if UNITY_IOS

		if (Application.platform == RuntimePlatform.IPhonePlayer) 
		{
			if (cb != null)
			{
                OnNotificationPermissionChanged = cb;
            	setNotificationPermissionCallback(NotificationPermissionCallback);
            	requestNotificationPermission();
            }
			return;
        }
		
#elif UNITY_ANDROID	

		// ?? 
        if (Application.platform == RuntimePlatform.Android) 
		{
		}

#endif

		if (cb != null)
		{
			cb(true);
		}
	
	}


    //=============================================================================
	// Bluetooth
	//=============================================================================

	//------------------------------------------
	/*
	ios: 
		CBManagerStateUnknown = 0,
		CBManagerStateResetting,
		CBManagerStateUnsupported,
		CBManagerStateUnauthorized,
		CBManagerStatePoweredOff,
		CBManagerStatePoweredOn,
	*/
	//------------------------------------------
    public static void GetBluetoothState(Action<int> cb = null)
	{

#if UNITY_IOS

		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (cb != null)
			{
				OnBluetoothStatusChanged = cb;
				setBluetoothStateUpdatedCallback(CBMStateCallback);
				bluetoothState();
			}

			return;
		}

#elif UNITY_ANDROID		

		if (Application.platform == RuntimePlatform.Android) 
		{
			if (cb != null)
			{
				bool has_bl = JavaObj().CallStatic<bool>("hasBluetooth");
				if (!has_bl)
				{
					// CBManagerStateUnsupported
					cb(2);
					return;
				}

				bool ble_on = JavaObj().CallStatic<bool>("checkBluetoothOn");
				if (ble_on)
				{
					// CBManagerStatePoweredOn
					cb(5);
					return;
				}

				// CBManagerStatePoweredOff
				cb(4);
			}

			return;
		}

#endif

		if (cb != null)
		{
			cb(5);
		}

	}

	public static bool CheckBluetoothOn()
	{
#if UNITY_IOS
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return checkBluetoothOn();
		}
#elif UNITY_ANDROID
		
		if (Application.platform == RuntimePlatform.Android)
        {
            bool r = JavaObj().CallStatic<bool>("checkBluetoothOn");

            Debug.Log("PermissionManager - GetCameraAuthorizationStatus - android call: " + r);

            return r;
        }

#endif

		return false;
	}


	public static void TurnBluetoothOn(Action<bool> cb)
	{
#if UNITY_IOS

		// 

#elif UNITY_ANDROID
	
		if (Application.platform == RuntimePlatform.Android)
        {
			OnBluetoothStateChanged = cb;

			bool result = JavaObj().CallStatic<bool>("turnBluetoothOn", new BluetoothStateChangedCallback());
			if (result)
            {
				// just continue
				OnBluetoothStateChanged = null;
				if (cb != null)
				{
					cb(true);
				}
			}

			return;
        }

#endif

		// just continue
		if (cb != null)
		{
			cb(true);
		}

	}


	//=============================================================================
	// Camera
	//=============================================================================
	public static int GetCameraAuthorizationStatus()
	{

#if UNITY_IOS

		/*
		 AVAuthorizationStatusNotDetermined = 0,
		 AVAuthorizationStatusRestricted    = 1,
		 AVAuthorizationStatusDenied        = 2,
		 AVAuthorizationStatusAuthorized    = 3,
		 */

		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return cameraAuthorizationStatus();
		}

#elif UNITY_ANDROID

		if (Application.platform == RuntimePlatform.Android)
        {
            bool r = JavaObj().CallStatic<bool>("cameraAuthorizationStatus");

            Debug.Log("PermissionManager - GetCameraAuthorizationStatus - android call: " + r);

            return r ? 3 : 2;
        }
	
#endif

		// not determined
		return 0;
	}


	public static bool IsCameraAuthorizationDetermined()
	{

#if UNITY_IOS

		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return isCameraAuthorizationDetermined();
		}

#elif UNITY_ANDROID

		// it's always determined on android
		return true;
#endif

		// not determined
		return false;
	}

	
}
