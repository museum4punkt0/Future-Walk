using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;

[System.Serializable]
public class AppSettings
{
    public string language = "german";
    public bool onboarded = false;
    public bool accessible = false;
    public bool firstTimeFakeCamera = false;
    public int andreaQuestion = 1;
    public int plantsQuestion = 1;
    public int calamityQuestion = 1;
    public int antiphoneQuestion = 1;
    public bool showAudioTranscript = true;
    public bool permissionsDone = false;
    public List<string> arcsDone = new List<string>();
}


[DisallowMultipleComponent]
public class GlobalSettings : MonoBehaviour
{
    public enum Language {
        EN = 0,
        DE,
    }

    //--------------------------------------------
    //--------------------------------------------
    public static GlobalSettings instance;

    public static string SystemInfoDeviceModel = "";

    public static readonly string MOTOKO_NAME = "Motoko";
    public static readonly string ANDREA_NAME = "Andrea";

    public static readonly string ENGLISH_STR = "english";
    public static readonly string GERMAN_STR = "german";

    public static readonly string MUSEUM_CHOOSER = "A_MuseumChoice";
    public static readonly string QUESTIONAIR_SCENE_STR = "Questionair";

    public static readonly string BUTTON_CONTINUE_EN = "Continue";
    public static readonly string BUTTON_CONTINUE_DE = "Weiter";

    public static readonly string IS_TALKING_EN = " is talking";
    public static readonly string IS_TALKING_DE = " spricht";

    // museum names
    public static readonly int ARC_COUNT = 4;

    public static readonly string NAME_MIM_EN = "Musikinstrumenten-Museum";
    public static readonly string NAME_MIM_DE = "Musikinstrumenten-Museum";

    public static readonly string NAME_KGM_EN = "Kunstgewerbemuseum";
    public static readonly string NAME_KGM_DE = "Kunstgewerbemuseum";

    public static readonly string NAME_GG_EN = "Gemäldegalerie";
    public static readonly string NAME_GG_DE = "Gemäldegalerie";

    public static readonly string NAME_OUT_EN = "outside";
    public static readonly string NAME_OUT_DE = "Außenbereich";



#if UNITY_EDITOR
    public static readonly int WATCHDOG_TIMEOUT = 10 * 1000; //1800 * 1000; // 30 minutes
#else
    public static readonly int WATCHDOG_TIMEOUT = 600 * 1000; //1800 * 1000; // 30 minutes
#endif

    // TODO remove those
    public static readonly bool doTimeoutInterrupt = false;
    public static readonly bool DO_GPS_INTERRUPT = false;


    //--------------------------------------------
    //--------------------------------------------
    [SerializeField] private bool onboardingDone = false;
    [SerializeField] private bool accessibilityEnabled = false;
    [SerializeField] private bool _doDebugButtons = false;
    [SerializeField] private GameObject debugButtons;
    [SerializeField] private GameObject burgerMenu;
    [SerializeField] private GameObject secretDebugSwitch;
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject ARContainer;

    // delegate and event for sending the state of accessibility
    public delegate void AccessibilityAction(bool on);
    public static event AccessibilityAction OnAccessibilityToggle;

    public bool doDebugButtons { get { return _doDebugButtons; } }

    // settings
    private string settingsFile = default;
    private AppSettings appSettings = default;
    private List<GameObject> englishGO = new List<GameObject>();
    private List<GameObject> germanGO = new List<GameObject>();

    public Language Lang
    {
        get {
            return appSettings.language == ENGLISH_STR ? Language.EN : Language.DE;
        }
        set {
            appSettings.language = value == Language.EN ? ENGLISH_STR : GERMAN_STR;
        
            StoreSettings();
            UpdateLanguageObjects(value);
        }
    }

    public string LanguageStr
    {
        get {
            return appSettings.language;
        }
    }

    public bool IsEnglish
    {
        get {
            return appSettings.language == ENGLISH_STR;
        }
    }

    public bool IsGerman
    {
        get {
            return appSettings.language == GERMAN_STR;
        }
    }

    public bool Onboarded
    {
        get {
            return appSettings.onboarded;
        }
        set {
            appSettings.onboarded = value;
            StoreSettings();
        }
    }

    public bool AccessibilityEnabled
    {
        get 
        {
            return appSettings.accessible;
        }
        set 
        {
            appSettings.accessible = value;
            accessibilityEnabled = value;
            StoreSettings();

            if (OnAccessibilityToggle != null)
            {
                OnAccessibilityToggle(value);
            }
        }
    }

    public int andreaQuestion
    {
        get
        {
            return appSettings.andreaQuestion;
        }
        set
        {
            appSettings.andreaQuestion = value;
            StoreSettings();
        }
    }

    public int plantsQuestion
    {
        get
        {
            return appSettings.plantsQuestion;
        }
        set
        {
            appSettings.plantsQuestion = value;
            StoreSettings();
        }
    }

    public int calamityQuestion
    {
        get
        {
            return appSettings.calamityQuestion;
        }
        set
        {
            appSettings.calamityQuestion = value;
            StoreSettings();
        }
    }

    public int antiphoneQuestion
    {
        get
        {
            return appSettings.antiphoneQuestion;
        }
        set
        {
            appSettings.antiphoneQuestion = value;
            StoreSettings();
        }
    }

    public bool firstTimeFakeCamera
    {
        get
        {
            return appSettings.firstTimeFakeCamera;
        }
        set
        {
            appSettings.firstTimeFakeCamera = value;
            StoreSettings();
        }
    }

    public bool showAudioTranscript
    {
        get {
            return appSettings.showAudioTranscript;
        }
        set {
            appSettings.showAudioTranscript = value;
            StoreSettings();
        }
    }

    public bool permissionsDone
    {
        get => appSettings.permissionsDone;
        set {
            appSettings.permissionsDone = value;
            StoreSettings();
        }
    }


    public void arcDone(string museum)
    {
        if (!appSettings.arcsDone.Contains(museum))
        {
            appSettings.arcsDone.Add(museum);
            StoreSettings();
        }
    }

    public bool isArcDone(string museum)
    {
        return appSettings.arcsDone.Contains(museum);
    }

    public int arcDoneCount()
    {
        return appSettings.arcsDone.Count;
    }

    //public void UpdateLanguageGO()
    //{
    //    germanGO.AddRange(GameObject.FindGameObjectsWithTag("DE"));
    //    germanGO = germanGO.Distinct().ToList();

    //    englishGO.AddRange(GameObject.FindGameObjectsWithTag("EN"));
    //    englishGO = englishGO.Distinct().ToList();
    //}

    //----------------------------
    //----------------------------
    void Awake()
    {
        if (instance)
        {
            Log.e("GlobalSettings - singleton already set!");
        }
        instance = this;

        if (debugButtons)
        {
            debugButtons.SetActive(_doDebugButtons);
        }

        if (burgerMenu)        
        {
            burgerMenu.SetActive(true);
        }
        else
        {
            Log.e("please set burgermenu");
        }

        // load app settings
        settingsFile = Application.persistentDataPath + "/settings.json";

        if (File.Exists(settingsFile)) 
		{
			appSettings = JsonUtility.FromJson<AppSettings>(File.ReadAllText(settingsFile));
            
            accessibilityEnabled = appSettings.accessible;
		}
        else
        {
            appSettings = new AppSettings();
            appSettings.onboarded = onboardingDone;
            appSettings.accessible = accessibilityEnabled;
            StoreSettings();
        }

        // look through everything to get all gameobjects with tag "EN" or "DE"
        GameObject[] root_gameobjects = gameObject.scene.GetRootGameObjects(); 
        List<Transform> all_text_de = new List<Transform>();
        List<Transform> all_text_en = new List<Transform>();

        foreach (var go in root_gameobjects)
        {
            Transform[] texts = go.transform.GetComponentsInChildren<Transform>(true);
            all_text_de.AddRange(Array.FindAll(texts, item => item.gameObject.tag == "DE"));
            all_text_en.AddRange(Array.FindAll(texts, item => item.gameObject.tag == "EN"));
        }
        foreach (var text in all_text_de)
        {
            germanGO.Add(text.gameObject);
        }
        foreach (var text in all_text_en)
        {
            englishGO.Add(text.gameObject);
        }

        UpdateLanguageObjects(appSettings.language);

        SystemInfoDeviceModel = SystemInfo.deviceModel;


        if (secretDebugSwitch)
        {
            if (debugButtons)
            {
                debugButtons.SetActive(false);
            }
        }
    }

    void Start()
    {
        var hidden = GameObject.FindGameObjectsWithTag("initiallyHidden");
        foreach (var go in hidden)
        {
            go.SetActive(false);
        }

        if (accessibilityEnabled)
        {
            Toggle toggle = burgerMenu.GetComponentInChildren<Toggle>();
            if (toggle)
            {
                toggle.SetIsOnWithoutNotify(accessibilityEnabled);
            }
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // permissionsDone is set once when first going through onboarding
        // once this is done, we can check if bluetooth is on
        if (permissionsDone)
        {
            // initially request bluetooth state
            // this is important for ios
            PermissionManager.CheckBluetoothOn();
        }
    }


    // work in progress
    public void CheckPermissionsBeforeStart()
    {
        if (permissionsDone)
        {
            // check all permissions again!

            // location
            if (!PermissionManager.CheckLocationPermission(true))
            {
                PermissionManager.RequestLocationPermission(true, (bool state) =>
                {
                    Log.d("RequestLocationPermission: " + state);

                    if (!state)
                    {
                        // well... ignore
                        //NativeToolkit.ShowAlert("No Location Permission", "Please allow this app to read your location");
                    }

                    CheckBLE();
                });
            }
            else
            {
                CheckBLE();
            }

            // notification?
            //PermissionManager.CheckNotificationPermission((bool state) =>
            //{
            //    if (!state)
            //    {
            //        PermissionManager.RequestNotificationPermission((bool notistate) =>
            //        {
            //            if (!notistate)
            //            {
            //                // what now? - ignore
            //            }
            //        });
            //    }
            //});

            
        }
        else
        {
            Log.d("StoryController.instance: " + StoryController.instance);
            StoryController.instance.StartStory();
        }
    }

    private void CheckBLE()
    {
        // bluetooth
        PermissionManager.GetBluetoothState((int status) =>
        {
            //Log.d("BLE state: " + status);

            if (status == 3)
            {
                // oh no - ignore
                NativeToolkit.ShowAlert("No Bluetooth Permission", "Please allow this app to use Bluetooth");
            }
            else if (status == 4)
            {
                NativeToolkit.ShowAlert("No Bluetooth", "Please check your Bluetooth.");
            }

            // done - call action
            Log.d("StoryController.instance: " + StoryController.instance);
            StoryController.instance.StartStory();
        });
    }

    public void Reset()
    {
        Lang = Language.DE;
        Onboarded = false;
        AccessibilityEnabled = false;
        andreaQuestion = 1;
        plantsQuestion = 1;
        calamityQuestion = 1;
        antiphoneQuestion = 1;
        firstTimeFakeCamera = false;
        showAudioTranscript = true;

        appSettings.arcsDone = new List<string>();
        StoreSettings();

        // reset bubble text size
        OnAccessibilityToggle?.Invoke(AccessibilityEnabled);
    }

    public void SetEnglish()    
    {
        Lang = Language.EN;
    }

    public void SetGerman()
    {
        Lang = Language.DE;
    }

    private void StoreSettings()
    {
        string json = JsonUtility.ToJson(appSettings, true);
        File.WriteAllText(settingsFile, json);
    }

    public void UpdateLanguageObject()
    {
        UpdateLanguageObjects(Lang);
    }

    private void UpdateLanguageObjects(string lang)
    {
        UpdateLanguageObjects(lang == ENGLISH_STR ? Language.EN : Language.DE);
    }

    private void UpdateLanguageObjects(Language lang)
    {
        // NOTE: this is not very efficient!
        //
        // same gameobject may get set twice
        // we do not do this very often - so it is fine
        //
        // this is necessary because we instantiate onboarding pages dynamically
        // we have to lookup objects with tags everytime we set the language

        var deGO = new List<GameObject>();
        var enGO = new List<GameObject>();

        if (UI)
        {
            Transform[] texts = UI.GetComponentsInChildren<Transform>(true);
            foreach(var t in texts)
            {
                if (t.gameObject.tag == "DE") deGO.Add(t.gameObject);
                else if (t.gameObject.tag == "EN") enGO.Add(t.gameObject);
            }            
        }

        if (ARContainer)
        {
            Transform[] texts = ARContainer.GetComponentsInChildren<Transform>(true);
            foreach (var t in texts)
            {
                if (t.gameObject.tag == "DE") deGO.Add(t.gameObject);
                else if (t.gameObject.tag == "EN") enGO.Add(t.gameObject);
            }
        }

        if (lang == Language.EN)
        {
            foreach (var go in englishGO)
            {
                go.SetActive(true);
            }
            foreach (var go in enGO)
            {
                go.SetActive(true);
            }
            foreach (var go in germanGO)
            {
                go.SetActive(false);
            }
            foreach (var go in deGO)
            {
                go.SetActive(false);
            }
        }
        else
        {
            foreach (var go in germanGO)
            {
                go.SetActive(true);
            }
            foreach (var go in deGO)
            {
                go.SetActive(true);
            }
            foreach (var go in englishGO)
            {   
                go.SetActive(false);
            }
            foreach (var go in enGO)
            {
                go.SetActive(false);
            }
        }
    }

    public int GetAndreaQuestion()
    {
        return appSettings.andreaQuestion;
    }
    public int GetPlantsQuestion()
    {
        return appSettings.andreaQuestion;
    }
    public int GetCalamityQuestion()
    {
        return appSettings.andreaQuestion;
    }
    public int GetAntiphoneQuestion()
    {
        return appSettings.andreaQuestion;
    }

    // hidden buttons
    string hiddenId = "";
    string hiddenKey = "0010";
    public void HiddenButton(int id)
    {
        hiddenId += id;
        if (hiddenId.Length > hiddenKey.Length)
        {
            hiddenId = hiddenId.Substring((hiddenId.Length - hiddenKey.Length), hiddenKey.Length);
        }

        if (hiddenId == hiddenKey)
        {
            if (debugButtons)
            {
                debugButtons.SetActive(true);                
            }
            if (secretDebugSwitch)
            {
                secretDebugSwitch.SetActive(false);
            }
            hiddenId = "";
        }
    }


    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

}
