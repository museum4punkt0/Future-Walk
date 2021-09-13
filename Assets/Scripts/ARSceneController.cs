using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;

public class ARSceneController : MonoBehaviour, IARSuccessProvider
{
    public enum ARType
    {
        None, ARImageGeorg, ARImageBouquet, ARImageVermeer, ARWurlitzer, ARTrautonium, ARCembalo, FakeCamera
    }

    private class DefaultARFalse : IARSuccessProvider
    {
        public bool Success => false;
    }

    public const string AR_SCENE_GEORGE = "georg";
    public const string AR_SCENE_VERMEER = "vermeer";
    public const string AR_SCENE_BOUQUET = "bouquet";

    //--------------------------------------------
    //--------------------------------------------
    public static ARSceneController instance;

    //--------------------------------------------
    //--------------------------------------------
    [Header("AR")]
    [SerializeField] GameObject arPrefab;

    [Header("Scene Prefabs")]
    [SerializeField] GameObject sceneGeorg;
    [SerializeField] GameObject sceneBouquet;
    [SerializeField] GameObject sceneVermeer;
    [SerializeField] GameObject sceneWurlitzer;
    [SerializeField] GameObject sceneTrautonium;
    [SerializeField] GameObject sceneCembalo;


    GameObject arGO;
    GameObject ARScene;
    GameObject ARCamera;

    GameObject fakeCameraCanvas;
    IARSuccessProvider fakeCameraSuccess;
    IARSuccessProvider curretARScene = new DefaultARFalse();

    // IARSuccessProvider
    public bool Success => curretARScene != null && curretARScene.Success;

    //--------------------------------------------
    //--------------------------------------------
    void Awake()
    {
        if (instance)
        {
            Log.e("ARSceneController singleton already set!");
        }
        instance = this;

        if (!arPrefab)
        {
            Log.e("no AR Prefab set!");
        }
    }

    void InitAR()
    {
        Log.d("InitAR");
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Log.e("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Log.d("!!! Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }


        // create ar
        arGO = Instantiate(arPrefab, transform);
        if (arGO)
        {
            ARCamera = arGO.GetComponentInChildren<Camera>(true).gameObject;            
            
            fakeCameraCanvas = arGO.GetComponentInChildren<FakeCamera>(true).gameObject;
            if (fakeCameraCanvas)
            {
                fakeCameraSuccess = fakeCameraCanvas.GetComponent<IARSuccessProvider>();
            }

            arGO.GetComponentInChildren<Button>(true).onClick.AddListener(() => {
                StoryChat.instance.CloseARWithCheck();
            });
        }
    }

    public void DestroyAR()
    {
        if (ARScene)
        {
            Destroy(ARScene);
            ARScene = default;
        }

        if (fakeCameraCanvas)
        {
            fakeCameraCanvas.SetActive(false);
            fakeCameraCanvas = default;
        }

        if (arGO)
        {
            Destroy(arGO);
            arGO = default;
        }

        ARCamera = default;
        fakeCameraCanvas = default;
        fakeCameraSuccess = default;
        curretARScene = new DefaultARFalse();

        StartCoroutine(FrameDelayAction(3, () => {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            Log.d("XR stopped completely.");
        }));
    }

    private IEnumerator FrameDelayAction(int frames, Action action = null)
    {
        if (action != null)
        {
            for (int i=0; i<frames; i++)
            {
                yield return null;
            }

            action();
        }
    }

    private void CreateARScene(GameObject prefab)
    {
        if (arGO)
        {
            ARScene = Instantiate(prefab, arGO.transform);
            if (ARScene)
            {
                var activationController = ARScene.GetComponent<ARActivationController>();
                if (activationController)
                {
                    activationController.Init(ARCamera);
                    curretARScene = activationController;
                }
                else
                {
                    // oh no!
                }
            }
        }
    }

    // scenes without indicator
    private void SetFakeCamera()
    {
        curretARScene = fakeCameraSuccess;

        if (fakeCameraCanvas)
        {
            fakeCameraCanvas.SetActive(true);
        }
    }

    public void ShowType(ARType type)
    {
        if (type == ARType.None) return;

        InitAR();

        // we should have arGO now - check
        if (!arGO)
        {
            StoryChat.instance._AR_close();
            return;
        }

        switch (type)
        {
            case ARType.ARImageGeorg:   CreateARScene(sceneGeorg); break;
            case ARType.ARImageBouquet: CreateARScene(sceneBouquet); break;
            case ARType.ARImageVermeer: CreateARScene(sceneVermeer); break;
            case ARType.ARWurlitzer:    CreateARScene(sceneWurlitzer); break;
            case ARType.ARTrautonium:   CreateARScene(sceneTrautonium); break;
            case ARType.ARCembalo:      CreateARScene(sceneCembalo); break;

            case ARType.FakeCamera:     SetFakeCamera(); break;

            default:
                Log.d("no such type: " + type);
                return;
        }

        // update language setting on newly created objects
        GlobalSettings.instance.UpdateLanguageObject();

        arGO.SetActive(true);
        MainPageController.instance.mainCanvasOut();
    }
}
