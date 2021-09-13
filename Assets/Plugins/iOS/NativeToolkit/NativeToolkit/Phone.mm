//
//  Phone.mm
//  NativeToolkit
//
//  Created by Ingo Randolf on 08.06.20.
//

#import <AudioToolbox/AudioToolbox.h>
#import <AVFoundation/AVFoundation.h>

extern "C"
{
	void vibratePhone()
	{
		AudioServicesPlayAlertSound(UInt32(kSystemSoundID_Vibrate));
	}

	void setAVAudioSessionPlayback()
	{
		[[AVAudioSession sharedInstance]
							setCategory: AVAudioSessionCategoryPlayback
								  error: nil];
	}
}
