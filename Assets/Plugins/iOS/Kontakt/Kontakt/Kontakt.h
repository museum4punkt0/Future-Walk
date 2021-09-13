//
//  Kontakt.h
//  Kontakt
//
//  Created by inx on 08.06.20.
//  Copyright © 2020 io. All rights reserved.
//

#ifndef Kontakt_h
#define Kontakt_h

#import <KontaktSDK/KontaktSDK.h>


@interface KDeviceDelegate : NSObject <KTKDevicesManagerDelegate>
- (void)devicesManager:(KTKDevicesManager*)manager didDiscoverDevices:(NSArray <KTKNearbyDevice*>*)devices;
- (void)devicesManagerDidFailToStartDiscovery:(KTKDevicesManager*)manager withError:(NSError*)error;
@end


@interface KontaktDelegate : NSObject <KTKDevicesManagerDelegate, KTKBeaconManagerDelegate>

@property KDeviceDelegate* devDelegate;
@property KTKDevicesManager* deviceManager;
@property KTKBeaconManager* beaconManager;
//@property KTKBeaconRegion* region;

- (KontaktDelegate *)init;
- (void)requestPermissions;

// device
- (void)startDeviceDiscovery;
- (void)stopDeviceDiscovery;

// regions
- (void)monitorRegion:(NSString*)identifier;
- (void)stopMonitorRegion:(NSString*)identifier;
- (void)stopMonitorAll;


// KTKDevicesManagerDelegate
- (void)devicesManager:(KTKDevicesManager*)manager didDiscoverDevices:(NSArray <KTKNearbyDevice*>*)devices;
- (void)devicesManagerDidFailToStartDiscovery:(KTKDevicesManager*)manager withError:(NSError*)error;

// KTKBeaconManagerDelegate
- (void)beaconManager:(KTKBeaconManager *)manager didChangeLocationAuthorizationStatus:(CLAuthorizationStatus)status;
- (void)beaconManager:(KTKBeaconManager *)manager didStartMonitoringForRegion:(__kindof KTKBeaconRegion *)region;
- (void)beaconManager:(KTKBeaconManager *)manager monitoringDidFailForRegion:(__kindof KTKBeaconRegion *_Nullable)region withError:(NSError *_Nullable)error;
- (void)beaconManager:(KTKBeaconManager *)manager didDetermineState:(CLRegionState)state forRegion:(__kindof KTKBeaconRegion *)region;
// ranging
- (void)beaconManager:(KTKBeaconManager *)manager didRangeBeacons:(NSArray *)beacons inRegion:(__kindof KTKBeaconRegion *)region;
- (void)beaconManager:(KTKBeaconManager *)manager rangingBeaconsDidFailForRegion:(__kindof KTKBeaconRegion *_Nullable)region withError:(NSError *_Nullable)error;
@end

#endif /* Kontakt_h */
