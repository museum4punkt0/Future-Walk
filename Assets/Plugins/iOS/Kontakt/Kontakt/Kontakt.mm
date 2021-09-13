//
//  Kontakt.m
//  Kontakt
//
//  Created by inx on 08.06.20.
//  Copyright © 2020 io. All rights reserved.
//

#import <Foundation/Foundation.h>

#import "Kontakt.h"


typedef void(*BeaconUpdateCallback)(const char* uuid, int rssi);
static BeaconUpdateCallback _beaconUpdateCallback = NULL;

typedef void(*BeaconEnterCallback)(const char* uuid);
static BeaconEnterCallback _beaconEnterCallback = NULL;

typedef void(*BeaconExitCallback)(const char* uuid);
static BeaconExitCallback _beaconExitCallback = NULL;



@implementation KDeviceDelegate

- (void)devicesManager:(KTKDevicesManager*)manager didDiscoverDevices:(NSArray <KTKNearbyDevice*>*)devices
{
	for (KTKNearbyDevice *device in devices)
	{
		if (_beaconUpdateCallback != NULL)
		{
			_beaconUpdateCallback([[device uniqueID] UTF8String], [[device RSSI] intValue]);
		}
	}
}

- (void)devicesManagerDidFailToStartDiscovery:(KTKDevicesManager*)manager withError:(NSError*)error
{
	NSLog(@"could not start discovery: %@", error);
}

@end



@implementation KontaktDelegate : NSObject

- (KontaktDelegate *)init
{
	[self requestPermissions];
	
	self.deviceManager = [[KTKDevicesManager alloc] initWithDelegate:self];
	self.beaconManager = [[KTKBeaconManager alloc] initWithDelegate:self];
	
	[self.beaconManager stopRangingBeaconsInAllRegions];
	[self.beaconManager stopMonitoringForAllRegions];
	
	return self;
}

- (void)requestPermissions
{
	switch ([KTKBeaconManager locationAuthorizationStatus])
	{
		case kCLAuthorizationStatusNotDetermined:
			[self.beaconManager requestLocationAlwaysAuthorization];
			break;
		case kCLAuthorizationStatusDenied:
		case kCLAuthorizationStatusRestricted:
			// No access to Location Services
			break;
		case kCLAuthorizationStatusAuthorizedWhenInUse:
			// For most iBeacon-based app this type of
			// permission is not adequate
			[self.beaconManager requestLocationAlwaysAuthorization];
			break;
		case kCLAuthorizationStatusAuthorizedAlways:
			break;
    }
}

// device
- (void)startDeviceDiscovery
{
	[self.deviceManager startDevicesDiscoveryWithInterval: 2.0];
}

- (void)stopDeviceDiscovery
{
	[self.deviceManager stopDevicesDiscovery];
}

// regions
- (void)monitorRegion:(NSString*)identifier
{
	KTKBeaconRegion * region = [[KTKBeaconRegion alloc] initWithProximityUUID:[[NSUUID alloc] initWithUUIDString:identifier] identifier:identifier];
		
	if (region != nil)
	{
		NSLog(@"monitorRegion: %@", identifier);
		
		[self.beaconManager startMonitoringForRegion: region];
		[self.beaconManager requestStateForRegion: region];
	}
	else
	{
		NSLog(@"could not create region for %@", identifier);
	}
}

- (void)stopMonitorRegion:(NSString*)identifier
{
	for (KTKBeaconRegion * region in self.beaconManager.monitoredRegions) {
		if ([region.identifier isEqualToString:identifier])
		{
			[self.beaconManager stopRangingBeaconsInRegion: region];
			[self.beaconManager stopMonitoringForRegion: region];
		}
	}
}

- (void)stopMonitorAll
{
	[self.beaconManager stopRangingBeaconsInAllRegions];
	[self.beaconManager stopMonitoringForAllRegions];
}

//--------------------------------------------
// KTKDevicesManagerDelegate
- (void)devicesManager:(KTKDevicesManager*)manager didDiscoverDevices:(NSArray <KTKNearbyDevice*>*)devices
{
	for (KTKNearbyDevice *device in devices)
	{
		if (_beaconUpdateCallback != NULL)
		{
			_beaconUpdateCallback(device.uniqueID.UTF8String, device.RSSI.intValue);
		}
	}
}

- (void)devicesManagerDidFailToStartDiscovery:(KTKDevicesManager*)manager withError:(NSError*)error
{
	NSLog(@"could not start discovery: %@", error);
}

//--------------------------------------------
// KTKBeaconManagerDelegate
- (void)beaconManager:(KTKBeaconManager *)manager didChangeLocationAuthorizationStatus:(CLAuthorizationStatus)status
{
	if (kCLAuthorizationStatusAuthorizedAlways != status)
	{
		//
	}
	
//	NSLog(@"didChangeLocationAuthorizationStatus: %d", status);
}

- (void)beaconManager:(KTKBeaconManager *)manager didStartMonitoringForRegion:(__kindof KTKBeaconRegion *)region
{
	//
}

- (void)beaconManager:(KTKBeaconManager *)manager monitoringDidFailForRegion:(__kindof KTKBeaconRegion *_Nullable)region withError:(NSError *_Nullable)error
{
	NSLog(@"monitoringDidFailForRegion: %@ - %@", [region identifier], error);
}

- (void)beaconManager:(KTKBeaconManager *)manager didDetermineState:(CLRegionState)state forRegion:(__kindof KTKBeaconRegion *)region
{
	switch (state) {
		case CLRegionStateUnknown:
			break;
			
		case CLRegionStateInside:
			if (_beaconEnterCallback != NULL)
			{
				_beaconEnterCallback(region.proximityUUID.UUIDString.UTF8String);
			}
			[manager startRangingBeaconsInRegion:region];
			break;
			
		case CLRegionStateOutside:
			if (_beaconExitCallback != NULL)
			{
				_beaconExitCallback(region.proximityUUID.UUIDString.UTF8String);
			}
			[manager stopRangingBeaconsInRegion:region];
			break;
			
		default:
			break;
	}
}

//

- (void)beaconManager:(KTKBeaconManager *)manager didRangeBeacons:(NSArray *)beacons inRegion:(__kindof KTKBeaconRegion *)region
{
	if (_beaconUpdateCallback != NULL)
	{
		for (CLBeacon *b in beacons)
		{
			/*
			 CLProximityUnknown,
			 CLProximityImmediate,
			 CLProximityNear, = 2
			 CLProximityFar
			 */
			if (b.rssi != 0
				&& b.proximity != CLProximityUnknown)
			{
//				NSLog(@"%@ - %f - %ld - %ld", b.proximityUUID.UUIDString, b.accuracy, (long)b.rssi, b.proximity);
				_beaconUpdateCallback(b.proximityUUID.UUIDString.UTF8String, (int)b.rssi);
			}
		}
	}
}

- (void)beaconManager:(KTKBeaconManager *)manager rangingBeaconsDidFailForRegion:(__kindof KTKBeaconRegion *_Nullable)region withError:(NSError *_Nullable)error
{
	NSLog(@"rangingBeaconsDidFailForRegion: %@", error);
}

@end


static KontaktDelegate* kontaktDelegate = NULL;

extern "C"
{
	void initKontaktDelegate()
	{
		if (kontaktDelegate == NULL)
		{
			kontaktDelegate = [[KontaktDelegate alloc] init];
		}
	}

	void startDiscoverBeacons()
	{
		initKontaktDelegate();
		
		if (kontaktDelegate != NULL)
		{
			[kontaktDelegate startDeviceDiscovery];
		}
	}

	void stopDiscoverBeacons()
	{
		initKontaktDelegate();
		
		if (kontaktDelegate != NULL)
		{
			[kontaktDelegate stopDeviceDiscovery];
		}
	}

	void setBeaconCallbacks(BeaconEnterCallback enterCb,
							BeaconUpdateCallback updateCb,
							BeaconExitCallback exitCb)
	{
		_beaconEnterCallback = enterCb;
		_beaconUpdateCallback = updateCb;
		_beaconExitCallback = exitCb;
		
		initKontaktDelegate();
	}

	void monitorBeacon(const char* uid)
	{
		initKontaktDelegate();
		
		if (kontaktDelegate != NULL)
		{
			NSString* uuidstring = [[NSString alloc] initWithUTF8String:uid];
			NSUUID* uuid = [[NSUUID alloc] initWithUUIDString: uuidstring];
			if (uuid != nil)
			{
				[kontaktDelegate monitorRegion: uuidstring];
			}
		}
	}
	void stopMonitorBeacon(const char* uid)
	{
		initKontaktDelegate();
		
		if (kontaktDelegate != NULL)
		{
			NSString* uuidstring = [[NSString alloc] initWithUTF8String:uid];
			NSUUID* uuid = [[NSUUID alloc] initWithUUIDString: uuidstring];
			if (uuid != nil)
			{
				[kontaktDelegate stopMonitorRegion: uuidstring];
			}
		}
	}
	void stopMonitorAll()
	{
		initKontaktDelegate();
		
		if (kontaktDelegate != NULL)
		{
			[kontaktDelegate stopMonitorAll];
		}
	}
}
