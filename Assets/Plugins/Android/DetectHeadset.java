package com.davikingcode.DetectHeadset;

import android.content.Context;
import android.media.AudioDeviceInfo;
import android.media.AudioManager;
import android.util.Log;

public class DetectHeadset {
	
	static final String TAG = "DetectHeadset";

	static AudioManager myAudioManager;
	
	public DetectHeadset(Context context) {
		myAudioManager = (AudioManager) context.getSystemService(Context.AUDIO_SERVICE);
	}
	
	public boolean _Detect()
	{
		//Added validation for newer api's above 26.
		if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.M)
		{
		    AudioDeviceInfo[] audioDeviceInfos = myAudioManager.getDevices(AudioManager.GET_DEVICES_OUTPUTS);

		    for (int i=0;i<audioDeviceInfos.length; i++)
		    {
				if (audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_BLUETOOTH_SCO ||
						audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_BLUETOOTH_A2DP ||
						audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_WIRED_HEADSET ||
						audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_WIRED_HEADPHONES ||
						audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_LINE_ANALOG ||
						audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_LINE_DIGITAL ||
						audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_USB_HEADSET ||
						audioDeviceInfos[i].getType() == AudioDeviceInfo.TYPE_HEARING_AID)
				{
					return true;
				}
		    }
		}
		else
		{
		    //This should work as expected for the older api's
		    if (myAudioManager.isWiredHeadsetOn() || 
				myAudioManager.isBluetoothA2dpOn() ||
				myAudioManager.isBluetoothScoOn())
			{
				return true;
			}
		}

		return false;
	}
}
