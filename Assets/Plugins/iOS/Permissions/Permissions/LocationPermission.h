//
//  LocationPermission.h
//  Permissions
//
//  Created by Ingo Randolf on 11.11.20.
//

#ifndef LocationPermission_h
#define LocationPermission_h

#import <CoreLocation/CoreLocation.h>

@interface LocationPermission : NSObject <CLLocationManagerDelegate>

@property bool _background;
@property CLLocationManager* locationManager;

- (LocationPermission*)init:(bool)background;
- (bool)checkPermission;
- (void)requestPermission;

// CLLocationManagerDelegate
- (void)locationManager:(CLLocationManager *)manager didChangeAuthorizationStatus:(CLAuthorizationStatus)status;

@end

#endif /* LocationPermission_h */
