//
//  Locale.m
//  NativeToolkit
//
//  Created by Ryan on 31/01/2015.
//
//

#import "Locale.h"
#import "StringTools.h"

double latitude;
double longitude;
double altitude;
double lastTimestamp;

@implementation Locale

static CLLocationManager *locationManager = NULL;

// location callbacks
typedef void(*LocationCallback)(double, double, double, double);
static LocationCallback _locationCallback = NULL;



- (Locale *)init:(bool)background
{
	self.isBackgroundMode = background;
	
    locationManager = [[CLLocationManager alloc] init];
    locationManager.delegate = self;
	
	return self;
}

- (void)stopLocation
{
	[locationManager stopUpdatingLocation];
}

- (void)setupLocationManagerAndStart
{
	[locationManager setDesiredAccuracy:kCLLocationAccuracyBest];
    [locationManager setDistanceFilter:kCLDistanceFilterNone];
	
	if (self.isBackgroundMode)
    {
		locationManager.pausesLocationUpdatesAutomatically = YES;
		locationManager.activityType = CLActivityTypeFitness;
		
		// For iOS9 we have to call this method if we want to receive location updates in background mode
		if([locationManager respondsToSelector:@selector(allowsBackgroundLocationUpdates)])
		{
			[locationManager setAllowsBackgroundLocationUpdates:YES];
		}
		
		
		if (@available(iOS 11.0, *)) {
			[locationManager setShowsBackgroundLocationIndicator:YES];
		} else {
			// Fallback on earlier versions
		}
		
		if([[[UIDevice currentDevice] systemVersion] floatValue] >= 8.0)
			[locationManager requestAlwaysAuthorization];
	}
	else
	{
		// set default values
		locationManager.pausesLocationUpdatesAutomatically = YES;
		locationManager.activityType = CLActivityTypeOther;
		
		if([locationManager respondsToSelector:@selector(allowsBackgroundLocationUpdates)])
		{
			[locationManager setAllowsBackgroundLocationUpdates:NO];
		}
		
		if (@available(iOS 11.0, *)) {
			[locationManager setShowsBackgroundLocationIndicator:NO];
		} else {
			// Fallback on earlier versions
		}
		
		if([[[UIDevice currentDevice] systemVersion] floatValue] >= 8.0)
			[locationManager requestWhenInUseAuthorization];
	}
	
	// finally start location manager
    [locationManager startUpdatingLocation];
}

- (void)restartLocation:(bool)background
{
	if (self.isBackgroundMode != background)
	{
		self.isBackgroundMode = background;

		[locationManager stopUpdatingLocation];

		[self setupLocationManagerAndStart];
	}
}


// CLLocationManagerDelegate
- (void)locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray *)locations;
{
    CLLocation *location = [locations lastObject];
    latitude = location.coordinate.latitude;
	longitude = location.coordinate.longitude;
	altitude = location.altitude;
	lastTimestamp = location.timestamp.timeIntervalSince1970;
    
//    NSLog(@"### %@ : lat:%f long:%f alt:%f", location.timestamp, latitude, longitude, altitude);
	
	if(_locationCallback != NULL)
	{
		_locationCallback(lastTimestamp, latitude, longitude, altitude);
	}
}

@end

static Locale* localeDelegate = NULL;

extern "C"
{
    char* getLocale()
    {
        NSLocale *locale = [NSLocale currentLocale];
        NSString *countryCode = [locale objectForKey: NSLocaleCountryCode];
        
        NSLog(@"##locale: %@", countryCode);
        
        return [StringTools createCString:[countryCode UTF8String]];
    }

	void locationInit(bool background)
	{
		if(localeDelegate == NULL)
		{
			localeDelegate = [[Locale alloc] init: background];
		}
	}
    
    void startLocation(bool background)
    {
        if(localeDelegate == NULL)
		{
			localeDelegate = [[Locale alloc] init: background];
		}
		
		if (localeDelegate != NULL)
		{
			[localeDelegate setupLocationManagerAndStart];
		}
    }

	void stopLocation()
	{
		if(localeDelegate != NULL)
		{
			[localeDelegate stopLocation];
		}
	}

	void restartLocation(bool background)
	{
		if(localeDelegate == NULL)
		{
			localeDelegate = [[Locale alloc] init: background];
			[localeDelegate setupLocationManagerAndStart];
		}
		else
		{
			[localeDelegate restartLocation:background];
		}
	}
    
    double getLongitude()
    {
        return longitude;
    }
    
    double getLatitude()
    {
        return latitude;
    }

	double getAltitude()
	{
		return altitude;
	}

	double getLastTimestamp()
	{
		return lastTimestamp;
	}

	void setupLocationCallback(LocationCallback callback)
	{
		_locationCallback = callback;
	}
}
