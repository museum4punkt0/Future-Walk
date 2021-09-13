//
//  Locale.h
//  NativeToolkit
//
//  Created by Ryan on 31/01/2015.
//
//

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#import <UIKit/UIKit.h>

@interface Locale : NSObject <CLLocationManagerDelegate>

@property bool isBackgroundMode;
@property bool _deferringUpdates;

- (Locale *)init:(bool)background;
- (void)stopLocation;
- (void)restartLocation:(bool)background;
- (void)setupLocationManagerAndStart;

// CLLocationManagerDelegate
- (void)locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray *)locations;

@end
