using System;
using System.Collections;
using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class MainPageController : MonoBehaviour
{
    //----------------------------------------
    //----------------------------------------
    public static MainPageController instance;

    public static void Next()
    {
        if (instance)
        {
            instance._Next();
        }
    }


    //----------------------------------------
    //----------------------------------------
    [Header("Game Elements")]
    [SerializeField] Canvas mainCanvas;
    [SerializeField] GameObject UIGo;
    [SerializeField] GameObject ARContainer;
    [SerializeField] Canvas canvas; // this contains our moving pages
    [SerializeField] GameObject footer;
    [SerializeField] Sprite sprite;
    [SerializeField] GameObject imageContainer;
    [SerializeField] float animationTime = 0.5f;

    RectTransform mainCanvasRectTransform;
    RectTransform canvasRectTransform;

    GameObject page1Instance;
    GameObject page2Instance;
    GameObject page3Instance;
    GameObject page4Instance;
    GameObject page5Instance;

    int currentPage = 0;
    bool acceptNext = true;
    Color bulletBaseColor = Style.bulletBase;

    void Awake()
    {
        if (instance)
        {
            Log.e("MainPageController singleton already set!");
        }
        instance = this;

        if (!ARContainer)
        {
            Log.e("ARContainer not set!");
        }
    }

    void Start()
    {
        // check if we have chat-bubbles
        //if (!string.IsNullOrEmpty(StoryChat.instance.LastSavedScene()))
        //{
        //    if (imageContainer) imageContainer.SetActive(false);
        //}


        // either create onboarding pages or
        // START THE STORY

        if (!GlobalSettings.instance.Onboarded)
        {
            CreateOnboardingPages();
        }

        if (mainCanvas)
        {
            mainCanvasRectTransform = mainCanvas.GetComponent<RectTransform>();
        }

        if (canvas)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();

            if (!GlobalSettings.instance.Onboarded
                && footer)
            {
                // this does not work propery
                // how would we get the correct (scaled) size?
                // keep it for now to fill it up - but eventually remove this
                for (int i = footer.transform.childCount; i < canvas.transform.childCount; i++)
                {
                    GameObject footerObj = new GameObject(); //Create the GameObject
                    RectTransform rectT = footerObj.AddComponent<RectTransform>();

                    rectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 8);
                    rectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 8);

                    Image img = footerObj.AddComponent<Image>(); //Add the Image Component script
                    img.sprite = sprite; //Set the Sprite of the Image Component on the new GameObject
                    img.color = bulletBaseColor;
                    img.fillAmount = 1;
                    footerObj.GetComponent<RectTransform>().SetParent(footer.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
                    footerObj.SetActive(true); //Activate the GameObject
                }

                // set inital color
                SetInitialFooterColor();
                UpdateIndex();
            }
            else if (footer)
            {
                // 
                footer.SetActive(false);
            }
        }
    }

    private GameObject CreatAsFirstSibling(string name)
    {        
        GameObject prefab = Resources.Load<GameObject>(name);
        if (prefab)
        {
            GameObject obj = CreatAsFirstSibling(prefab);
            return obj;
        }
        return null;
    }

    private GameObject CreatAsFirstSibling(GameObject prefab)
    {
        var go = Instantiate(prefab, canvas.transform);
        go.transform.SetAsFirstSibling();

        return go;
    }

    private void DestroyOnboardingPages()
    {
        if (page5Instance) Destroy(page5Instance);
        if (page4Instance) Destroy(page4Instance);
        if (page3Instance) Destroy(page3Instance);
        if (page2Instance) Destroy(page2Instance);
        if (page1Instance) Destroy(page1Instance);

        page5Instance = default;
        page4Instance = default;
        page3Instance = default;
        page2Instance = default;
        page1Instance = default;

        canvasRectTransform.localPosition = new Vector3(0, 0, 0);
        Resources.UnloadUnusedAssets();
    }

    private void CreateOnboardingPages()
    {
        // create onbaording pages
        if (canvas)
        {            
            page5Instance = CreatAsFirstSibling("BeSafePage");
            page4Instance = CreatAsFirstSibling("HeadphonesPage");
            page3Instance = CreatAsFirstSibling("TicketPage");
            page2Instance = CreatAsFirstSibling("OnSitePage");
            page1Instance = CreatAsFirstSibling("LanguagePage");
        }
    }

    private void SetInitialFooterColor()
    {
        if (footer)
        {
            foreach (Transform child in footer.transform)
            {
                Image img = child.gameObject.GetComponent<Image>();
                if (img)
                {
                    img.color = bulletBaseColor;
                }
            }
        }
    }


    public void Reset()
    {
        currentPage = 0;
        acceptNext = true;

        CreateOnboardingPages();

        if (canvasRectTransform) canvasRectTransform.localPosition = new Vector3(0, 0, 0);
        if (footer) footer.gameObject.SetActive(true);
        if (imageContainer) imageContainer.SetActive(true);

        SetInitialFooterColor();
        UpdateIndex();

        // for language reset
        GlobalSettings.instance.UpdateLanguageObject();
    }


    public void MainCanvasSetOffscreen()
    {
        mainCanvasRectTransform.anchoredPosition = new Vector2(-Screen.width, mainCanvasRectTransform.anchoredPosition.y);
    }

    public void MainCanvasSetOnscreen()
    {
        mainCanvasRectTransform.anchoredPosition = new Vector2(0, mainCanvasRectTransform.anchoredPosition.y);
    }


    public void mainCanvasOut()
    {
        if (mainCanvasRectTransform)
        {
            Action<ITween<Vector2>> pageTween = (t) =>
            {
                mainCanvasRectTransform.anchoredPosition = t.CurrentValue;
            };

            Action<ITween<Vector2>> pageTweenCompleted = (t) =>
            { 
                if (UIGo)
                {
                    UIGo.SetActive(false);
                }
            };

            mainCanvas.gameObject.Tween("MainPageMove",
                                        mainCanvasRectTransform.anchoredPosition,
                                        new Vector2(-Screen.width, mainCanvasRectTransform.anchoredPosition.y),
                                        0.8f,
                                        TweenScaleFunctions.CubicEaseInOut,
                                        pageTween,
                                        pageTweenCompleted);
        }
    }

    public void mainCanvasIn(Action finishAction = null)
    {
        if (mainCanvasRectTransform)
        {
            Action<ITween<Vector2>> pageTween = (t) =>
            {
                mainCanvasRectTransform.anchoredPosition = t.CurrentValue;
            };

            Action<ITween<Vector2>> pageTweenCompleted = (t) =>
            {
                ARSceneController.instance.DestroyAR();

                finishAction?.Invoke();
            };

            if (UIGo)
            {
                UIGo.SetActive(true);
            }

            mainCanvas.gameObject.Tween("MainPageMove",
                                        mainCanvasRectTransform.anchoredPosition,
                                        new Vector2(0, mainCanvasRectTransform.anchoredPosition.y),
                                        0.8f,
                                        TweenScaleFunctions.CubicEaseInOut,
                                        pageTween,
                                        pageTweenCompleted);
        }
    }

    public void FadeoutImages()
    {
        if (!imageContainer) return;
        if (!imageContainer.activeSelf) return;

        Action<ITween<float>> pageTween = (t) =>
        {
            foreach(Image image in imageContainer.GetComponentsInChildren<Image>())
            {
                Color c = image.color;
                c.a = t.CurrentValue;
                image.color = c;
            }
        };

        mainCanvas.gameObject.Tween("fadeout",
                                    1f,
                                    0f,
                                    1.2f,
                                    TweenScaleFunctions.CubicEaseInOut,
                                    pageTween,
                                    (t) => {
                                        imageContainer.SetActive(false);
                                    });
    }

    // sliding pages

    void UpdateIndex()
    {
        if (footer)
        {
            Image[] images = footer.GetComponentsInChildren<Image>(true);

            if (currentPage > 0 && images.Length > currentPage)
            {
                images[currentPage-1].color = bulletBaseColor;
            }

            if (currentPage < (canvas.transform.childCount-1) && images.Length > (currentPage+1))
            {
                images[currentPage+1].color = bulletBaseColor;
            }

            if (images.Length > currentPage)
            {
                images[currentPage].color = Style.bulletHighlight;
            }
        }

        if (currentPage < (canvas.transform.childCount-1))
        {
            acceptNext = true;
        }
        else
        {
            if (footer) footer.gameObject.SetActive(false);
            GlobalSettings.instance.Onboarded = true;

            DestroyOnboardingPages();

            DoStartStory();
        }
    }


    public void DoStartStory()
    {
        if (GlobalSettings.instance.Onboarded)
        {
            StartCoroutine(StartStoryNow());
        }
    }

    IEnumerator StartStoryNow()
    {
        yield return new WaitForSeconds(0.1F);

        FadeoutImages();    
        StoryController.instance.StartStory();
    }
    
    private void DoMove()
    {
        Action<ITween<Vector3>> pageTween = (t) =>
        {
            canvasRectTransform.localPosition = t.CurrentValue;
        };

        Action<ITween<Vector3>> pageTweenCompleted = (t) =>
        {
            UpdateIndex();
        };

        canvas.gameObject.Tween("PageMove", 
                                canvasRectTransform.localPosition, 
                                new Vector3(currentPage * -375, 0, 0), 
                                animationTime, 
                                TweenScaleFunctions.CubicEaseInOut, 
                                pageTween, 
                                pageTweenCompleted);
    }

    private void _Next()
    {
        if (!acceptNext) return;

        currentPage++;
        acceptNext = false;
        DoMove();
    }

    public void Prev()
    {
        if (currentPage <= 0) return;

        currentPage--;
        acceptNext = false;
        DoMove();
    }

}
