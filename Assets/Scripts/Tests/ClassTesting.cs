using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry;

public class ClassTesting : MonoBehaviour
{
    public float inAngle;
    public float angle;
    public Rect bounds;
	private void Update()
	{
        List<Vector2> p = new List<Vector2>();
        p.Add(new Vector2(0, 0));
        p.Add(new Vector2(2, 0));
        p.Add(new Vector2(1.5f, 1));
        p.Add(new Vector2(0.5f, 1));

        for (int i = 0; i < 4; i ++)
        {
            p[i] = p[i].RotatedAround(Vector2.zero, inAngle);
        }

        bounds = HelperFunctions.GetOrientedBounds(p, ref angle);
        angle *= Mathf.Rad2Deg;
	}
}