using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System;

public class BeaconController : MonoBehaviour
{
    //------------------------------------
    // static
    //------------------------------------
    public static BeaconController instance;

    public static int threshold = -95;
    public static int outthreshold = -105;

    List<string> uuidFilter = new List<string>();

    //--------------------------------------------------------
    // Beacon class
    //--------------------------------------------------------
     class Beacon
    {
        public string name;
        public int rssi;
        public DateTime age;
        public bool entered = false;

        public Beacon(string name, int rssi)
        {
            this.name = name;
            SetRssi(rssi);
        }

        public void SetRssi(int rssi)
        {
            this.rssi = rssi;
            this.age = DateTime.Now;

            if (rssi > threshold)
            {
                entered = true;
            }
            else if (rssi < outthreshold)
            {
                entered = false;
            }
        }

        public override string ToString()
        {
            return name + " - " + rssi;
        }
    }
    //--------------------------------------------------------
    //--------------------------------------------------------

    public Text outputText;

    bool hasFocus = true;
    bool scanStarted = false;

    readonly Dictionary<string, Beacon> beaconInfos = new Dictionary<string, Beacon>();

    public delegate void EnterCallbackDelegate(string id);
    public delegate void ExitCallbackDelegate(string id);
    readonly List<EnterCallbackDelegate> enterCbList = new List<EnterCallbackDelegate>();
    readonly List<ExitCallbackDelegate> exitCbList = new List<ExitCallbackDelegate>();

    //--------------------------------------------------------
    // uuid to name mapping
    //--------------------------------------------------------
    // (waitForBeacon: 120, "Scene1_DataTree1")
    readonly Dictionary<string, string> beaconIdLookup = new Dictionary<string, string>
    {
        // neeeu beacons
        { "a0696b2f-50aa-4e8c-81c5-192da2d34bc2", "VfKg" },             // VfKg
        { "34ebc380-df4e-4e15-9167-1c33ea8e217d", "EZva" },             // EZva
        { "142714b3-7ffb-4e17-a0d7-74ad5d736710", "TFqn" },             // TFqn
        { "70bf5846-51fb-45b5-9d63-3a9616ad6054", "SmNU" },             // SmNU

        // SMB beacons
        // GG
        {"2eaa77c2-f9f6-450c-8a77-9358303a4101", "gg_room1_a"},         // 3FLN
        {"4e13ba12-b07a-466b-9309-74b42f3992dd", "gg_room1_b"},         // Pq3G
        {"a8303087-a0ec-44f3-a03c-5a0bdb45ea5c", "gg_room9_a"},         // 588n
        {"d210ca1d-40dc-4ff8-ad0f-881324b2e57d", "gg_room9_b"},         // KIgA
        {"7f91315f-8caa-42ac-8d30-a8b0b9114fc2", "gg_room18_a"},        // Cjql
        {"4efaaa6c-5bcc-42d6-b791-fcace476cc8a", "gg_room18_b"},        // 7h0Q
        {"ca61dfcc-da91-4a2f-aa14-cbce32fbad7f", "gg_room22_a"},        // DGy1
        {"ff8e5296-95dc-4b09-a851-32a43eb7f0ea", "gg_room22_b"},        // 8Hvt
        {"d898b44b-950d-4f48-92c6-0186cd763b4c", "gg_entrance_a"},      // QZnH
        {"9a139523-af53-4274-b90f-19d8cc4b565a", "gg_entrance_b"},      // 3CFQ
        // KGM
        {"3b6eab7c-8348-4633-b3e9-c5b823034542", "kgm_entrance_f3_a"},  // WKIS
        {"03a1f84c-2d5c-4f6c-87d8-36d0f6ee4a9a", "kgm_entrance_f3_b"},  // rykB
        {"6807de12-819d-49d9-9e23-8ab16db35ab5", "kgm_entrance_f2_a"},  // QbI0
        {"f7826da6-4fa2-4e98-8024-bc5b71e0893e", "kgm_entrance_f2_b"},  // w2in
        // MiM
        {"15458876-f56a-4894-9861-12beaf75b1c1", "mim_entrance_a"},     // Xnjz
        {"7d36daee-290a-4e44-bdd1-3eab8db00ede", "mim_entrance_b"},     // urDi

        //
        {"8c644035-bdab-474a-9cda-71a32fa084e8", "kbc2"},               // kbc2
        {"978b85b6-b141-40df-9841-f6e490c585a0", "klp8"},               // klp8
    };
    readonly Dictionary<string, string> beaconIdLookupRev = new Dictionary<string, string>
    {
        // neeeu beacons
        { "VfKg", "a0696b2f-50aa-4e8c-81c5-192da2d34bc2" },             // VfKg
        { "EZva", "34ebc380-df4e-4e15-9167-1c33ea8e217d" },             // EZva
        { "TFqn", "142714b3-7ffb-4e17-a0d7-74ad5d736710" },             // TFqn
        { "SmNU", "70bf5846-51fb-45b5-9d63-3a9616ad6054" },             // SmNU

        // SMB beacons
        // GG
        {"gg_room1_a", "2eaa77c2-f9f6-450c-8a77-9358303a4101"},         // 3FLN
        {"gg_room1_b", "4e13ba12-b07a-466b-9309-74b42f3992dd"},         // Pq3G
        {"gg_room9_a", "a8303087-a0ec-44f3-a03c-5a0bdb45ea5c"},         // 588n
        {"gg_room9_b", "d210ca1d-40dc-4ff8-ad0f-881324b2e57d"},         // KIgA
        {"gg_room18_a", "7f91315f-8caa-42ac-8d30-a8b0b9114fc2"},        // Cjql
        {"gg_room18_b", "4efaaa6c-5bcc-42d6-b791-fcace476cc8a"},        // 7h0Q
        {"gg_room22_a", "ca61dfcc-da91-4a2f-aa14-cbce32fbad7f"},        // DGy1
        {"gg_room22_b", "ff8e5296-95dc-4b09-a851-32a43eb7f0ea"},        // 8Hvt
        {"gg_entrance_a", "d898b44b-950d-4f48-92c6-0186cd763b4c"},      // QZnH
        {"gg_entrance_b", "9a139523-af53-4274-b90f-19d8cc4b565a"},      // 3CFQ
        // KGM        
        {"kgm_entrance_f3_a", "3b6eab7c-8348-4633-b3e9-c5b823034542"},  // WKIS
        {"kgm_entrance_f3_b", "03a1f84c-2d5c-4f6c-87d8-36d0f6ee4a9a"},  // rykB
        {"kgm_entrance_f2_a", "6807de12-819d-49d9-9e23-8ab16db35ab5"},  // QbI0
        {"kgm_entrance_f2_b", "f7826da6-4fa2-4e98-8024-bc5b71e0893e"},  // w2in
        // MiM
        {"mim_entrance_a", "15458876-f56a-4894-9861-12beaf75b1c1"},     // Xnjz
        {"mim_entrance_b", "7d36daee-290a-4e44-bdd1-3eab8db00ede"},     // urDi

        //
        {"kbc2", "8c644035-bdab-474a-9cda-71a32fa084e8"},               // kbc2
        {"klp8", "978b85b6-b141-40df-9841-f6e490c585a0"},               // klp8
    };

    //--------------------------------------------------------
    //--------------------------------------------------------

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        hasFocus = true;

        // set callbacks
        KontaktIO.SetCallbacks(OnBeaconEnter, OnBeaconUpdate, OnBeaconExit);

        // generate some random UUIDs
        // List<string> list = new List<string>();
        // for (int i = 0; i < 5; i++)
        // {            
        //     list.Add(System.Guid.NewGuid().ToString());
        // }
        // Debug.Log(string.Join(", ", list));
        
    }

    void OnApplicationFocus(bool focus)
	{
        hasFocus = focus;

        if (focus)
        {
            showBeaconText();
        }
	}

    void OnApplicationQuit()
    {
        KontaktIO.StopAll();
    }

    //--------------------------------------------------------
    //--------------------------------------------------------
    private void OnBeaconEnter(string id)
    {
#if UNITY_ANDROID
        if (!uuidFilter.Contains(id)) return;
#endif
    
        var b = new Beacon(id, int.MinValue);
        beaconInfos.Add(id, b);
    }

    private void OnBeaconExit(string id)
    {
        beaconInfos.Remove(id);
    }

    private void OnBeaconUpdate(string id, int rssi)
    {
#if UNITY_ANDROID
        if (!uuidFilter.Contains(id)) return;
#endif

        // Debug.Log("BEACON: " + id + " - rssi: " + rssi + " - thread: " + Thread.CurrentThread.ManagedThreadId);
        
        if (beaconInfos.ContainsKey(id))
        {
            bool wasE = beaconInfos[id].entered;
            beaconInfos[id].SetRssi(rssi);

            if (beaconInfos[id].entered && !wasE)
            {
                callEntered(id);
            }
            else if (!beaconInfos[id].entered && wasE)
            {
                callExited(id);
            }
        }
        else
        {
            var b = new Beacon(id, rssi);
            beaconInfos.Add(id, b);

            if (b.entered)
            {
                callEntered(id);
            }
        }

        if (hasFocus)
        {
            showBeaconText();
        }
    }

    private void showBeaconText()
    {
        if (outputText)
        {
            UnityToolbag.Dispatcher.InvokeAsync(() =>
            {
                string o = "";
                foreach (var item in beaconInfos)
                {
                    o += item.Value.ToString() + "\n";
                }
                outputText.text = o;                
            });
        }
    }

    // -----------------  -----------------------

    public void Reset()
    {
        KontaktIO.StopMonitor();
        beaconInfos.Clear();
        uuidFilter.Clear();
        StopAll();
    }

    public void StartScan()
    {
        if (scanStarted) return;

        UnityToolbag.Dispatcher.InvokeAsync(() =>
        {
            KontaktIO.StartScan();
            scanStarted = true;
        });
    }


    public void StopAll()
    {
        if (!scanStarted) return;

        UnityToolbag.Dispatcher.InvokeAsync(() =>
        {
            KontaktIO.StopAll();
            scanStarted = false;
        });
    }

    public void Monitor(string bn)
    {
        var id = BeaconId(bn);
        if (string.IsNullOrEmpty(id))
        {
            Log.d("could not get id for beacon: " + bn);
            return;
        }

#if UNITY_IOS        
        KontaktIO.Monitor(id);
#elif UNITY_ANDROID
        // this may be called via the Story from a thread
        // we only can call UnityJNI from the main-thread

        // -> filter in c#
        if (!uuidFilter.Contains(id))
        {
            uuidFilter.Add(id);
        }
#endif
    }

    public bool HasEntered(string bn)
    {
        string id = BeaconId(bn);

        if (beaconInfos.ContainsKey(id))
        {
            return beaconInfos[id].entered;
        }

        return false;
    }

    private string BeaconId(string bn)
    {
        if (beaconIdLookupRev.ContainsKey(bn))
        {
            return beaconIdLookupRev[bn];
        }

        return "";
    }

    private string BeaconName(string id)
    {
        string _id = id.ToLower();
        if (beaconIdLookup.ContainsKey(_id))
        {
            return beaconIdLookup[_id];
        }

        return id;
    }

    // ----------------- enter callback -----------------------

    public void AddOnEnterBeacon(EnterCallbackDelegate cb)
    {
        if (!enterCbList.Contains(cb))
        {
            enterCbList.Add(cb);
        }
    }
    public void RemoveOnEnterBeacon(EnterCallbackDelegate cb)
    {
        if (enterCbList.Contains(cb))
        {
            enterCbList.Remove(cb);
        }
    }

    private void callEntered(string id)
    {
        string bn = BeaconName(id);

        foreach (var cb in enterCbList)
        {
            cb(bn);
        }
    }

    // ----------------- exit callback -----------------------

    public void AddOnExitBeacon(ExitCallbackDelegate cb)
    {
        if (!exitCbList.Contains(cb))
        {
            exitCbList.Add(cb);
        }
    }
    public void RemoveOnExitBeacon(ExitCallbackDelegate cb)
    {
        if (exitCbList.Contains(cb))
        {
            exitCbList.Remove(cb);
        }
    }

    private void callExited(string id)
    {
        string bn = BeaconName(id);

        foreach (var cb in exitCbList)
        {
            cb(bn);
        }
    }
}
