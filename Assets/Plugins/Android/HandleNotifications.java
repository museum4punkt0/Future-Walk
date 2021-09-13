package com.neeeu.audiowalk;

import android.annotation.TargetApi;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;

import androidx.annotation.NonNull;
import androidx.core.app.NotificationCompat;

import com.unity3d.player.R;

import java.util.Random;

public class HandleNotifications {

    //
    // Common stuff.
    //

    public static int getRandomNumber() {
        return new Random().nextInt(100000);
    }
    public static final int ONGOING_NOTIFICATION_ID = getRandomNumber();
    public static final int SMALL_ICON = R.drawable.ic_fw_notification;

    /** PendingIntent to stop the service. */
    private static PendingIntent getStopServicePI(Service context) {
        PendingIntent piStopService;
        {
            Intent iStopService = new MyIntentBuilder(context).setCommand(Command.STOP).build(MyService.class);
            piStopService = PendingIntent.getService(context, getRandomNumber(), iStopService, 0);
        }
        return piStopService;
    }

    /** Get pending intent to launch the activity. */
    private static PendingIntent getLaunchActivityPI(Service context) {
        PendingIntent piLaunchMainActivity;
        {
            Intent iLaunchMainActivity = new Intent(context, MainActivity.class);
            piLaunchMainActivity =
                    PendingIntent.getActivity(context, getRandomNumber(), iLaunchMainActivity, 0);
        }
        return piLaunchMainActivity;
    }

    //
    // Pre O specific.
    //

    @TargetApi(25)
    public static class PreO {

        public static void createNotification(Service context) {
            // Create Pending Intents.
            PendingIntent piLaunchMainActivity = getLaunchActivityPI(context);
            PendingIntent piStopService = getStopServicePI(context);
            
            // Create a notification.
            Notification mNotification =
                    new NotificationCompat.Builder(context)
                            .setContentTitle(getNotificationTitle(context))
//                            .setContentText(getNotificationContent(context))
                            .setSmallIcon(SMALL_ICON)
                            .setContentIntent(piLaunchMainActivity)
                            .setStyle(new NotificationCompat.BigTextStyle())
                            .build();

            context.startForeground(ONGOING_NOTIFICATION_ID, mNotification);
        }
    }

    @NonNull
    private static String getNotificationContent(Service context) {
//        return context.getString(R.string.notification_text_content);
        return "";
    }

    @NonNull
    private static String getNotificationTitle(Service context) {
//        return context.getString(R.string.notification_text_title);
        return "Kulturforum Future Walk is running";
    }


    //
    // O Specific.
    //

    @TargetApi(26)
    public static class O {

        public static final String CHANNEL_ID = String.valueOf(getRandomNumber());

        public static void createNotification(Service context) {
            String channelId = createChannel(context);
            Notification notification = buildNotification(context, channelId);
            context.startForeground(ONGOING_NOTIFICATION_ID, notification);
        }

        private static Notification buildNotification(Service context, String channelId) {
            // Create Pending Intents.
            PendingIntent piLaunchMainActivity = getLaunchActivityPI(context);
            PendingIntent piStopService = getStopServicePI(context);

            // Create a notification.
            return new Notification.Builder(context, channelId)
                    .setContentTitle(getNotificationTitle(context))
//                    .setContentText(getNotificationContent(context))
                    .setSmallIcon(SMALL_ICON)
                    .setContentIntent(piLaunchMainActivity)
                    .setStyle(new Notification.BigTextStyle())
                    .build();
        }

        @NonNull
        private static String createChannel(Service context) {
            // Create a channel.
            NotificationManager notificationManager =
                    (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
            CharSequence channelName = "Playback channel";
            int importance = NotificationManager.IMPORTANCE_DEFAULT;
            NotificationChannel notificationChannel =
                    new NotificationChannel(CHANNEL_ID, channelName, importance);
            notificationManager.createNotificationChannel(notificationChannel);
            return CHANNEL_ID;
        }
    }

    @NonNull
    private static String getNotificationStopActionText(Service context) {
//        return context.getString(R.string.notification_stop_action_text);
        return "stop";
    }
}
