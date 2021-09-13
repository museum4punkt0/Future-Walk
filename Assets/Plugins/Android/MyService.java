package com.neeeu.audiowalk;

import android.annotation.TargetApi;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.Service;
import android.bluetooth.BluetoothAdapter;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.IBinder;
import android.util.Log;

import androidx.core.app.NotificationCompat;

import com.kontakt.sdk.android.ble.configuration.ScanMode;
import com.kontakt.sdk.android.ble.configuration.ScanPeriod;
import com.kontakt.sdk.android.ble.manager.ProximityManager;
import com.kontakt.sdk.android.ble.manager.ProximityManagerFactory;
import com.kontakt.sdk.android.ble.manager.listeners.IBeaconListener;
import com.kontakt.sdk.android.ble.manager.listeners.simple.SimpleIBeaconListener;
import com.kontakt.sdk.android.common.KontaktSDK;
import com.kontakt.sdk.android.common.profile.IBeaconDevice;
import com.kontakt.sdk.android.common.profile.IBeaconRegion;

import java.util.ArrayList;
import java.util.List;

import static android.util.Log.d;
import static android.util.Log.e;

public class MyService extends Service {

    public static final String TAG = "MyService";

    private static BeaconUpdated beaconCb = null;

    private static MyService instance;

    //----------------------------------------
    // static functions
    public static void setBeaconCallback(BeaconUpdated callback)
    {
        beaconCb = callback;
    }
    public static void startBeaconScan()
    {
        if (instance != null)
        {
            instance.startScanning();
        }
    }
    public static void stopBeaconScan()
    {
        if (instance != null)
        {
            instance.stopScanning();
        }
    }
    public static void monitorBeacon(String uid)
    {
        if (instance != null)
        {
            instance.addFilter(uid);
        }
    }
    public static void stopMonitorBeacon(String uid)
    {
        if (instance != null)
        {
            instance.removeFilter(uid);
        }
    }
    public static void stopMonitorAll()
    {
        if (instance != null)
        {
            instance.removeAllFilters();
        }
    }


    //----------------------------------------
    private boolean mServiceIsStarted;
    private boolean mNotificationIsOn;

//    public class MyBinder extends android.os.Binder {
//        MyService getService(){
//            // Simply return a reference to this instance
//            //of the Service.
//            return MyService.this;
//        }
//    }
//    private MyBinder mBinder = null;

    private ProximityManager proximityManager;
    private boolean hasBL = false;
    List<String> filterList = new ArrayList<>();

//    IBeaconFilter customIBeaconFilter = new IBeaconFilter() {
//        @Override
//        public boolean apply(IBeaconDevice iBeaconDevice) {
//            return filterList.contains(iBeaconDevice.getProximityUUID().toString().toLowerCase());
//        }
//    };

    @Override
    public void onCreate()
    {
        super.onCreate();

        instance = this;

//        BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
//        hasBL = mBluetoothAdapter != null;
//
//        if (hasBL
//            && !mBluetoothAdapter.isEnabled())
//        {
//            mBluetoothAdapter.enable();
//        }

        if (!KontaktSDK.isInitialized())
        {
            // TODO: do this not hardcoded...
            KontaktSDK.initialize("flRnhGsBVakhQtjgHvgxkoBZNJdcSIPx");
            setupProximityManager();
        }

        Log.i(TAG, "isPreAndroidO(): " + isPreAndroidO());

//        if (isAndroidO())
//        {
//            Log.i(TAG, "create notification channel");
//
//            final String CHANNEL_ID = "audiowalk_channel";
//            final NotificationChannel channel = new NotificationChannel(CHANNEL_ID,
//                    "Notification Channel for Audiowalk",
//                    NotificationManager.IMPORTANCE_DEFAULT);
//
//            ((NotificationManager)getSystemService(Context.NOTIFICATION_SERVICE)).createNotificationChannel(channel);
//
//            Log.i(TAG, "create notification");
//            final Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
//                    .setContentTitle("")
//                    .setContentText("").build();
//
//            Log.i(TAG, "start foreground");
//            startForeground(1, notification);
//        }
    }

    private void setupProximityManager()
    {
        // Create proximity manager instance
        proximityManager = ProximityManagerFactory.create(this);

        // Configure proximity manager basic options
        proximityManager.configuration()
                //Using ranging for continuous scanning or MONITORING for scanning with intervals
                .scanPeriod(ScanPeriod.RANGING)
                //Using BALANCED for best performance/battery ratio
                .scanMode(ScanMode.BALANCED);

        // Set up iBeacon listener
        proximityManager.setIBeaconListener(createIBeaconListener());

        // not for now...
//        proximityManager.filters().iBeaconFilter(customIBeaconFilter);
    }

    private IBeaconListener createIBeaconListener()
    {
        return new SimpleIBeaconListener() {
            @Override
            public void onIBeaconDiscovered(IBeaconDevice ibeacon, IBeaconRegion region)
            {
                Log.i(TAG, "onIBeaconDiscovered: " + ibeacon.toString());

                if (beaconCb != null)
                {
                    String uid = ibeacon.getProximityUUID().toString();

                    if (!uid.isEmpty())
                    {
                        new Thread(() -> beaconCb.beaconEnter(uid)).start();
                    }
                }
            }

            @Override
            public void onIBeaconsUpdated(List<IBeaconDevice> ibeacons, IBeaconRegion region)
            {
                for (IBeaconDevice ibeacon : ibeacons)
                {
                    Log.d(TAG, "onIBeaconsUpdated" + ibeacon.toString());
                }

                if (beaconCb != null)
                {
                    for (IBeaconDevice ibeacon : ibeacons)
                    {
                        String uid = ibeacon.getProximityUUID().toString();

                        if (!uid.isEmpty())
                        {
                            new Thread(() -> beaconCb.beaconUpdate(uid, ibeacon.getRssi())).start();
                        }
                    }
                }
            }

            @Override
            public void onIBeaconLost(IBeaconDevice ibeacon, IBeaconRegion region)
            {
                Log.e(TAG, "onIBeaconLost: " + ibeacon.toString());

                if (beaconCb != null)
                {
                    String uid = ibeacon.getProximityUUID().toString();

                    if (!uid.isEmpty())
                    {
                        new Thread(() -> beaconCb.beaconExit(uid)).start();
                    }
                }
            }
        };
    }

    private void startScanning()
    {
        if (!proximityManager.isScanning())
        {
            proximityManager.connect(() ->
            {
                Log.d(TAG, "startScanning");
                proximityManager.startScanning();
            });
        }
    }

    private void stopScanning()
    {
        if (proximityManager != null)
        {
            Log.d(TAG, "stopScanning");
            proximityManager.disconnect();
        }
    }

    void addFilter(String uid)
    {
        String _uid = uid.toLowerCase();

        if (!filterList.contains(_uid))
        {
            filterList.add(_uid);
        }
    }
    void removeFilter(String uid)
    {
        String _uid = uid.toLowerCase();
        filterList.remove(_uid);
    }
    void removeAllFilters()
    {
        filterList.clear();
    }

    @Override
    public IBinder onBind(Intent intent) {
//        Log.d(TAG, "BIND: " + Thread.currentThread().getName());
//        if (mBinder == null){
//            mBinder = new MyBinder();
//        }
//        return mBinder;

        return null;
    }
    @Override
    public boolean onUnbind(Intent i){
        return false;
    }


    @Override
    public int onStartCommand(Intent intent, int flags, int startId)
    {
        mServiceIsStarted = true;
        routeIntentToCommand(intent);

        return super.onStartCommand(intent, flags, startId);
    }

    @Override
    public void onDestroy()
    {
        if (proximityManager != null) {
            proximityManager.disconnect();
            proximityManager = null;
        }
        super.onDestroy();
    }

    private void routeIntentToCommand(Intent intent) {
        if (intent != null) {

            // process command
            if (MyIntentBuilder.containsCommand(intent)) {
                processCommand(MyIntentBuilder.getCommand(intent));
            }

            // process message
            if (MyIntentBuilder.containsMessage(intent)) {
                processMessage(MyIntentBuilder.getMessage(intent));
            }
        }
    }

    private void processMessage(String message)
    {
        try {
            d(TAG, String.format("doMessage: message from client: '%s'", message));

        } catch (Exception e) {
            e(TAG, "processMessage: exception", e);
        }
    }

    private void processCommand(int command) {
        try {
            switch (command) {
                case Command.START:
                    commandStart();
                    break;
                case Command.STOP:
                    commandStop();
                    break;
            }
        } catch (Exception e) {
            e(TAG, "processCommand: exception", e);
        }
    }

    private void commandStop()
    {
        // make sure to stop main activity by sending this intent
        Intent service_intent = new Intent("com.neeeu.MainActivity.stop");
        sendBroadcast(service_intent);

        // stop
        stopForeground(true);
        stopSelf();
        mServiceIsStarted = false;
        mNotificationIsOn = false;
    }

    private void commandStart()
    {
        if (!mServiceIsStarted) {
            moveToStartedState();
            return;
        }

        if (!mNotificationIsOn)
        {
            mNotificationIsOn = true;

            if (isPreAndroidO()) {
                HandleNotifications.PreO.createNotification(this);
            } else {
                HandleNotifications.O.createNotification(this);
            }
        }
    }

    @TargetApi(Build.VERSION_CODES.O)
    private void moveToStartedState() {

        Intent intent = new MyIntentBuilder(this)
                .setCommand(Command.START).build(MyService.class);
        if (isPreAndroidO()) {
            Log.d(TAG, "moveToStartedState: on N/lower");
            startService(intent);
        } else {
            Log.d(TAG, "moveToStartedState: on O");
            startForegroundService(intent);
        }
    }

    public static boolean isPreAndroidO() {
        return Build.VERSION.SDK_INT <= Build.VERSION_CODES.N_MR1;
    }

    public static boolean isAndroidO() {
        return Build.VERSION.SDK_INT >= Build.VERSION_CODES.O;
    }
}
