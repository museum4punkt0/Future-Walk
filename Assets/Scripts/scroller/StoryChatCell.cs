
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

class StoryChatCell : MonoBehaviour, ICanvasElement, IContentLoader<ChatEntry>
{
    [SerializeField] GameObject heightProvider = default;

    string content = "";
    public string Content {
        get => content;
    }

    bool _layoutDone = false;
    public bool LayoutDone {
        get => _layoutDone;
    }

    bool _graphicDone = false;
    public bool GraphicDone {
        get => _graphicDone;
    }

    private AsyncOperationHandle<Sprite> _currentOperationHandle = default;

    private void OnDestroy()
    {
        ReleaseSprite();
    }


    //

    public void UpdatePosition(float position)
    {
        UpdatePositionXY(GetComponent<RectTransform>().anchoredPosition.x, position);
    }

    public void UpdatePositionXY(float x, float y)
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    public float Height()
    {
        if (heightProvider)
        {
            return heightProvider.GetComponent<RectTransform>().rect.height;
        }

        return 0;
    }


    private void SpriteCleanup(GameObject obj)
    {
        var images = obj.GetComponentsInChildren<Image>();
        foreach (var item in images)
        {
            if (item.name == "contentImage"
                || item.name == "ArtworkImage")
            {	
                if (item.sprite)
                {
                    //Resources.UnloadAsset(item.sprite);
                    item.sprite = null;
                }
            }
        }
    }

    public void ReleaseSprite()
    {
        if (_currentOperationHandle.IsValid())
        {
            Addressables.Release(_currentOperationHandle);
            _currentOperationHandle = default;
        }
    }


    public bool ForceRelayout()
    {
        _layoutDone = false;
        _graphicDone = false;

        if (!CanvasUpdateRegistry.TryRegisterCanvasElementForLayoutRebuild(this))
        {
            Log.d("could not register cell for layout rebuild");
            return false;
        }

        if (!CanvasUpdateRegistry.TryRegisterCanvasElementForGraphicRebuild(this))
        {
            Log.d("could not register for graphics rebuild");
            return false;
        }

        LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        return true;
    }

    //------------------------
    // IContentLoader
    //------------------------

    public IEnumerator LoadContent(ChatEntry chatEntry)
    {
        ReleaseSprite();

        if (chatEntry.type == ChatEntry.ChatEntryType.Image
            || chatEntry.type == ChatEntry.ChatEntryType.Map
            || chatEntry.type == ChatEntry.ChatEntryType.Artwork)
        {
            // load addressable
            var currentOperationHandle = Addressables.LoadAssetAsync<Sprite>(Tools.removeSuffix(chatEntry.content));

            // wait for it
            yield return currentOperationHandle;

            // set it
            _currentOperationHandle = currentOperationHandle;

            if (!currentOperationHandle.IsValid())
            {
                Log.d("COULD NOT LOAD: " + chatEntry.content);
            }
        }
    }

    public void LoadContentAsync(ChatEntry chatEntry, Action<bool> action)
    {
        ReleaseSprite();

        if (chatEntry.type == ChatEntry.ChatEntryType.Image
            || chatEntry.type == ChatEntry.ChatEntryType.Map
            || chatEntry.type == ChatEntry.ChatEntryType.Artwork)
        {
            // load addressable
            var currentOperationHandle = Addressables.LoadAssetAsync<Sprite>(Tools.removeSuffix(chatEntry.content));

            currentOperationHandle.Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                _currentOperationHandle = handle;

                if (action != null)
                {
                    action(handle.IsValid());
                }
            };
        }
        else if (action != null)
        {
            action(true);
        }
    }

    public void UpdateContent(ChatEntry chatEntry)
    {
        if (!heightProvider)
        {
            return;
        }

        // set content
        content = chatEntry.content;

        if (chatEntry.type == ChatEntry.ChatEntryType.Image 
            || chatEntry.type == ChatEntry.ChatEntryType.Map)
        {
            var images = heightProvider.GetComponentsInChildren<Image>(true);

            Image content_img = null;
            Image mask_img = null;
            foreach (var item in images)
            {
                if (item.name == "contentImage")
                {	
                    content_img = item;
                }
                else if (item.name == "maskImage")
                {	
                    mask_img = item;
                }				
            }

            if (content_img && mask_img)
            {
                if (_currentOperationHandle.IsValid())
                {
                    content_img.sprite = _currentOperationHandle.Result;
                }
                else
                {
                    Log.d("try to load sprite from resources!");
                    content_img.sprite = Tools.Load<Sprite>(Tools.removeSuffix(chatEntry.content));
                }

                var btn = content_img.GetComponent<Button>();
                if (btn)
                {
                    btn.onClick.AddListener(() => 
                    {
                        StoryChat.instance.showFullscreen(chatEntry.content);
                    });
                }

                // set mask height
                if (content_img.sprite)
                {
                    // NOTE: should we set height to something in realtion to scroller height?
                    var trans = heightProvider.GetComponent<RectTransform>();
                    var w = (content_img.sprite.rect.width * trans.rect.height) / content_img.sprite.rect.height;

                    if (w > trans.rect.width)
                    {
                        // constrain width to original rect.width
                        var h = (content_img.sprite.rect.height * trans.rect.width) / content_img.sprite.rect.width;
                        trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
                    }
                    else
                    {
                        trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                    }

                }
            }
        }
        else if (chatEntry.type == ChatEntry.ChatEntryType.Artwork)
        {
            // set image and content
            var texts = heightProvider.GetComponentsInChildren<Text>();
            var images = heightProvider.GetComponentsInChildren<Image>();

            Image artwork_image = null;
            Text title_text = null;
            Text subtitle_text = null;

            foreach (var item in texts)
            {
                if (item.name == "Title")
                {	
                    title_text = item;
                }
                else if (item.name == "SubTitle")
                {	
                    subtitle_text = item;
                }				
            }

            foreach (var item in images)
            {
                if (item.name == "ArtworkImage")
                {	
                    artwork_image = item;
                }
            }

            string title = ArtworkInfoController.instance.GetArtworkTitle(chatEntry.content);
            string authyear = ArtworkInfoController.instance.GetArtworkAuthorAndYear(chatEntry.content);

            if (artwork_image)
            {
                if (_currentOperationHandle.IsValid())
                {
                    artwork_image.sprite = _currentOperationHandle.Result;
                }
                else
                {
                    Log.d("try to load sprite from resources!");
                    artwork_image.sprite = Tools.Load<Sprite>(Tools.removeSuffix(chatEntry.content));
                }

                if (artwork_image.sprite)
                {
                    AspectRatioFitter fitter = artwork_image.GetComponent<AspectRatioFitter>();
                    if (fitter)
                    {
                        fitter.aspectRatio = artwork_image.sprite.rect.size.x / artwork_image.sprite.rect.size.y;
                    }
                }
            }

            if (title_text)
            {
                title_text.text = title;
            }

            if (subtitle_text)
            {
                subtitle_text.text = authyear;
            }

            Button btn = heightProvider.GetComponentInChildren<Button>();
            if (btn)
            {
                btn.onClick.AddListener(() =>
                {
                    ArtworkInfoController.instance.LoadArtwork(chatEntry.content);
                });
            }
        }
        else
        {
            var text = heightProvider.GetComponentInChildren<Text>();
            if (text)
            {
                text.text = content;
            }
        }
    }


    //--------------------
    // ICanvasElement
    //--------------------

    public void Rebuild(CanvasUpdate executing)
    {
    }

    public void LayoutComplete()
    {
        _layoutDone = true;
    }

    public void GraphicUpdateComplete()
    {
        _graphicDone = true;
    }

    public bool IsDestroyed()
    {
        return false;
    }
}
