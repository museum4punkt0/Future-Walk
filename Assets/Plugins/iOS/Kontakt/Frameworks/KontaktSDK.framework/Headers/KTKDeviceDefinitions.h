//
//  KontaktSDK
//  Version: 3.0.1
//
//  Copyright (c) 2015 Kontakt.io. All rights reserved.
//

#import <Foundation/Foundation.h>

#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 100000 || __TV_OS_VERSION_MAX_ALLOWED >= 100000
#else
#define CBManagerState CBCentralManagerState
#define CBManagerStateUnknown CBCentralManagerStateUnknown
#define CBManagerStateResetting CBCentralManagerStateResetting
#define CBManagerStateUnsupported CBCentralManagerStateUnsupported
#define CBManagerStateUnauthorized CBCentralManagerStateUnauthorized
#define CBManagerStatePoweredOff CBCentralManagerStatePoweredOff
#define CBManagerStatePoweredOn CBCentralManagerStatePoweredOn
#endif

/**
 *  Device Connection Operation Types
 */
typedef NS_ENUM(NSInteger, KTKDeviceConnectionOperationType) {
    /**
     *  Type Unknown.
     */
    KTKDeviceConnectionOperationTypeUnknown = -1,
    /**
     *  Read Operation.
     */
    KTKDeviceConnectionOperationTypeRead    = 1,
    /**
     *  Write Operation.
     */
    KTKDeviceConnectionOperationTypeWrite   = 2,
    /**
     *  DFU Operation.
     */
    KTKDeviceConnectionOperationTypeDFU     = 3,
    /**
     *  Notification Operation.
     */
    KTKDeviceConnectionOperationTypeNotify     = 4
};

/**
 *  Configuration Profile Generator Types
 */
typedef NS_ENUM(NSInteger, KTKConfigProfileGenerator) {
    /**
     *  Generate using Cloud Only API Key required.
     */
    KTKConfigProfileGeneratorUsingCloud       = 1,
    
    /**
     *  Generate using KTKDeviceCredentails.
     */
    KTKConfigProfileGeneratorUsingCredentials = 2
};

/**
 *  A Kontakt device type.
 */
typedef NS_ENUM(NSInteger, KTKDeviceType) {
    /**
     *  Invalid device type.
     */
    KTKDeviceTypeInvalid     = -1,
    /**
     *  Beacon device type.
     */
    KTKDeviceTypeBeacon      = 1,
    /**
     *  Cloud Beacon device type.
     */
    KTKDeviceTypeCloudBeacon = 2
};

/**
 *  Legacy devices Advertising Profile
 */
typedef NS_ENUM(NSInteger, KTKDeviceAdvertisingProfile) {
    /**
     *  Invalid Profile.
     */
    KTKDeviceAdvertisingProfileInvalid   = -1,
    /**
     *  iBeacon Profile.
     */
    KTKDeviceAdvertisingProfileIBeacon   = 1,
    /**
     *  Eddystone Profile.
     */
    KTKDeviceAdvertisingProfileEddystone = 2
};

/**
 *  A device Advertising Packets
 */
typedef NS_OPTIONS(NSInteger, KTKDeviceAdvertisingPackets) {
    /**
     *  Ivalid packet.
     */
    KTKDeviceAdvertisingPacketsInvalid         = 1 << 0,
    /**
     *  iBeacon packet.
     */
    KTKDeviceAdvertisingPacketsIBeacon         = 1 << 1,
    /**
     *  Eddystone UID packet.
     */
    KTKDeviceAdvertisingPacketsEddystoneUID    = 1 << 2,
    /**
     *  Eddystone URL packet.
     */
    KTKDeviceAdvertisingPacketsEddystoneURL    = 1 << 3,
    /**
     *  Eddystone Telemetry packet.
     */
    KTKDeviceAdvertisingPacketsEddystoneTLM    = 1 << 4,
    /**
     *  Eddystone Telemetry packet.
     */
    KTKDeviceAdvertisingPacketsEddystoneEID    = 1 << 5,
    /**
     *  Eddystone Telemetry packet.
     */
    KTKDeviceAdvertisingPacketsEddystoneETLM   = 1 << 6,
    /**
     *  Kontakt identification packet.
     */
    KTKDeviceAdvertisingPacketsKontakt         = 1 << 7,
    /**
     *  Kontakt identification packet.
     */
    KTKDeviceAdvertisingPacketsKontaktTLM      = 1 << 8,
    /**
     *  iBeacon Button packet.
     */
    KTKDeviceAdvertisingPacketsIBeaconButton   = 1 << 9,
    /**
     *  All supprted Eddystone packets.
     */
    KTKDeviceAdvertisingPacketsEddystoneAll    = (NSInteger)0b00000000000000000000000000011100,
    /**
     *  All supprted Eddystone Secure (EID + ETLM).
     */
    KTKDeviceAdvertisingPacketsEddystoneSecure = (NSInteger)0b00000000000000000000000001100000,
    /**
     *  All supprted packets.
     */
    KTKDeviceAdvertisingPacketsAll             = (NSInteger)0b11111111111111111111111111111110
};

/**
 *  A device access rights.
 */
typedef NS_ENUM(NSInteger, KTKDeviceAccess) {
    /**
     *  Invalid value.
     */
    KTKDeviceAccessInvalid   = -1,
    /**
     *  Owner
     */
    KTKDeviceAccessOwner      = 1,
    /**
     *  Supervisor
     */
    KTKDeviceAccessSupervisor = 2,
    /**
     *  Editor
     */
    KTKDeviceAccessEditor     = 3,
    /**
     *  Viewer
     */
    KTKDeviceAccessViewer     = 4
};

/**
 *  A kontakt device specification.
 */
typedef NS_ENUM(NSInteger, KTKDeviceSpecification) {
    /**
     *  Invalid value.
     */
    KTKDeviceSpecificationInvalid  = -1,
    /**
     *  Standard specification.
     */
    KTKDeviceSpecificationStandard = 1,
    /**
     *  Tough Beacon specification.
     */
    KTKDeviceSpecificationTough = 2
};

/**
 *  A kontakt device models.
 */
typedef NS_ENUM(NSInteger, KTKDeviceModel) {
    /**
     *  Invalid value.
     */
    KTKDeviceModelInvalid  = -1,
    /**
     *  Unknown model.
     */
    KTKDeviceModelUnknown  = 0,
    /**
     *  Smart Beacon
     */
    KTKDeviceModelSmartBeacon = 1,
    /**
     *  USB Beacon
     */
    KTKDeviceModelUSBBeacon = 2,
    /**
     *  Sensor Beacon
     */
    KTKDeviceModelSensorBeacon = 3,
    /**
     *  Cloud Beacon
     */
    KTKDeviceModelCloudBeacon = 4,
    /**
     *  Card Beacon
     */
    KTKDeviceModelCardBeacon = 5,
    /**
     *  Pro Beacon
     */
    KTKDeviceModelProBeacon = 6,
    /**
     *  Gateway
     */
    KTKDeviceModelGateway = 7,
    /**
     *  Tag Beacon
     */
    KTKDeviceModelTagBeacon = 8,
    /**
     *  Smart Beacon 3 (Retrofit)
     */
    KTKDeviceModelSmartBeacon3 = 9,
    /**
     *  Heavy Duty Beacon
     */
    KTKDeviceModelHeavyDutyBeacon = 10,
    /**
     *  Card Beacon 2
     */
    KTKDeviceModelCardBeacon2 = 11,
};

/**
 *  A kontakt device symbols.
 */
typedef NS_ENUM(NSInteger, KTKDeviceSymbol) {
    KTKDeviceSymbolInvalid  = -1,
    KTKDeviceSymbolUnknown,
    KTKDeviceSymbolSB16_2,
    KTKDeviceSymbolTB15_1,
    KTKDeviceSymbolGW14_1,
    KTKDeviceSymbolUB16_2,
    KTKDeviceSymbolCT16_2,
    KTKDeviceSymbolGW16_2,
    KTKDeviceSymbolBP16_3,
    KTKDeviceSymbolS18_3,
    KTKDeviceSymbolSB18_3,
    KTKDeviceSymbolHD18_3,
    KTKDeviceSymbolCT18_3,
    KTKDeviceSymbolC18_3,
    KTKDeviceSymbolSB18_3H,
    KTKDeviceSymbolTB18_2,
    KTKDeviceSymbolBT18_3,
};

/**
 *  A kontakt device shuffle status.
 */
typedef NS_ENUM(NSInteger, KTKDeviceShuffleStatus) {
    /**
     *  Status is unknown.
     */
    KTKDeviceShuffleStatusUknown        = -1,
    /**
     *  Device shuffle is not supported.
     */
    KTKDeviceShuffleStatusNotSupported  = 0,
    /**
     *  Device shuffle status is ON.
     */
    KTKDeviceShuffleStatusON            = 1,
    /**
     *  Device shuffle status is OFF.
     */
    KTKDeviceShuffleStatusOFF           = 2
};

/**
 *  A device transmission power level.
 *
 *  @see https://support.kontakt.io/hc/en-gb/articles/201621521-Transmission-power-settings
 */
typedef NS_ENUM(NSInteger, KTKDeviceTransmissionPower) {
    /**
     *  Invalid value
     */
    KTKDeviceTransmissionPowerInvalid = -1,
    /**
     *  Transmission power level 0 (-30dBm).
     */
    KTKDeviceTransmissionPower0,
    /**
     *  Transmission power level 1 (-20dBm).
     */
    KTKDeviceTransmissionPower1,
    /**
     *  Transmission power level 2 (-16dBm).
     */
    KTKDeviceTransmissionPower2,
    /**
     *  Transmission power level 3 (-12dBm).
     */
    KTKDeviceTransmissionPower3,
    /**
     *  Transmission power level 4 (-8dBm).
     */
    KTKDeviceTransmissionPower4,
    /**
     *  Transmission power level 5 (-4dBm).
     */
    KTKDeviceTransmissionPower5,
    /**
     *  Transmission power level 6 (0dBm).
     */
    KTKDeviceTransmissionPower6,
    /**
     *  Transmission power level 7 (4dBm).
     */
    KTKDeviceTransmissionPower7,
};

/**
 *  A device motion detection modes.
 */
typedef NS_ENUM(NSInteger, KTKDeviceMotionDetectionMode) {
    /**
     *  Invalid mode.
     */
    KTKDeviceMotionDetectionModeInvalid  = -1,
    /**
     *  Motion detection is off.
     */
    KTKDeviceMotionDetectionModeOff      = 0,
    /**
     *  Motion detection is set in counting mode.
     * 
     *  You can access counter value by reading device configuration.
     *  
     *  @see [KTKDeviceConfiguration motionCounter]
     */
    KTKDeviceMotionDetectionModeCounting = 1,
    /**
     *  Motion detection is set in alarm mode.
     *
     *  When motion is detected device will advertise `4b6f6e74-616b-742e-696f-4d6f74696f6e` proximity UUID.
     *  Major and minor values will remain the same.
     */
    KTKDeviceMotionDetectionModeAlarm    = 2
};

/**
 *  A device data logger fields.
 */
typedef NS_OPTIONS(uint32_t, KTKDeviceDataLoggerFields) {
    KTKDeviceDataLoggerFieldsTemperature8   = 1 << 0,
    KTKDeviceDataLoggerFieldsTemperature16  = 1 << 1,
    KTKDeviceDataLoggerFieldsHumidity       = 1 << 2,
    KTKDeviceDataLoggerFieldsLightLevel     = 1 << 3,
    KTKDeviceDataLoggerFieldsAccelerometer  = 1 << 4,
    KTKDeviceDataLoggerFieldsDebugCounter   = 1 << 5,
    KTKDeviceDataLoggerFieldsTimestamp      = 1 << 6,
    KTKDeviceDataLoggerFieldsBattery        = 1 << 7,
};

/**
 *  A kontakt GPIO state options.
 */
typedef NS_ENUM(int8_t, KTKGPIOState) {
    KTKGPIOStateOff = -1,
    KTKGPIOStateLow,
    KTKGPIOStateHigh,
    KTKGPIOStateInput,
};

/**
 *  A kontakt device GPIOs states.
 */
struct KTKNearbyDeviceGPIOStates {
    KTKGPIOState pin[8];
};

/**
 *  A device acceleration structure.
 */
typedef struct {
    int8_t x;
    int8_t y;
    int8_t z;
} KTKDeviceAcceleration;
