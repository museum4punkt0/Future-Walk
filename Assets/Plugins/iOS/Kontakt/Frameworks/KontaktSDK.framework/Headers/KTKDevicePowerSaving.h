//
//  KontaktSDK
//  Version: 3.0.1
//
//  Copyright © 2016 Kontakt.io. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/**
 *  Power saving features.
 */
typedef NS_OPTIONS(NSInteger, KTKDevicePowerSavingFeatures) {
    /**
     *  Power saving turned Off.
     */
    KTKDevicePowerSavingFeaturesOff    = 1 << 0,
    /**
     *  Power saving based on motion sensor.
     */
    KTKDevicePowerSavingFeaturesMotion = 1 << 1,
    /**
     *  Power saving based on light sensor.
     */
    KTKDevicePowerSavingFeaturesLight  = 1 << 2,
    /**
     *  Power saving based on real time clock.
     */
    KTKDevicePowerSavingFeaturesRTC    = 1 << 3,
    /**
     *  Burst mode Power Saving.
     *
     *  Beacon by default advertises continuously.
     *  When `Burst Mode` is enabled Beacon periodically advertises all configured frames for set amount seconds, then it sleeps.
     */
    KTKDevicePowerSavingFeaturesBurst  = 1 << 5,
};

#pragma mark - KTKDevicePowerSaving (Interface)
@interface KTKDevicePowerSaving : NSObject <NSObject, NSCopying, NSSecureCoding>

#pragma mark - General Properties
///--------------------------------------------------------------------
/// @name General Properties
///--------------------------------------------------------------------

@property (nonatomic, assign, readwrite) KTKDevicePowerSavingFeatures features;

#pragma mark - Light Sensor Properties
///--------------------------------------------------------------------
/// @name Light Sensor Properties
///--------------------------------------------------------------------

@property (nonatomic, assign, readwrite) NSInteger lightBrightnessLevel;

#pragma mark - RTC Properties
///--------------------------------------------------------------------
/// @name RTC Properties
///--------------------------------------------------------------------

@property (nonatomic, copy, readwrite) NSDictionary<NSNumber *, NSIndexSet *> * _Nullable rtcHours;

#pragma mark - Burst Mode Properties
///--------------------------------------------------------------------
/// @name Burst Mode Properties
///--------------------------------------------------------------------

/**
 *  Time interval in which the beacon will be in advertising mode.
 */
@property (nonatomic, assign, readwrite) NSTimeInterval burstOn;

/**
 *  Time interval in which the beacon will be in sleep mode.
 *
 *  This value will be rounded to full minutes.
 */
@property (nonatomic, assign, readwrite) NSTimeInterval burstOff;

@end

NS_ASSUME_NONNULL_END
