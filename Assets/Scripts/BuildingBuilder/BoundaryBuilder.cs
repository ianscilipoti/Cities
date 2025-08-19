using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BoundaryBuilder
{
    CityRegion region;
    City city;
    BoundaryContourPoint[] contour;

    public static BoundaryContourPoint[] roadContour = new BoundaryContourPoint[] {
        new BoundaryContourPoint(Vector2.zero, null, 0f),
        new BoundaryContourPoint(new Vector2(0, -0.1f), null, 1f),
        new BoundaryContourPoint(new Vector2(1, -0.1f), null, 1f),
        new BoundaryContourPoint(Vector2.right, null, 0f) 
    };

    public BoundaryBuilder (CityRegion region, City city, BoundaryContourPoint[] contour)
    {
        this.region = region;
        this.city = city;
        this.contour = contour;
    }

    public void PlaceBoundary()
    {
        GameObject buildingObject = new GameObject("Boundary", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
        Mesh mesh = GetMesh();
        buildingObject.GetComponent<MeshFilter>().mesh = mesh;
        buildingObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        buildingObject.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Road");
        buildingObject.transform.parent = city.cityParent;
    }

    public Mesh GetMesh ()
    {
        Vector2[] regionPoints = region.GetPoints();
        Vector3[] verts = new Vector3[regionPoints.Length + 1];
        Vector2[] UVs = new Vector2[regionPoints.Length + 1];
        int[] tris = new int[regionPoints.Length*3];

        Vector3 avg = Vector3.zero;
        for (int i = 0; i < regionPoints.Length; i ++)
        {
            float x = regionPoints[i].x;
            float z = regionPoints[i].y;
            float heightSample = city.SampleElevation(x, z);
            verts[i] = new Vector3(x, heightSample, z);
            UVs[i] = new Vector2(x, z);
            avg += verts[i] / regionPoints.Length;
        }

        verts[regionPoints.Length] = avg;
        UVs[regionPoints.Length] = new Vector2(avg.x, avg.z);
        int centerVertInd = regionPoints.Length;

        for (int i = 0; i < regionPoints.Length; i++)
        {
            tris[i * 3] = i;
            tris[i * 3 + 2] = (i+1) % regionPoints.Length;
            tris[i * 3 + 1] = centerVertInd;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = UVs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public struct BoundaryContourPoint 
    {
        public Vector2 point;//x is the between 0-1. Gets scaled. //y is a relative offset from either terrain height or "level" height
        public Material material;
        public float levelness;

        public BoundaryContourPoint (Vector2 point, Material material, float levelness)
        {
            this.point = point;
            this.material = material;
            this.levelness = levelness;
        }
    }
}
