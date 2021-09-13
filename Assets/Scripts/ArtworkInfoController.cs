using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;


[System.Serializable]
public class ArtInfo
{
    public string id;
    public string title_en;
    public string title_de;
    public string author_en;
    public string author_de;
    public string year;
    public string content_en;
    public string content_de;
    public string footer_en;
    public string footer_de;
}

[System.Serializable]
public class ArtInfos
{
    //employees is case sensitive and must match the string "employees" in the JSON.
    public ArtInfo[] artworks;
}

public class ArtworkInfoController : MonoBehaviour
{
    //--------------------------------------------
    //--------------------------------------------
    public static ArtworkInfoController instance;


    //--------------------------------------------
    //--------------------------------------------
    [SerializeField] Image headerImage;
    [SerializeField] Text title;
    [SerializeField] Text author;
    [SerializeField] Text content;
    [SerializeField] Text footerText;
    [SerializeField] TextAsset jsonFile;
    [SerializeField] PageMover pageMover;
    [SerializeField] ScrollRect artworkScroller;


    private Dictionary<string, ArtInfo> artDict = new Dictionary<string, ArtInfo>();

    private AsyncOperationHandle<Sprite> _currentOperationHandle;

    public void Awake()
    {
        instance = this;

        // read json
        // JsonUtility.FromJson<List<ArtinfoItem>>()
        ArtInfos art_infos = JsonUtility.FromJson<ArtInfos>(jsonFile.text);

        foreach (ArtInfo info in art_infos.artworks)
        {
            artDict.Add(info.id, info);
        }
    }

    public bool Contains(string id)
    {
        return artDict.ContainsKey(id);
    }

    public string GetArtworkTitle(string id)
    {
        if (artDict.ContainsKey(id))
        {
            if (GlobalSettings.instance.IsEnglish)
            {
                return artDict[id].title_en;
            }
                        
            return artDict[id].title_de;
        }

        return "";
    }

    public string GetArtworkAuthorAndYear(string id)
    {
        if (artDict.ContainsKey(id))
        {
            if (GlobalSettings.instance.IsEnglish)
            {
                return artDict[id].author_en + "\n" + artDict[id].year;
            }
            
            return artDict[id].author_de + "\n" + artDict[id].year;            
        }

        return "";
    }

    public void ReleaseSprite()
    {
        if (_currentOperationHandle.IsValid())
        {
            Addressables.Release(_currentOperationHandle);
            _currentOperationHandle = default;
        }
    }

    public void LoadArtwork(string id)
    {
        if (pageMover && headerImage && title && author && content && footerText)
        {
            if (artDict.ContainsKey(id))
            {
                ReleaseSprite();

                var currentOperationHandle = Addressables.LoadAssetAsync<Sprite>(id);
                currentOperationHandle.Completed += (AsyncOperationHandle<Sprite> handle) =>
                {
                    _currentOperationHandle = handle;

                    if (handle.IsValid())
                    {
                        SetContent(id);
                    }
                    else
                    {
                        Log.d("could not load: " + id);
                    }
                };
            }
        }
    }


    private void SetContent(string id)
    {
        if (_currentOperationHandle.IsValid())
        {
            headerImage.sprite = _currentOperationHandle.Result;
        }
        else
        {
            Log.d("Artwork: try to load from Resources: " + id);
            headerImage.sprite = Tools.Load<Sprite>(id);
        }

        if (!headerImage.sprite)
        {
            Log.d("Artwork: could not load: " + id);
            return;
        }


        // all ok

        AspectRatioFitter fitter = headerImage.GetComponent<AspectRatioFitter>();
        if (fitter)
        {
            fitter.aspectRatio = headerImage.sprite.rect.size.x / headerImage.sprite.rect.size.y;
        }

        if (GlobalSettings.instance.IsEnglish)
        {
            title.text = artDict[id].title_en;
            author.text = artDict[id].author_en + (string.IsNullOrWhiteSpace(artDict[id].year) ? "" : (" (" + artDict[id].year + ")"));
            footerText.text = artDict[id].footer_en;
            content.text = artDict[id].content_en;
        }
        else
        {
            title.text = artDict[id].title_de;
            author.text = artDict[id].author_de + (string.IsNullOrWhiteSpace(artDict[id].year) ? "" : (" (" + artDict[id].year + ")"));
            footerText.text = artDict[id].footer_de;
            content.text = artDict[id].content_de;
        }

        // scroll up
        if (artworkScroller)
        {
            Canvas.ForceUpdateCanvases();
            // scroll to top of scroller
            artworkScroller.content.localPosition = new Vector2(0, 0);
        }

        pageMover.moveOut();
    }

}

