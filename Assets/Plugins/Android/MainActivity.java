package com.neeeu.audiowalk;

import android.Manifest;
import android.bluetooth.BluetoothAdapter;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.davikingcode.DetectHeadset.DetectHeadset;
import com.unity3d.nostatusbar.UnityPlayerActivityStatusBar;
import com.unity3d.player.UnityPlayer;

public class MainActivity extends UnityPlayerActivityStatusBar {

    public static final String TAG = "MainActivity";

    //----------
    // binder
    //private boolean mServiceBound = false;
    //MyService mService = null;

//    private ServiceConnection mServiceConnection = new ServiceConnection(){
//
//        @Override
//        public void onServiceConnected(ComponentName cName, IBinder service)
//        {
//            MyService.MyBinder binder = (MyService.MyBinder) service;
//            mService = binder.getService();
//
//            Intent intent = MyIntentBuilder.getInstance(MainActivity.this).setCommand(Command.START).build(MyService.class);
//            if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O)
//            {
//                startService(intent);
//            }
//            else
//            {
//                startForegroundService(intent);
//            }
//
//            mServiceBound = true;
//        }
//
//        @Override
//        public void onServiceDisconnected(ComponentName cName){
//            mServiceBound= false;
//        }
//    };

    //----------
    // detect headset
    public static DetectHeadset detectHeadset;
    public static boolean _Detect()
    {
        if (detectHeadset != null)
        {
            return detectHeadset._Detect();
        }

        // this should never happen
        Log.d(TAG, "no detectHeadsetObject!!");

        return false;
    }

    BroadcastReceiver updateUIReciver;


    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        // don't pause unity on activity pause
        pausePlayerOnPause = false;

        detectHeadset = new DetectHeadset(this);

        //------------------------------------
        // receive stop signal from service
        IntentFilter filter = new IntentFilter();
        filter.addAction("com.neeeu.MainActivity.stop");
        updateUIReciver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                Log.i(TAG, "receive: " + intent.getAction());
                //UI update here
                MainActivity.this.finishAndRemoveTask();
            }
        };
        registerReceiver(updateUIReciver, filter);

        //------------------------------------
        // start background service
        Intent service_intent = MyIntentBuilder.getInstance(this).setCommand(Command.START).build(MyService.class);
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O)
        {
            startService(service_intent);
        }
        else
        {
            startForegroundService(service_intent);
        }

        // bindService(
        //         new MyIntentBuilder(this).build(MyService.class),
        //         mServiceConnection,
        //         BIND_AUTO_CREATE);
    }

    @Override
    protected void onStart() {
        super.onStart();
        //checkPermission();
    }

    @Override
    protected void onResume() {
        super.onResume();

        registerReceiver(mReceiver, new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED));
    }

    private final BroadcastReceiver mReceiver = new BroadcastReceiver() {

        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();

            if (BluetoothAdapter.ACTION_STATE_CHANGED.equals(action)) {
                int state = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, -1);

                switch (state) {
                    case BluetoothAdapter.STATE_OFF:
                        //Indicates the local Bluetooth adapter is off.

                        if (blStateCb != null)
                        {
                            blStateCb.onStateChanged(false);
                            blStateCb = null;
                        }

                        break;

                    case BluetoothAdapter.STATE_TURNING_ON:
                        //Indicates the local Bluetooth adapter is turning on. However local clients should wait for STATE_ON before attempting to use the adapter.
                        break;

                    case BluetoothAdapter.STATE_ON:
                        //Indicates the local Bluetooth adapter is on, and ready for use.

                        if (blStateCb != null)
                        {
                            blStateCb.onStateChanged(true);
                            blStateCb = null;
                        }

                        break;

                    case BluetoothAdapter.STATE_TURNING_OFF:
                        //Indicates the local Bluetooth adapter is turning off. Local clients should immediately attempt graceful disconnection of any remote links.
                        break;
                }
            }
        }
    };

    @Override
    protected void onDestroy()
    {
        // make sure to stop background service
        Intent service_intent = MyIntentBuilder.getInstance(this).setCommand(Command.STOP).build(MyService.class);
        stopService(service_intent);

        // if (mServiceBound) {
        //     unbindService(mServiceConnection);
        //     mServiceBound = false;
        // }

        super.onDestroy();
    }

    private void checkPermission()
    {
        int checkSelfPermissionResult = ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_COARSE_LOCATION);
        if (PackageManager.PERMISSION_GRANTED == checkSelfPermissionResult)
        {
            //already granted
        }
        else
        {
            if (ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.ACCESS_COARSE_LOCATION))
            {
                //we should show some explanation for user here
            }
            else
            {
                //request permission
                ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.ACCESS_COARSE_LOCATION}, 100);
            }
        }
    }

    //--------------------------------
    // permission manager
    //--------------------------------

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults)
    {
        int fine_location = -1;
        int bluetooth = -1;

        int i=0;
        for (String s : permissions)
        {
            if (s.equals(Manifest.permission.ACCESS_FINE_LOCATION)) fine_location = i;
            if (s.equals(Manifest.permission.BLUETOOTH)) bluetooth = i;
            i++;
        }

        if (fine_location >= 0)
        {
            if (locationPermissionCb != null)
            {
                locationPermissionCb.onPermissionChanged(grantResults[fine_location] == PackageManager.PERMISSION_GRANTED);
            }
        }

        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    private static PermissionRequestCallback locationPermissionCb;

    public static void setLocationPermissionCallback(PermissionRequestCallback callback) {
        locationPermissionCb = callback;
    }

    public static boolean checkLocationPermission()
    {
        return ContextCompat.checkSelfPermission(UnityPlayer.currentActivity, Manifest.permission.ACCESS_COARSE_LOCATION) == PackageManager.PERMISSION_GRANTED &&
                ContextCompat.checkSelfPermission(UnityPlayer.currentActivity, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED;
    }

    public static void requestLocationPermission()
    {
        if (checkLocationPermission())
        {
            return;
        }

        ActivityCompat.requestPermissions(UnityPlayer.currentActivity,
                new String[]{Manifest.permission.ACCESS_FINE_LOCATION}, 1);
    }


    //----------------------------------------
    // bluetooth
    //----------------------------------------

    private static BluetoothStateCallback blStateCb;

    public static boolean hasBluetooth()
    {
        BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
        return mBluetoothAdapter != null;
    }

    public static boolean checkBluetoothOn()
    {
        BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();

        if (mBluetoothAdapter == null)
        {
            return false;
        }

        return mBluetoothAdapter.isEnabled();
    }

    public static boolean turnBluetoothOn(BluetoothStateCallback cb)
    {
        blStateCb = cb;

        BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();

        if (mBluetoothAdapter == null)
        {
            // just continue
            return true;
        }

        if (!mBluetoothAdapter.isEnabled())
        {
            mBluetoothAdapter.enable();
        }

        //
        return mBluetoothAdapter.isEnabled();
    }


    //----------------------------------------
    // camera
    //----------------------------------------
    public static boolean cameraAuthorizationStatus()
    {
        return ContextCompat.checkSelfPermission(UnityPlayer.currentActivity, Manifest.permission.CAMERA) == PackageManager.PERMISSION_GRANTED;
    }

}
