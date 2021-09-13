package com.neeeu.audiowalk;

public interface BeaconUpdated {
    void beaconEnter(String uid);
    void beaconUpdate(String uid, int rssi);
    void beaconExit(String uid);
}
