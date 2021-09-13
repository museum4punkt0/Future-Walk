//
//  NotificationPermission.mm
//  Permissions
//
//  Created by Ingo Randolf on 11.11.20.
//

#import <Foundation/Foundation.h>
#import <UserNotifications/UserNotifications.h>

#import "Permissions.h"

static PermissionCb _authStatusCallback = NULL;
static PermissionCb _authPermissionCallback = NULL;

@interface NotificationPermission : NSObject

+ (void)notificationsEnabled;
+ (void)requestNotificationPermission;

@end

@implementation NotificationPermission

+ (void)notificationsEnabled
{
	[[UNUserNotificationCenter currentNotificationCenter] getNotificationSettingsWithCompletionHandler: ^(UNNotificationSettings *settings)
	{
		if (_authStatusCallback)
		{
			_authStatusCallback(settings.authorizationStatus == UNAuthorizationStatusAuthorized);
		}
	}];
}

+ (void)requestNotificationPermission
{
	[[UNUserNotificationCenter currentNotificationCenter] requestAuthorizationWithOptions:(UNAuthorizationOptionBadge | UNAuthorizationOptionSound | UNAuthorizationOptionAlert) completionHandler:^(BOOL granted, NSError * _Nullable error)
	{
		if (error)
		{
			NSLog(@"%@", error);
			if (_authPermissionCallback)
			{
				_authPermissionCallback(false);
			}
		}
		else
		{
			NSLog(@"granted: %d", granted);
			if (_authPermissionCallback)
			{
				_authPermissionCallback(granted);
			}
		}
	}];
}

@end


//----------------------------------------------------
//----------------------------------------------------
// c interface
//----------------------------------------------------
//----------------------------------------------------

extern "C"
{
	void checkNotificationPermission()
	{
		[NotificationPermission notificationsEnabled];
	}

	void setNotificationStatusCallback(PermissionCb callback)
	{
		_authStatusCallback = callback;
	}

	void requestNotificationPermission()
	{
		[NotificationPermission requestNotificationPermission];
	}

	void setNotificationPermissionCallback(PermissionCb callback)
	{
		_authPermissionCallback = callback;
	}
}
