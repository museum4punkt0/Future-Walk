using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ImageOverlay : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] DigitalRubyShared.GestureInterface gestController;
    [SerializeField] GameObject mainCameraToDisable;

    private AsyncOperationHandle<Sprite> _currentOperationHandle;

    private void OnEnable()
    {
        if (mainCameraToDisable)
        {
            mainCameraToDisable.SetActive(false);
        }

        MainPageController.instance.MainCanvasSetOffscreen();

        if (gestController)
        {
            gestController.gameObject.SetActive(true);    
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

    public void Show(string filename)
    {        
        var currentOperationHandle = Addressables.LoadAssetAsync<Sprite>(Tools.removeSuffix(filename));

        currentOperationHandle.Completed += (AsyncOperationHandle<Sprite> handle) =>
        {
            _currentOperationHandle = handle;

            if (handle.IsValid())
            {
                gameObject.SetActive(true);

                if (renderer)
                {
                    renderer.sprite = handle.Result;
                }

                if (gestController)
                {
                    gestController.gameObject.SetActive(true); 
                    gestController.Reset();   
                }
            }
            else
            {
                Log.d("ImageOverlay: could not load: " + filename);
            }
        };
    }

    public void Hide()
    {
        if (mainCameraToDisable)
        {
            mainCameraToDisable.SetActive(true);
        }

        MainPageController.instance.MainCanvasSetOnscreen();

        gameObject.SetActive(false);

        if (gestController)
        {
            gestController.gameObject.SetActive(false);
        }

        if (renderer)
        {
            renderer.sprite = null;
        }

        ReleaseSprite();
    }
}
