using System.Collections;
using System.Collections.Generic;
using GeoJSON;
using UnityEngine;

public class GeoFence
{
    public FeatureObject Feature { get; }
    public PositionObject Center { get; }
    public string Id { get; }
    public double Radius { get; }

    public GeoFence(FeatureObject feature, PositionObject center, string id, double radius = 0)
    {
        this.Feature = feature;
        this.Center = center;
        this.Id = id;
        this.Radius = radius;
    }
}
