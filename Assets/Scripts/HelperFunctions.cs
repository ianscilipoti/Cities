using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperFunctions
{
    public static float clipperScale = 1000000f;

    //public static Rect GetOrientedBounds (List<Vector2> points, Vector2 right)
    //{
        
    //}

    public static Vector2 ScaleFrom(Vector2 vec, Vector2 from, float factor)
    {
        Vector2 fromTo = (vec - from).normalized;
        return vec + fromTo * factor;
    }

    public static ClipperLib.IntPoint GetIntPoint (Vector2 pt)
    {
        return new ClipperLib.IntPoint(pt.x * clipperScale, pt.y * clipperScale);
    }

    public static Vector2 GetPoint (ClipperLib.IntPoint pt)
    {
        return new Vector2((float)pt.X / clipperScale, (float)pt.Y / clipperScale);
    }

    // Start is called before the first frame update
    public static Vector2 projVec3 (Vector3 inp)
    {
        return new Vector2(inp.x, inp.z);
    }

    public static Vector3 projVec2 (Vector2 inp) 
    {
        return new Vector3 (inp.x, 0, inp.y);
    }

    public static float acot(float x)
    {
        return Mathf.PI / 2 - Mathf.Atan(x);
    }

    public static Vector2 getRightPerpendicularDirection (Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x).normalized;
    }

    public static Vector2 getLeftPerpendicularDirection(Vector2 vec)
    {
        return -getRightPerpendicularDirection(vec);
    }

    public static Vector2 getReverseDirection(Vector2 vec)
    {
        return -vec.normalized;
    }

    public static float aAlongB (Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) / b.magnitude;
    }



}
