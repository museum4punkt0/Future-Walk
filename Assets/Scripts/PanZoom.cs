using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanZoom : MonoBehaviour
{
    Vector3 touchStart;
    //Vector3 initialPos;
    Vector3 initialCamPosition;
    float initialCamZoom;
    bool inited = false;

    [SerializeField] Camera cam;
    [SerializeField] float zoomOutMin = 1f;
    [SerializeField] float zoomOutMax = 8f;
    [SerializeField] float dragSensitivity = 0.1f;

    void Start()
    {
        //initialPos = gameObject.transform.position;
         
        inited = true;

        if(cam == null)
        {
            Debug.Log("camera not added, looking for camera");
            cam = FindObjectOfType<Camera>();
        }
        initialCamPosition = cam.transform.position;
        initialCamZoom = cam.orthographicSize;


    }


	public void Reset()
    {
        //gameObject.transform.localScale = new Vector3(1, 1, 0);
        //gameObject.transform.localScale = new Vector3(1, 1, 1);
        if (inited)
        {
            //gameObject.transform.position = initialPos;
            cam.transform.position = initialCamPosition;
            cam.orthographicSize = initialCamZoom;
        }
    }
	
	void Update () {
        //float sizeFactor = gameObject.transform.localScale.x;

        if(Input.GetMouseButtonDown(0)){

            touchStart = Input.mousePosition;

        }
        if(Input.touchCount == 2){

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            zoom(difference * 0.01f);
        }
        else if(Input.GetMouseButton(0))
        {
            //direction to drag
            Vector3 direction = touchStart - Input.mousePosition;
            //current cam position
            Vector3 camPosition = cam.transform.position;
            camPosition += dragSensitivity * direction;

            //clamp to viewport
            camPosition.x = Mathf.Clamp(camPosition.x, -9f, 9f);
            camPosition.y = Mathf.Clamp(camPosition.y, -7f, 7f);
            cam.transform.SetPositionAndRotation(camPosition, Quaternion.identity);

            touchStart = Input.mousePosition;
        }

        zoom(Input.GetAxis("Mouse ScrollWheel"));
	}

    void zoom(float increment)
    {
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }
}
