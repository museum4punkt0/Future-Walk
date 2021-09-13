using System;
using UnityEngine;
using UnityEngine.UI;

public class GpsController : MonoBehaviour
{
    //------------------------------------
    // static
    //------------------------------------
    public static GpsController instance;


    //------------------------------------
    //
    //------------------------------------
    [SerializeField] Text infoText;
    [SerializeField] Text currentGpsText;
    [SerializeField] GeoJsonController geoJsonController;
    [SerializeField] bool map;                              // location from map

    // [SerializeField]
    // public GameObject marker;

    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);


    // ----- simulation
    double walked = 0;


    // ----- out gps modes
    enum GpsMode
    {
        None,
        Device,
        Map,
        Simulated
    }

    GpsMode gpsMode = GpsMode.None;

    Vector3 initialMousePos;
    private bool locationInit = false;
    bool mouseDown = false;

    string gpsFormatter = "F8";


    // ----------------- functions -----------------------

    // ----- ILocationProvider
    // public Location CurrentLocation { get; set; }
    public event Action<GeoJSON.PositionObject> OnLocationUpdated;

    GeoJSON.PositionObject currentPosition = new GeoJSON.PositionObject();

    // ----- append a text to info
    private void prependText(string text)
    {
        Log.d(text);
        if (infoText) infoText.text = text + "\n" + infoText.text;
    }

    private void setCurrentLocationInfo(double lat, double lng, DateTime date)
    {
        if (currentGpsText)
        {
            currentGpsText.text = lat.ToString(gpsFormatter) + ", " + lng.ToString(gpsFormatter) + "\n" + date;
        }
    }

    public GeoJSON.PositionObject currentLocationAsPO()
    {
        return currentPosition;
        // return new GeoJSON.PositionObject(CurrentLocation.LatitudeLongitude.y, CurrentLocation.LatitudeLongitude.x);
    }

    // ----------------- location callback -----------------------

    void OnLocationUpdate(DateTime dateTime, double lat, double lng, double alt)
    {
        // Log.d("******** " + dateTime + ":  lat: " + lat + "lng: " + lng + " alt: " + alt);
        //prependText(dateTime + "\n" + lat + " " + lng);
    
        currentPosition.latitude = lat;
        currentPosition.longitude = lng;

        // set info and marker
        setCurrentLocationInfo(lat, lng, dateTime);
        updateMarkerWithCurrentLocation();

        if (OnLocationUpdated != null)
        {
            OnLocationUpdated.Invoke(currentPosition);
        }

        if (geoJsonController != null)
        {
            geoJsonController.checkPosition(lat, lng, alt);
        }
    }

    // ----------------- geojson -----------------------
    void updateMarkerWithCurrentLocation()
    {
        // if (marker && map)
        // {
        //     marker.transform.position = map.GeoToWorldPosition(CurrentLocation.LatitudeLongitude);

        //     marker.SetActive(marker.transform.position != Vector3.zero);
        // }
    }

    // ----------------- MonoBehaviour related stuff -----------------------

    void Awake()
    {
        instance = this;

        if (infoText)
        {
            infoText.text = "";
        }

        if (currentGpsText)
        {
            currentGpsText.text = "GPS";
        }

    }

    void Start()
    {
        if (geoJsonController)
        {
           geoJsonController.AddOnEnterFence((string id) =>
           {
              prependText("Entering " + id);
           });

           geoJsonController.AddOnExitFence((string id) =>
           {
              prependText("Exiting " + id);
           });
        }

        updateMarkerWithCurrentLocation();
    }

    public void StartLocationService()
    {        
        // safety
        if (locationInit) return;        
        
        UnityToolbag.Dispatcher.InvokeAsync(() => 
        {
            // start location
            if (NativeToolkit.StartLocation(true, OnLocationUpdate))
            {
                gpsMode = GpsMode.Device;
                locationInit = true;

                prependText("location started");
            }
            else
            {
                gpsMode = GpsMode.None;     

                prependText("could not start location");
            }            
        });
    }

    public void StopLocationService()
    {
        if (!locationInit) return;

        UnityToolbag.Dispatcher.InvokeAsync(() => 
        {
            NativeToolkit.StopLocation();
            locationInit = false;
        });
    }

    void OnGUI ()
    {
// #if PLATFORM_ANDROID
//         if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
//         {            
//             prependText("user denied location permission");
//             return;
//         }
//         else if (!locationInit)
//         {
//             prependText("user granted access to location");
            
//             // StartLocationService();
//         }
// #endif
    }

    void Update()
    {
        if (gpsMode == GpsMode.Map && map)
        {
            checkMap();
        }
        else if (gpsMode == GpsMode.Simulated ||
            (gpsMode == GpsMode.Map && !map))
        {
            walkSimulated();
        }
    }

    // ----------------- update related stuff -----------------------

    void checkMap()
    {
        // track mouse-down
        if (Input.GetMouseButtonDown(0))
        {
            initialMousePos = Input.mousePosition;
            mouseDown = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseDown = false;

            var mouseDiff = Input.mousePosition - initialMousePos;

            if (mouseDiff.magnitude > 5)
            {
                // mouse moved... update marker
                updateMarkerWithCurrentLocation();
                return;
            }
            
            // only accept mouseclick on map
            // !! don't do it this way... for now it's sufficient
            if (Input.mousePosition.x < 2*Screen.width/3)
            {
                return;
            }

            //
            float rayDistance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                var pos = ray.GetPoint(rayDistance);

                if (map)
                {
                    // var geo = map.WorldToGeoPosition(pos);

                    // var loc = new Location();
                    // loc.LatitudeLongitude = geo;// new Vector2d(geo.y, geo.x);
                    // CurrentLocation = loc;

                    // if (OnLocationUpdated != null)
                    //     OnLocationUpdated.Invoke(CurrentLocation);

                    // if (geoJsonController != null)
                    //     geoJsonController.checkPosition(geo.x, geo.y);

                    // //
                    // setCurrentLocationInfo(CurrentLocation.LatitudeLongitude.x, CurrentLocation.LatitudeLongitude.y);
                    // updateMarkerWithCurrentLocation();
                }
            }
        }
    }

    void walkSimulated()
    {
        // TODO: add some random update here...

        if (geoJsonController != null)
        {
            walked += Time.unscaledDeltaTime * geoJsonController.walkSpeed;
            var p = walked / geoJsonController.pathLength;
            var point = geoJsonController.PointOnPath(p);

            // var loc = new Location();
            // loc.LatitudeLongitude.x = point.latitude;
            // loc.LatitudeLongitude.y = point.longitude;
            // CurrentLocation = loc;

            currentPosition = point;

            if (OnLocationUpdated != null)
                OnLocationUpdated.Invoke(currentPosition);

            if (geoJsonController != null)
                geoJsonController.checkPosition(point);

            // update texts and marker
            setCurrentLocationInfo(currentPosition.latitude, currentPosition.longitude, DateTime.Now);
            updateMarkerWithCurrentLocation();
        }
    }

    public double distanceTo(string id)
    {
        if (geoJsonController)
        {
            return geoJsonController.distanceTo(id, currentLocationAsPO());
        }

        return -1;
    }
}
