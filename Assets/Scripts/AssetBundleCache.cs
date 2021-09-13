using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
 

 // BEFORE RELEASE: 
 //
 //         bool ignoreCacheScript allows the game to start without downloading asset bundles for dev. purposes     
 //
 //         generate the appropriate bundles for the task as follows:
 //         WARNING: asset bundles are not case sensitive. Use all lowercase or it will raise errors when building the paths.
 //         WARNING: asset bundles are files with no extension
 //
 //         for each target build, set the editor to the desired build target
 //         for each asset to be bundled, in the bottom line of the inspector in the field "AssetBundle"
 //             choose the language to be bundled in lower case
 //             in the menu click on Assets/Build AssetBundles
 //                 we will get the assetbundles in the Assets/AssetBundles project in the hierarchy. 
 //                 We only need the files without extension, including the manifest asset bundle(will be called "assetbundles", "ios" or "android")
 //
 //         
 //             Folder Structure in the server: manifestPath    = serverPath + "/" + buildTarget + "/" + targetBuild;
 //                                             assetBundlePath = serverPath + "/" + buildTarget + "/" + language.ToString();
 //
 //                 ios/english         => AssetBundle built on Unity Editor with TargetBuild set to iOS with all the English Audio assets
 //                 ios/deutsch         =>  =       =       =       =       =       =       =       =       =         German Audio assets
 //                 ios/AssetBundles             => Contains a manifest of all asset bundles references and dependencies.
 //
 //                 android/english     => same for Android
 //                 android/deutsch
 //                 android/AssetBundles
 //
 // TODOS:
 //         Error cathching if download fails => what happens if it doesnt work??
 //         go back to same page until it works (?)
 //         
 //         clear cache clears all cache paths, standard and custom (?)
 //         REFRACTOR CLEAR CACHE

public class AssetBundleCache : MonoBehaviour
{
    [SerializeField] Toggle germanVersion;
    [SerializeField] Toggle englishVersion;
    [SerializeField] Button nextButton;
    [SerializeField] Image maskImage;
    [SerializeField] bool clearCacheAtStart = false; // make sure we clear cache. 
    [SerializeField] bool useCustomCache = false; // custom cache is now pointing to a folder inside the standard cache.

    [SerializeField] bool ignoreCacheScript = false;

    [SerializeField] PageController pageController;

    /////////////////////////////////////////////////////////////////////////////

    // VARIABLES TO STORE ASSET BUNDLE ADRESSES
    private string manifestPath; // No file extension (this file gets created when you build your bundles)
    // private string manifestPath = "http://ingorandolf.info/test/assetBundle"; // No file extension (this file gets created when you build your bundles)
                                                                                 // this file would normally be called iOS or Android. It is an asset bundle that contains
                                                                                 // a manifest file with all the asset bundles of the project and crossed dependencies amongst bundles.
    private string assetBundlePath; //= "http://ingorandolf.info/test/ab1"; // Also no extension. This is the actual content asset bundle

    private string serverPath = "https://ingorandolf.info/test"; //landing page of the asset bundle
    
    /////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////
    //
    // this block contains the building blocks for the asset path
    //
    private enum LanguageCases {english, deutsch};
    private enum BuildTarget {ios, android};
    private LanguageCases language;
    private BuildTarget buildTarget;
    //
    /////////////////////////////////////////////////////////////////////////////

    public AssetBundle assetBundle = default;
    private string onBoardingReadyText = "Go to onboarding"; // text to be put in the button after download is complete

    private void Start()
    {

        //for debug purposes, we can skip downloading the assets and use the app as normal calling the files normally from the project folder structure
        if(ignoreCacheScript)
        {
            pageController.enabled = true;
            return;
        }
        else
        {
            pageController.enabled = false;
        }

#if UNITY_IOS
        buildTarget = BuildTarget.ios;
#endif

#if UNITY_ANDROID
        buildTarget = BuildTarget.android;
#endif

// #if UNITY_EDITOR
//         buildTarget = BuildTarget.android; // set default to android 
// #endif

        manifestPath = serverPath + "/" + buildTarget.ToString() + "/AssetBundles"; 
        
        Debug.Log("auto manifest path: " + manifestPath);

        manifestPath = "https://ingorandolf.info/test/ios/AssetBundles";

        Debug.Log("CurrentManifestPath: " + manifestPath);

        if (nextButton)
        {
            nextButton.onClick.AddListener(StartDownloadCoroutine);      
        } 
    }

    private void SetCache() 
    {
        // as discussed with ingo, they may be a global variable stored to clear cache globally
        string customCache = Path.Combine(Application.persistentDataPath,language.ToString());

        if(clearCacheAtStart) 
        {
            // make sure we delete both default and custom cache
            Caching.currentCacheForWriting = Caching.defaultCache;
            Caching.ClearCache();

            if(Directory.Exists(customCache))
            {
                Cache newCache = Caching.AddCache(customCache);
                Caching.ClearCache();
            }

        }
        
        if(!useCustomCache)
        {
            Caching.currentCacheForWriting = Caching.defaultCache;
        }
        else 
        {
            if(!Directory.Exists(customCache)) 
                Directory.CreateDirectory(customCache);

            Cache newCache = Caching.AddCache(customCache);
 
            if(newCache.valid)
            {
                Caching.currentCacheForWriting = newCache;
            }
        }
    }

    private void StartDownloadCoroutine()
    {
        if(germanVersion.isOn)
        {
            // TODO unify those enums
            GlobalSettings.instance.Lang = GlobalSettings.Language.DE;
            language = LanguageCases.deutsch; //.German TODO: CHECK BEFORE PUSHING

            // turn off english versions
            GameObject[] gos = GameObject.FindGameObjectsWithTag("EN");
            foreach (var go in gos)
            {   
                go.SetActive(false);
            }
        }
        else if(englishVersion.isOn)
        {
            GlobalSettings.instance.Lang = GlobalSettings.Language.EN;
            language = LanguageCases.english;

            GameObject[] gos = GameObject.FindGameObjectsWithTag("DE");
            foreach (var go in gos)
            {
                go.SetActive(false);
            }
        }

        SetCache(); // warning there is no way of looking for all old caches and clearing them all at once
        
        nextButton.onClick.RemoveListener(StartDownloadCoroutine);

        StartCoroutine(DownloadAndCacheAssetBundle(manifestPath, language));
    }

    IEnumerator DownloadProgress(UnityWebRequestAsyncOperation operation)
    {
        // get (float)progress from download to update progress bar
        while (!operation.isDone)
        {
                // DEBUG: print on screen
                // float downloadDataProgress = operation.progress * 100;
                // Debug.Log("Download: " + downloadDataProgress);
            maskImage.fillAmount = operation.progress;
            yield return null;
        }

        maskImage.fillAmount = 1f;
        //Debug.Log("Done");
        
        yield return null;
    }

    IEnumerator DownloadAndCacheAssetBundle(string manif, LanguageCases language)
    {
        AssetBundle manifestBundle;

        Debug.Log(manif);

        // Load the manifest (from url)
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(manif);
        yield return www.SendWebRequest();
            // DEBUG: CHECK WE ARE ACTUALLY DOWNLOADING THE "MANIFEST" ASSET BUNDLE
            // UnityWebRequestAsyncOperation op = www.SendWebRequest();
            // yield return DownloadProgress(op);
        
        manifestBundle = DownloadHandlerAssetBundle.GetContent(www);

        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest"); // manifest of the asset bundles in the project.
 
        string[] allAssetBundleNames = manifest.GetAllAssetBundles();

        string assetBundleName = language.ToString();

        // Get hash of bundle
        Hash128 hash = manifest.GetAssetBundleHash(assetBundleName);
        //Debug.Log("Hash for bundle " + assetBundleName + " is " + hash);

        assetBundlePath = serverPath + "/" + buildTarget.ToString() + "/" + language.ToString(); 

        // Download actual content bundle
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundlePath, hash, 0);
        
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        yield return DownloadProgress(operation); // to visualize, states the fill amount of the button.

        Debug.Log(request.responseCode); // Code 200 means downloaded from web and Code 0 means loaded from cache.

        assetBundle = DownloadHandlerAssetBundle.GetContent(request);

        nextButton.GetComponentInChildren<Text>().text = onBoardingReadyText; // set the button TEXT ready to start onboarding

        nextButton.onClick.AddListener(MainPageController.Next); // add listener to advance pageeee 
    }

    private void OnDestroy() 
    {
        // I think this is redundant but just in case
        if (assetBundle != null)
        {            
            assetBundle.Unload(false); // Don't forget to unload the bundle
        }
    }
 
}