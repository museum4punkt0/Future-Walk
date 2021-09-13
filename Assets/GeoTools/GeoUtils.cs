using System;
using System.Collections.Generic;
using GeoJSON;
using UnityEngine;

public class GeoUtils : MonoBehaviour
{
    public static readonly double EarthRadius = 6371000.0;

    /**
     * Calculate the approximate distance between two coordinates (lat/lon)
     *
     * © Chris Veness, MIT-licensed,
     * http://www.movable-type.co.uk/scripts/latlong.html#equirectangular
     */
    public static double Distance(PositionObject p1, PositionObject p2)
    {
        double d_lng = (p2.longitude - p1.longitude) * Math.PI / 180.0;
        double lat1 = p1.latitude * Math.PI / 180.0;
        double lat2 = p2.latitude * Math.PI / 180.0;

        double x = d_lng * Math.Cos((lat1 + lat2) / 2.0);
        double y = (lat2 - lat1);

        return (EarthRadius * Math.Sqrt(x * x + y * y));
    }

    public static double Length(LineStringGeometryObject path)
    {
        if (path.coordinates.Count < 2)
            return 0;

        double result = 0;
        for (var i = 1; i < path.coordinates.Count; i++)
        {
            result += Distance(path.coordinates[i - 1], path.coordinates[i]);
        }

        return result;
    }

    public static ICollection<double> LengthSegments(LineStringGeometryObject path)
    {
        var result = new List<double>();
        if (path.coordinates.Count < 2)
            return result;

        for (var i = 1; i < path.coordinates.Count; i++)
        {
            result.Add(Distance(path.coordinates[i - 1], path.coordinates[i]));
        }

        return result;
    }

    public static PositionObject Interpolate(PositionObject p1, PositionObject p2, double p)
    {
        double d_lng = p2.longitude - p1.longitude;
        double lng = p1.longitude + (d_lng * p);

        double d_lat = p2.latitude - p1.latitude;
        double lat = p1.latitude + (d_lat * p);

        //var d_alt = p2.altitude - p1.altitude;
        //var alt = p1.altitude + (d_alt * p);

        return new PositionObject(lng, lat);
    }


    public static bool Inside(PointGeometryObject p, double r, PositionObject pos)
    {
        return Distance(p.coordinates, pos) < r;
    }

    public static bool Inside(PolygonGeometryObject p, PositionObject pos)
    {
        //var dist = Distance(p.coordinates, pos);

        // Get the angle between the point and the
        // first and last vertices.

        int max_point = p.coordinates[0].Count - 1;
        double total_angle = GetAngle(
            p.coordinates[0][max_point].longitude, p.coordinates[0][max_point].latitude,
            pos.longitude, pos.latitude,
            p.coordinates[0][0].longitude, p.coordinates[0][0].latitude);

        // Add the angles from the point
        // to each other pair of vertices.
        for (int i = 0; i < max_point; i++)
        {
            total_angle += GetAngle(
                p.coordinates[0][i].longitude, p.coordinates[0][i].latitude,
                pos.longitude, pos.latitude,
                p.coordinates[0][i + 1].longitude, p.coordinates[0][i + 1].latitude);
        }

        // The total angle should be 2 * PI or -2 * PI if
        // the point is in the polygon and close to zero
        // if the point is outside the polygon.
        // The following statement was changed. See the comments.
        //return (Math.Abs(total_angle) > 0.000001);
        return (Math.Abs(total_angle) > 1);
    }



    // Return the angle ABC.
    // Return a value between PI and -PI.
    // Note that the value is the opposite of what you might
    // expect because Y coordinates increase downward.
    public static double GetAngle(double Ax, double Ay,
        double Bx, double By, double Cx, double Cy)
    {
        // Get the dot product.
        double dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

        // Get the cross product.
        double cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

        // Calculate the angle.
        return (double)Math.Atan2(cross_product, dot_product);
    }

    // Return the cross product AB x BC.
    // The cross product is a vector perpendicular to AB
    // and BC having length |AB| * |BC| * Sin(theta) and
    // with direction given by the right-hand rule.
    // For two vectors in the X-Y plane, the result is a
    // vector with X and Y components 0 so the Z component
    // gives the vector's length and direction.
    public static double CrossProductLength(double Ax, double Ay,
        double Bx, double By, double Cx, double Cy)
    {
        // Get the vectors' coordinates.
        double BAx = Ax - Bx;
        double BAy = Ay - By;
        double BCx = Cx - Bx;
        double BCy = Cy - By;

        // Calculate the Z coordinate of the cross product.
        return (BAx * BCy - BAy * BCx);
    }

    // Return the dot product AB · BC.
    // Note that AB · BC = |AB| * |BC| * Cos(theta).
    private static double DotProduct(double Ax, double Ay,
        double Bx, double By, double Cx, double Cy)
    {
        // Get the vectors' coordinates.
        double BAx = Ax - Bx;
        double BAy = Ay - By;
        double BCx = Cx - Bx;
        double BCy = Cy - By;

        // Calculate the dot product.
        return (BAx * BCx + BAy * BCy);
    }
}
