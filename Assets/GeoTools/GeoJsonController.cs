using System.Collections.Generic;
using GeoJSON;
using UnityEngine;
using UnityEngine.UI;

public class GeoJsonController : MonoBehaviour
{

    //------------------------------------
    // static
    //------------------------------------
    static readonly string ID_STRING = "id";
    static readonly string RADIUS_STRING = "radius";
    static readonly string SPEED_STRING = "speed";

    public static GeoJsonController instance;

    /*
     * edit geojson here - kulturforum
     * http://geojson.io/#map=18/52.50845/13.36709
     * https://geojson.net/#18/52.50845/13.36709
     */
    
    //----------------------------

    // info text
    [SerializeField] private Text infoText;
    [SerializeField] private TextAsset geojsonMapAsset;
    [SerializeField] private TextAsset geojsonWalkingAsset;

    //
    private FeatureCollection featureCollection = null;
    private LineStringGeometryObject walkingPath = null;
    public double pathLength { get; set; }
    public double walkSpeed { get; set; }
    
    List<GeoFence> geoFences = new List<GeoFence>();
    List<GeoFence> geoFencesEntered = new List<GeoFence>();

    // callbacks
    public delegate void EnterCallbackDelegate(string assetId);
    public delegate void ExitCallbackDelegate(string assetId);
    public delegate void DistanceCallbackDelegate(string assetId, double distance, double radius);

    private List<EnterCallbackDelegate> EnterCb = new List<EnterCallbackDelegate>();
    private List<ExitCallbackDelegate> ExitCb = new List<ExitCallbackDelegate>();

    private Dictionary<string, List<EnterCallbackDelegate>> idEnteredCbs = new Dictionary<string, List<EnterCallbackDelegate>>();
    private Dictionary<string, List<ExitCallbackDelegate>> idExitedCbs = new Dictionary<string, List<ExitCallbackDelegate>>();

    private Dictionary<string, List<DistanceCallbackDelegate>> idDistanceCbs = new Dictionary<string, List<DistanceCallbackDelegate>>();

    // ----------------- enter callback -----------------------

    public void AddOnEnterFence(EnterCallbackDelegate cb)
    {
        if (!EnterCb.Contains(cb))
        {
            EnterCb.Add(cb);
        }
    }
    public void RemoveOnEnterFence(EnterCallbackDelegate cb)
    {
        if (EnterCb.Contains(cb))
        {
            EnterCb.Remove(cb);
        }
    }

    // ----------------- exit callback -----------------------

    public void AddOnExitFence(ExitCallbackDelegate cb)
    {
        if (!ExitCb.Contains(cb))
        {
            ExitCb.Add(cb);
        }
    }
    public void RemoveOnExitFence(ExitCallbackDelegate cb)
    {
        if (ExitCb.Contains(cb))
        {
            ExitCb.Remove(cb);
        }
    }

    // ----------------- enter callback for specific ids -----------------------

    public void AddOnEnterFence(string id, EnterCallbackDelegate cb)
    {
        if (!idEnteredCbs.ContainsKey(id))
        {
            idEnteredCbs.Add(id, new List<EnterCallbackDelegate>());
        }

        if (!idEnteredCbs[id].Contains(cb))
        {
            idEnteredCbs[id].Add(cb);
        }
    }
    public void RemoveOnEnterFence(string id, EnterCallbackDelegate cb)
    {
        if (!idEnteredCbs.ContainsKey(id))
        {
            return;
        }

        if (idEnteredCbs[id].Contains(cb))
        {
            idEnteredCbs[id].Remove(cb);

            if (idEnteredCbs[id].Count == 0)
            {
                idEnteredCbs.Remove(id);
            }
        }
    }
    public void ClearOnEnterFence(string id)
    {
        if (!idEnteredCbs.ContainsKey(id))
        {
            return;
        }

        idEnteredCbs.Remove(id);
    }

    // ----------------- exit callback for specific ids -----------------------

    public void AddOnExitFence(string id, ExitCallbackDelegate cb)
    {
        if (!idExitedCbs.ContainsKey(id))
        {
            idExitedCbs.Add(id, new List<ExitCallbackDelegate>());
        }

        if (!idExitedCbs[id].Contains(cb))
        {
            idExitedCbs[id].Add(cb);
        }
    }
    public void RemoveOnExitFence(string id, ExitCallbackDelegate cb)
    {
        if (!idExitedCbs.ContainsKey(id))
        {
            return;
        }

        if (idExitedCbs[id].Contains(cb))
        {
            idExitedCbs[id].Remove(cb);

            if (idExitedCbs[id].Count == 0)
            {
                idExitedCbs.Remove(id);
            }
        }
    }
    public void ClearOnExitFence(string id)
    {
        if (!idExitedCbs.ContainsKey(id))
        {
            return;
        }

        idExitedCbs.Remove(id);
    }

    // ----------------- distance callback for specific ids -----------------------

    public void AddFenceDistanceCb(string id, DistanceCallbackDelegate cb)
    {
        if (!idDistanceCbs.ContainsKey(id))
        {
            idDistanceCbs.Add(id, new List<DistanceCallbackDelegate>());
        }

        if (!idDistanceCbs[id].Contains(cb))
        {
            idDistanceCbs[id].Add(cb);
        }
    }
    public void RemoveFenceDistanceCb(string id, DistanceCallbackDelegate cb)
    {
        if (!idDistanceCbs.ContainsKey(id))
        {
            return;
        }

        if (idDistanceCbs[id].Contains(cb))
        {
            idDistanceCbs[id].Remove(cb);

            if (idDistanceCbs[id].Count == 0)
            {
                idDistanceCbs.Remove(id);
            }
        }
    }
    public void RemoveFenceDistanceCb(DistanceCallbackDelegate cb)
    {
        foreach(var item in idDistanceCbs)
        {
            item.Value.Remove(cb);
        }
    }
    public void ClearFenceDistanceCb(string id)
    {
        if (!idDistanceCbs.ContainsKey(id))
        {
            return;
        }

        idDistanceCbs.Remove(id);
    }

    // ----------------- call callbacks -----------------------

    private void callEnter(string id)
    {
        foreach (var cb in EnterCb)
        {
            cb.Invoke(id);
        }

        // call specific callbacks for ids
        if (idEnteredCbs.ContainsKey(id))
        {
            foreach (var cb in idEnteredCbs[id])
            {
                cb.Invoke(id);
            }
        }
    }
    private void callExit(string id)
    {
        foreach (var cb in ExitCb)
        {
            cb.Invoke(id);
        }

        // call specific callbacks for ids
        if (idExitedCbs.ContainsKey(id))
        {
            foreach (var cb in idExitedCbs[id])
            {
                cb.Invoke(id);
            }
        }
    }


    // ----------------- features -----------------------

    void loadFeatures()
    {        
        if (geojsonMapAsset)
        {
            //Debug.Log("load features from geojsonMapAsset");
            featureCollection = GeoJSONObject.Deserialize(geojsonMapAsset.text);
        }
        else
        {
            Debug.LogError("no geojsonMapAsset");
        }

        //
        // calc feature centers
        if (featureCollection != null &&
            featureCollection.features.Count > 0)
        {

            foreach (var feature in featureCollection.features)
            {
                // TODO: calculate radius

                if (feature.geometry is GeoJSON.PointGeometryObject)
                {
                    var p = feature.geometry as GeoJSON.PointGeometryObject;

                    double radius = 0;
                    if (feature.properties.ContainsKey(RADIUS_STRING))
                    {
                        radius = double.Parse(feature.properties[RADIUS_STRING]);
                    }

                    // check for ID
                    if (feature.properties.ContainsKey(ID_STRING))
                    {
                        // add a geofence
                        geoFences.Add(new GeoFence(feature, p.coordinates, feature.properties[ID_STRING], radius));
                    }
                    else
                    {
                        // no id - ignore this
                        // https://www.openstreetmap.org/#map=19/52.55330/13.34180
                        Debug.Log("ignoring geo fence: " + feature.geometry.type + " - center: https://www.openstreetmap.org/#map=18/" +  p.coordinates.latitude + "/" + p.coordinates.longitude);

                    }
                }
                else if (feature.geometry is GeoJSON.PolygonGeometryObject)
                {
                    var p = feature.geometry as GeoJSON.PolygonGeometryObject;
                    var cnt = p.coordinates[0].Count;

                    double lats = 0.0;
                    double lngs = 0.0;


                    foreach (var coord in p.coordinates[0])
                    {
                        lats += coord.latitude;
                        lngs += coord.longitude;
                    }

                    PositionObject center = new PositionObject(lngs / cnt, lats / cnt);

                    // calc radius
                    double radius = 0;

                    foreach (var coord in p.coordinates[0])
                    {
                        double d = GeoUtils.Distance(coord, center);
                        if (d > radius)
                        {
                            radius = d;
                        }
                    }

                    // check for ID
                    if (feature.properties.ContainsKey(ID_STRING))
                    {
                        // add a geofence
                        geoFences.Add(new GeoFence(feature, center, feature.properties[ID_STRING], radius));
                    }
                    else
                    {
                        // no id - ignore this
                        // https://www.openstreetmap.org/#map=19/52.55330/13.34180
                        Debug.Log("ignoring geo fence: " + feature.geometry.type + " - center: https://www.openstreetmap.org/#map=18/" +  center.latitude + "/" + center.longitude);
                    }
                }
                else if (feature.geometry is GeoJSON.LineStringGeometryObject)
                {
                    var p = feature.geometry as GeoJSON.LineStringGeometryObject;
                    var cnt = p.coordinates.Count;

                    double lats = 0.0;
                    double lngs = 0.0;

                    foreach (var coord in p.coordinates)
                    {
                        lats += coord.latitude;
                        lngs += coord.longitude;
                    }

                    PositionObject center = new PositionObject(lngs / cnt, lats / cnt);

                    // calc radius
                    double radius = 0;

                    foreach (var coord in p.coordinates)
                    {
                        double d = GeoUtils.Distance(coord, center);
                        if (d > radius)
                        {
                            radius = d;
                        }
                    }

                    // check for ID
                    if (feature.properties.ContainsKey(ID_STRING))
                    {
                        // add a geofence
                        geoFences.Add(new GeoFence(feature, center, feature.properties[ID_STRING], radius));
                    }
                    else
                    {
                        // no id - ignore this
                        // https://www.openstreetmap.org/#map=19/52.55330/13.34180
                        Debug.Log("ignoring geo fence: " + feature.geometry.type + " - center: https://www.openstreetmap.org/#map=18/" +  center.latitude + "/" + center.longitude);
                    }
                }
            }

        }
        else
        {
            Debug.LogError("no features!");
        }
    }

    /// <summary>
    /// log features
    /// </summary>
    void logFeatures()
    {
        foreach (var item in geoFences)
        {
            Debug.Log("feature: " + item.Feature.geometry.type + " center @ " + item.Center);

            if (item.Feature.geometry is GeoJSON.PointGeometryObject)
            {
                //
            }
            else if (item.Feature.geometry is GeoJSON.PolygonGeometryObject)
            {
                var p = item.Feature.geometry as GeoJSON.PolygonGeometryObject;
                var c = p.coordinates[0].Count;
                Debug.Log("count: " + c);
            }

            foreach (var attr in item.Feature.properties)
            {
                Debug.Log(attr.Key + ": " + attr.Value);
            }
        }
    }

    /// <summary>
    /// load walking path
    /// </summary>
    void loadWalkingPath()
    {
        FeatureCollection pathcoll = null;

        if (geojsonWalkingAsset)
        {
            //Debug.Log("load path from geojsonWalkingAsset");
            pathcoll = GeoJSONObject.Deserialize(geojsonWalkingAsset.text);
        }
        else
        {
            return;
        }

        if (pathcoll != null &&
            pathcoll.features.Count > 0)
        {
            // get first LineString
            foreach (var feature in pathcoll.features)
            {
                if (feature.geometry is GeoJSON.LineStringGeometryObject &&
                    (feature.geometry as GeoJSON.LineStringGeometryObject).coordinates.Count > 0)
                {
                    walkingPath = feature.geometry as GeoJSON.LineStringGeometryObject;

                    pathLength = GeoUtils.Length(walkingPath);

                    if (feature.properties.ContainsKey(SPEED_STRING))
                    {
                        walkSpeed = float.Parse(feature.properties[SPEED_STRING].ToString());
                    }

                    break;
                }
            }
        }

        // initially check start of path
        if (walkingPath != null)
        {
            //checkPosition(walkingPath.coordinates[0]);
        }
        else
        {
            Debug.LogError("no walking path!");
        }
    }

    /// <summary>
    /// log walking path
    /// </summary>
    void logWalkingPath()
    {
        if (walkingPath == null)
        {
            Debug.LogError("no walking path!");
        }

        Debug.Log("-------- walking path: " + walkingPath);
        Debug.Log("path length: " + pathLength + " [m]");
        Debug.Log("walking speed: " + walkSpeed + " [m/s]");
    }

    /// <summary>
    /// get point on path
    /// </summary>
    /// <param name="p">
    /// percent on path: 0..1
    /// </param>
    /// <returns>
    /// point on path
    /// </returns>
    public PositionObject PointOnPath(double percent)
    {
        if (walkingPath == null)
        {
            return new PositionObject(0, 0);
        }

        if (walkSpeed == 0)
        {
            return walkingPath.coordinates[0];
        }

        if (percent < 0) percent = 0;
        if (percent > 1) percent = 1;

        double dist_from_start = pathLength * percent;

        for (var i = 1; i < walkingPath.coordinates.Count; i++)
        {
            PositionObject p1 = walkingPath.coordinates[i - 1];
            PositionObject p2 = walkingPath.coordinates[i];

            var sd = GeoUtils.Distance(p1, p2);

            if (dist_from_start > sd)
            {
                dist_from_start -= sd;
            }
            else
            {
                double pp = dist_from_start / sd;
                return GeoUtils.Interpolate(p1, p2, pp);
            }
        }

        return new PositionObject(0, 0);
    }




    /// <summary>
    /// check if position p is inside or outside of any feature
    /// call callback functions accordingly
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lng"></param>
    /// <param name="alt"></param>
    public void checkPosition(double lat, double lng, double alt = 0)
    {
        checkPosition(new PositionObject(lng, lat));
    }

    /// <summary>
    /// check if position p is inside or outside of any feature
    /// call callback functions accordingly
    /// </summary>
    /// <param name="p"></param>
    public void checkPosition(PositionObject p)
    {
        if (featureCollection == null ||
            geoFences.Count == 0)
        {
            Debug.Log("no features!");
            return;
        }

        var distances = new List<string>();

        //------------------------------------------------
        foreach (var fence in geoFences)
        {
            // dist to center
            double dist = GeoUtils.Distance(p, fence.Center);

            // Debug.Log("p: " + p + " -- " + fence.Center + " dist: " + dist);

            //------------------------------------------------
            // hack for test-version
            // there might be more fences with same id
            // only use the closest of these fences for distance-callback
            // call distance callbacks below
            // if (idDistanceCbs.ContainsKey(item.Id))
            // {
            //     foreach(var cb in idDistanceCbs[item.Id])
            //     {
            //         cb.Invoke(item.Id, (float)dist, item.Radius);
            //     }
            // }
            //------------------------------------------------

            var feature = fence.Feature;

            if (feature.geometry is PointGeometryObject)
            {
                distances.Add("point: " + dist.ToString("F3") + " (" + fence.Center.ToString() + ") " + fence.Radius.ToString("F3"));

                if (dist < fence.Radius)
                //if (feature.properties.ContainsKey(RADIUS_STRING))
                {
                    // check if we are withing radius
                    double r = double.Parse(feature.properties[RADIUS_STRING].ToString());

                    if (GeoUtils.Inside(feature.geometry as PointGeometryObject, r, p))
                    {
                        if (!geoFencesEntered.Contains(fence))
                        {
                            // we are inside
                            geoFencesEntered.Add(fence);

                            callEnter(fence.Id);
                        }

                    }
                    else if (geoFencesEntered.Contains(fence))
                    {
                        // we are outside
                        geoFencesEntered.Remove(fence);

                        callExit(fence.Id);
                    }
                }
            }
            else if (feature.geometry is PolygonGeometryObject)
            {
                /*
                 * TODO:
                 * optimize this: 
                 * only calculate inside if we are close enough to the center
                 * use item.Radius for this.
                 */

                distances.Add("poly: " + dist.ToString("F3") + " (" + fence.Center.ToString() + ") " + fence.Radius.ToString("F3"));

                // use radius
                if (dist < fence.Radius)
                //if (GeoUtils.Inside(feature.geometry as PolygonGeometryObject, p))
                {
                    if (!geoFencesEntered.Contains(fence))
                    {
                        // we are inside
                        geoFencesEntered.Add(fence);

                        callEnter(fence.Id);
                    }
                }
                else if (geoFencesEntered.Contains(fence))
                {
                    // we are outside
                    geoFencesEntered.Remove(fence);

                    callExit(fence.Id);
                }
            }
            else if (feature.geometry is LineStringGeometryObject)
            {
                // TODO: implement crossing lines
            }
        } // foreach fences


        //------------------------------------------------
        //------------------------------------------------
        // hack for test-version
        // see explanation above    
        Dictionary<string, double> shortestdist = new Dictionary<string, double>();
        Dictionary<string, double> radii = new Dictionary<string, double>();

        // distance callbacks
        foreach (var item in geoFences)
        {
            // dist to center
            double dist = GeoUtils.Distance(p, item.Center);

            // call distance callbacks
            if (idDistanceCbs.ContainsKey(item.Id))
            {
                radii[item.Id] = item.Radius;

                if (shortestdist.ContainsKey(item.Id))
                {
                    if (shortestdist[item.Id] > dist) {
                        shortestdist[item.Id] = dist;
                    }
                }
                else
                {
                    shortestdist.Add(item.Id, dist);
                }
            }
        }
        // send out callbacks
        foreach (var dist in shortestdist)
        {
            foreach (var cb in idDistanceCbs[dist.Key])
            {
                cb.Invoke(dist.Key, (float)dist.Value, radii[dist.Key]);
            }
        }
        // hack done
        //------------------------------------------------
        //------------------------------------------------

        //------------------------------------------------
        // set info
        infoText.text = string.Join("\n", distances);
    }

    public bool hasEntered(string id)
    {
        foreach(var item in geoFencesEntered)
        {
            if (item.Id.Equals(id))
            {
                return true;
            }
        }

        return false;
    }

    GeoFence getFence(string id)
    {
        foreach (var fence in geoFences)
        {
            if (fence.Id == id)
                return fence;
        }

        return null;
    }

    public bool hasFence(string id)
    {
        return getFence(id) != null;
    }

    public double fenceRadius(string id)
    {
        var fence = getFence(id);
        if (fence != null)
        {
            return fence.Radius;
        }

        return 0.0;
    }

    public double distanceTo(string id, PositionObject pos)
    {
        // HACK for testing
        // there might be more than one geofence with same id right now
        // get the closest one

        double closestDist = double.MaxValue;
        GeoFence closestFence = null;

        foreach (var fence in geoFences)
        {
            if (fence.Id == id)
            {
                var dist = GeoUtils.Distance(fence.Center, pos);
                if (dist < closestDist) {
                    closestDist = dist;
                    closestFence = fence;
                }
            }
        }

        if (closestFence != null) 
        {
            return closestDist;
        }

        return -1;
    }

    public double distanceToNormalized(string id, PositionObject pos)
    {
        // HACK for testing
        // there might be more than one geofence with same id right now
        // get the closest one

        double closestDist = double.MaxValue;
        GeoFence closestFence = null;

        foreach (var fence in geoFences)
        {
            if (fence.Id == id)
            {
                var dist = GeoUtils.Distance(fence.Center, pos);
                if (dist < closestDist) {
                    closestDist = dist;
                    closestFence = fence;
                }
            }
        }

        if (closestFence != null) 
        {
            return closestDist / closestFence.Radius;
        }

        return -1;

    }

    //----------------------------------
    //----------------------------------

    void Awake()
    {
        instance = this;

        if (infoText)
        {
            infoText.text = "";
        }

        loadFeatures();
        loadWalkingPath();

        //logFeatures();
        //logWalkingPath();
    }
}
