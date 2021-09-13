//
//  LocationPermission.mm
//  Permissions
//
//  Created by Ingo Randolf on 11.11.20.
//

#import "Permissions.h"
#import "LocationPermission.h"

@implementation LocationPermission

// permission callback
static PermissionCb _locationPermissionCallback = NULL;

- (LocationPermission*)init:(bool)background
{
	self._background = background;
	
	self.locationManager = [[CLLocationManager alloc] init];
    self.locationManager.delegate = self;
	
	return self;
}

- (bool)checkPermission
{
	if (self._background)
	{
		return [CLLocationManager locationServicesEnabled] && [CLLocationManager authorizationStatus] == kCLAuthorizationStatusAuthorizedAlways;
	}
	
	return [CLLocationManager locationServicesEnabled] && [CLLocationManager authorizationStatus] == kCLAuthorizationStatusAuthorizedWhenInUse;
}

- (void)requestPermission
{
	if ([CLLocationManager authorizationStatus] == kCLAuthorizationStatusDenied ||
		[CLLocationManager authorizationStatus] == kCLAuthorizationStatusRestricted)
	{
		// User has explicitly denied authorization for this application, or
		// location services are disabled in Settings.
		
		// or it is retricted and can not be changes... in both cases say no to the callback
		
		if (_locationPermissionCallback)
		{
			_locationPermissionCallback(false);
		}
		else
		{
			// can not do much about it...
		}
		
		return;
	}
	
	if (self._background)
	{
		if ([CLLocationManager authorizationStatus] != kCLAuthorizationStatusAuthorizedAlways)
		{
			if (@available(iOS 8, *))
			{
				[self.locationManager requestAlwaysAuthorization];
			}
			else
			{
				//?
				if (_locationPermissionCallback)
				{
					_locationPermissionCallback(false);
				}
				else
				{
					NSLog(@"no callback!");
				}
			}
		}
		else
		{
			if (_locationPermissionCallback)
			{
				_locationPermissionCallback(true);
			}
			else
			{
				NSLog(@"already always - no CB");
			}
		}
	}
	else
	{
		if ([CLLocationManager authorizationStatus] != kCLAuthorizationStatusAuthorizedWhenInUse)
		{
			if (@available(iOS 8, *))
			{
				[self.locationManager requestWhenInUseAuthorization];
			}
			else
			{
				//?
				if (_locationPermissionCallback)
				{
					_locationPermissionCallback(false);
				}
			}
		}
		else
		{
			if (_locationPermissionCallback)
			{
				_locationPermissionCallback(true);
			}
		}
	}
}

// CLLocationManagerDelegate
- (void)locationManager:(CLLocationManager *)manager didChangeAuthorizationStatus:(CLAuthorizationStatus)status
{
	if(_locationPermissionCallback != NULL)
	{
		if (self._background)
		{
			_locationPermissionCallback(status == kCLAuthorizationStatusAuthorizedAlways
										|| status == kCLAuthorizationStatusAuthorizedWhenInUse);
		}
		else
		{
			_locationPermissionCallback(status == kCLAuthorizationStatusAuthorizedWhenInUse);
		}
	}
}

@end



//----------------------------------------------------
//----------------------------------------------------
// c interface
//----------------------------------------------------
//----------------------------------------------------

static LocationPermission* locationPermissionDelegate = NULL;

extern "C"
{
	void locationPermissionInit(bool background)
	{
		if(locationPermissionDelegate == NULL)
		{
			locationPermissionDelegate = [[LocationPermission alloc] init: background];
		}
	}

	bool checkLocationPermission()
	{
		if (locationPermissionDelegate != NULL)
		{
			return [locationPermissionDelegate checkPermission];
		}
		
		return false;
	}

	void requestLocationPermission()
	{
		if (locationPermissionDelegate != NULL)
		{
			[locationPermissionDelegate requestPermission];
		}
	}

	void setLocationPermissionCallback(PermissionCb callback)
	{
		_locationPermissionCallback = callback;
	}
}
