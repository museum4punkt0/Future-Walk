#import <AVFoundation/AVFoundation.h>

extern "C" {

	bool _Detect() {
		
        AVAudioSessionRouteDescription* route = [[AVAudioSession sharedInstance] currentRoute];
        
        for (AVAudioSessionPortDescription* desc in [route outputs])
		{
			NSLog(@"route: %@", desc.portName);
		}

        for (AVAudioSessionPortDescription* desc in [route outputs]) {
            if ([[desc portType] isEqualToString:AVAudioSessionPortHeadphones])
                return true;
            if ([[desc portType] isEqualToString:AVAudioSessionPortBluetoothHFP])
                return true;
	    //It was displaying this route for bluetooth wireless headphones.
            //Tested with Unity 2019.1
            if ([[desc portType] isEqualToString:AVAudioSessionPortBluetoothA2DP])
                return true;

            if ([[desc portType] isEqualToString:AVAudioSessionPortLineOut])
				return true;
			if ([[desc portType] isEqualToString:AVAudioSessionPortBluetoothLE])
				return true;
			if ([[desc portType] isEqualToString:AVAudioSessionPortCarAudio])
				return true;
			if ([[desc portType] isEqualToString:AVAudioSessionPortUSBAudio])
				return true;
			if ([[desc portType] isEqualToString:AVAudioSessionPortAirPlay])
				return true;
        }
        
        return false;
        
	}
	
}
