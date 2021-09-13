//
//  CameraPermission.m
//  UnityFramework
//
//  Created by inx on 20.04.21.
//

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>

//----------------------------------------------------
//----------------------------------------------------
// c interface
//----------------------------------------------------
//----------------------------------------------------

extern "C"
{
	int cameraAuthorizationStatus()
	{
		/*
		 AVAuthorizationStatusNotDetermined = 0,
		 AVAuthorizationStatusRestricted    = 1,
		 AVAuthorizationStatusDenied        = 2,
		 AVAuthorizationStatusAuthorized    = 3,
		 */
		return (int)[AVCaptureDevice authorizationStatusForMediaType: AVMediaTypeVideo];
	}
	
	bool isCameraAuthorizationDetermined()
	{
		return [AVCaptureDevice authorizationStatusForMediaType: AVMediaTypeVideo] != AVAuthorizationStatusNotDetermined;
	}
}
