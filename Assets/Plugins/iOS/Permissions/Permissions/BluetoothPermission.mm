//
//  BluetoothPermission.mm
//  Permissions
//
//  Created by Ingo Randolf on 11.11.20.
//

#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

typedef void(*CBMStateUpdateCb)(int status);
static CBMStateUpdateCb _cbmStateUpdateCallback = NULL;

@interface BluetoothDelegate : NSObject <CBCentralManagerDelegate>

@property CBCentralManager* bluetoothManager;

- (BluetoothDelegate *)init;
- (BOOL)checkState:(CBManagerState)state;

@end

@implementation BluetoothDelegate

- (BluetoothDelegate *)init
{
    self.bluetoothManager = [[CBCentralManager alloc] initWithDelegate:self queue:dispatch_get_main_queue()];
	
	return self;
}

- (BOOL)checkState:(CBManagerState)state
{
	return self.bluetoothManager.state == state;
}

- (BOOL)checkPermission:(CBManagerAuthorization)permission
API_AVAILABLE(ios(13.0))
{
	return self.bluetoothManager.authorization == permission;
}

- (void)centralManagerDidUpdateState:(nonnull CBCentralManager *)bluetoothManager
{
	NSString *stateString = nil;
	switch(bluetoothManager.state)
    {
        case CBManagerStateResetting:
			stateString = @"Bluetooth is resetting.";
            break;

        case CBManagerStateUnsupported:
			stateString = @"Bluetooth is unsupported.";
            break;

        case CBManagerStateUnauthorized:
			stateString = @"Bluetooth unauthoriezes.";
            break;
			
        case CBManagerStatePoweredOff:
            stateString = @"Bluetooth is currently powered off.";
            break;
			
        case CBManagerStatePoweredOn:
            stateString = @"Bluetooth is currently powered on and available to use.";
            break;
			
        default:
            stateString = @"State unknown, update imminent.";
            break;
    }
	
	NSLog(@"Bluetooth State %@",stateString);
	
	if (_cbmStateUpdateCallback)
	{
		_cbmStateUpdateCallback(int(bluetoothManager.state));
	}
}

@end




//----------------------------------------------------
//----------------------------------------------------
// c interface
//----------------------------------------------------
//----------------------------------------------------

static BluetoothDelegate* btDelegate = NULL;

extern "C"
{
	void setBluetoothStateUpdatedCallback(CBMStateUpdateCb callback)
	{
		_cbmStateUpdateCallback = callback;
	}

	void _bluetoothDelegateInit()
	{
		if (btDelegate == NULL)
		{
			btDelegate = [[BluetoothDelegate alloc] init];
		}
	}
	
	void bluetoothState()
	{
		_bluetoothDelegateInit();
		
		if (btDelegate)
		{
			[btDelegate centralManagerDidUpdateState: btDelegate.bluetoothManager];
		}
	}

	bool checkBluetoothOn()
	{
		_bluetoothDelegateInit();
		
		if (btDelegate)
		{
			return [btDelegate checkState: CBManagerStatePoweredOn];
		}
		
		NSLog(@"check bluetooth on - fail no manager");
		
		return false;
	}

	bool checkBluetoothPermission()
	{
		_bluetoothDelegateInit();
		
		if (btDelegate)
		{
			if (@available(iOS 13.0, *)) {
				return ![btDelegate checkPermission: CBManagerAuthorizationAllowedAlways];
			} else {
				// Fallback on earlier versions
				return checkBluetoothOn();
			}
		}
		
		NSLog(@"check bluetooth permission - fail no manager");
		
		return false;
	}
}
