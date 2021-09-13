using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
//using UnityEngine.XR;


public class PanZoom1 : MonoBehaviour
{
    Vector3 touchStart = new Vector2(0f, 0f);
    Vector2 pinchCenter = new Vector2(0.0f, 0f);
    Vector2 screenSize = new Vector2(0f, 0f);
    Vector2 imageSize = new Vector2(0f,0f);
    Vector2 rectSize = new Vector2(0f, 0f);

    float screenAspectRatio, imageAspectRatio;
    float dx, dy;
    float canvasScale;

    bool inited = false;
    bool ScaleBlockPan = false;

    [Header("Zoom Attributes")]
    float scaleZoom = 1f;
    [SerializeField] float minScale = 1f;
    [SerializeField] float maxScale = 8f;
    [SerializeField] float zoomSensitivity = 0.006f;
    [SerializeField] float zoomDragSensitivity = 0.2f;

    [SerializeField] Canvas canvas;

    void Start()
    {
        if(!inited)
        {
            rectSize = new Vector2(GetComponent<RectTransform>().rect.width,
                                   GetComponent<RectTransform>().rect.height);

            imageSize = new Vector2(GetComponent<UnityEngine.UI.Image>().sprite.rect.width,
                                    GetComponent<UnityEngine.UI.Image>().sprite.rect.height);

            screenSize = new Vector2((float)Screen.width, (float)Screen.height);

            screenAspectRatio = screenSize.x / screenSize.y;
            imageAspectRatio = imageSize.x / imageSize.y;

            canvasScale = screenSize.x / canvas.GetComponent<CanvasScaler>().referenceResolution.x;

            inited = true;
        }

    }

    private void OnDisable()
    {
        Reset();
    }

    public void Reset()
    {
        GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
        scaleZoom = 1f;
        
        inited = false;
    }

    void Update () 
    {
        var ms = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            touchStart = ms;
        }

        if (Input.touchCount == 2)
        {
            ScaleBlockPan = true;

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            pinchCenter =  0.5f * (touchZero.position + touchOne.position - 0.5f * screenSize);

            Zoom(- difference * zoomSensitivity);             

        }
        else if (Input.GetMouseButton(0) && !ScaleBlockPan)            
        {
            Vector2 displacement = ms - touchStart;

            MoveImage(displacement);

            touchStart = ms;
        }
        else if (Input.touchCount == 0)
        {
            ScaleBlockPan = false;
        }


#if UNITY_EDITOR
        pinchCenter = Input.mousePosition;
        pinchCenter -= screenSize / 2f;
        Zoom(Input.GetAxis("Mouse ScrollWheel"));
#endif
	}

    private void MoveImage(Vector2 displacement)
    {
        Vector2 imagePos = gameObject.GetComponent<RectTransform>().anchoredPosition;

        imagePos += displacement/canvasScale;

        imagePos = ClampToViewport(imagePos);

        gameObject.GetComponent<RectTransform>().anchoredPosition = imagePos;

    }

    private Vector2 ClampToViewport(Vector2 imagePosition)
    {
        

        if (scaleZoom <= 1.0f)
        {
            dx = 0f;
            imagePosition.x = 0.0f;
        }
        else
        {
            dx = Mathf.Abs(((screenSize.x / canvasScale) - (rectSize.x * scaleZoom)) / 2f);
            imagePosition.x = Mathf.Clamp(imagePosition.x, -dx, dx);
        }

        if (scaleZoom * screenAspectRatio / imageAspectRatio  <= 1.0f)
        {
            dy = 0f;
            imagePosition.y = 0.0f;
        }
        else
        {
            dy = Mathf.Abs(((screenSize.y / canvasScale) - (rectSize.x * scaleZoom / imageAspectRatio)) / 2f) ;
            imagePosition.y = Mathf.Clamp(imagePosition.y, -dy, dy);
        }

        return imagePosition;

    }

    void Zoom(float increment)
    {
        if (increment == 0F)
        {
            return;
        }
        
        scaleZoom = Mathf.Clamp(scaleZoom - increment, minScale, maxScale);

        if (scaleZoom == maxScale)
        {
            return;
        }

        gameObject.GetComponent<RectTransform>().localScale = new Vector3(scaleZoom, scaleZoom,1f);

        //Vector2 displacement = maxScale * zoomDragSensitivity * increment * pinchCenter;
        //MoveImage(displacement);

        Vector2 p0 = gameObject.GetComponent<RectTransform>().anchoredPosition;

        float magnitude = -0.5f*(1f-1f/scaleZoom);
        
        Vector2 p1 = magnitude*pinchCenter;
        
        MoveImage(p1-p0);
        
        //DEBUG
        _displacement = p1 - p0;
        _dir = pinchCenter;
        _magnitude = magnitude;
    }

    //PUBLIC VARIABLES FOR DEBUGGING PURPOSES
    public Vector2 _displacement =new Vector2(0f,0f);
    public Vector2 _dir =new Vector2(0f,0f);
    public float _magnitude = 0f;

    // Extended Matrix approach => not working well.
    //          TODO LIST.
    //          1. check ref system change
    //          2. check the composition order
    //          3. check extension from 2 to 4 dimensions
    //          4. build matrix multiplication system to operate in 2x2 extended to 3x3 system (?)
    //
    // private Matrix4x4 TSiTextendedMatrix(float scale, Vector2 point) {
    //     Vector3 scale3 = new Vector3(scale, scale, 0f);
    //     Vector3 point3 = new Vector3(point.x,point.y,1f);

    //     Matrix4x4 traf0 = Matrix4x4.Translate(point3);
    //     Matrix4x4 traf1 = Matrix4x4.Scale(scale3);
    //     Matrix4x4 traf2 = Matrix4x4.Translate(-point3);

    //     Matrix4x4 comp = Matrix4x4.zero;

    //     comp = traf2 * traf1 * traf0;
    //     return traf0;
    // }


}
