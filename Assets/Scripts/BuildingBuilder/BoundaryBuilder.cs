using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BoundaryBuilder
{
    CityEdge edge;
    City city;
    BoundaryContourPoint[] contour;

    public static BoundaryContourPoint[] roadContour = new BoundaryContourPoint[] {
        new BoundaryContourPoint(Vector2.zero, null, 0f),
        new BoundaryContourPoint(new Vector2(0, -0.1f), null, 1f),
        new BoundaryContourPoint(new Vector2(1, -0.1f), null, 1f),
        new BoundaryContourPoint(Vector2.right, null, 0f) 
    };

    public BoundaryBuilder (CityEdge edge, City city, BoundaryContourPoint[] contour)
    {
        this.edge = edge;
        this.city = city;
        this.contour = contour;
    }

    public void PlaceBoundary()
    {
        GameObject buildingObject = new GameObject("Boundary", typeof(MeshRenderer), typeof(MeshFilter));
        Mesh mesh = GetMesh();
        buildingObject.GetComponent<MeshFilter>().mesh = mesh;
        buildingObject.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Road");
        buildingObject.transform.parent = city.cityParent;
    }

    public Mesh GetMesh ()
    {
        Vector3 aPt = new Vector3(edge.a.pt.x, city.SampleElevation(edge.a.pt.x, edge.a.pt.y), edge.a.pt.y);
        Vector3 bPt = new Vector3(edge.b.pt.x, city.SampleElevation(edge.b.pt.x, edge.b.pt.y), edge.b.pt.y);
        Vector3 segmentDirection = bPt - aPt;
        Vector3 leftDirection = Vector3.Cross(segmentDirection, Vector3.up).normalized * edge.GetWidth() * 0.5f;
        Mesh mesh = new Mesh();

        //front right / front left edge
        CityEdge flEdge = (CityEdge)edge.b.GetLeftConnection(edge);
        CityEdge frEdge = (CityEdge)edge.b.GetRightConnection(edge);

        CityEdge blEdge = (CityEdge)edge.a.GetRightConnection(edge);
        CityEdge brEdge = (CityEdge)edge.a.GetLeftConnection(edge);

        float halfWidth = edge.GetWidth() / 2;

        Vector2 frInter = HelperFunctions.GetIntersectionPoint(edge.a.pt, edge.b.pt, frEdge.GetOppositeVertex(edge.b).pt, halfWidth, frEdge.GetWidth()/2);
        Vector2 flInter = HelperFunctions.GetIntersectionPoint(flEdge.GetOppositeVertex(edge.b).pt, edge.b.pt, edge.a.pt, flEdge.GetWidth() / 2, halfWidth);

        Vector2 brInter = HelperFunctions.GetIntersectionPoint(brEdge.GetOppositeVertex(edge.a).pt, edge.a.pt, edge.b.pt, brEdge.GetWidth() / 2, halfWidth);
        Vector2 blInter = HelperFunctions.GetIntersectionPoint(edge.b.pt, edge.a.pt, blEdge.GetOppositeVertex(edge.a).pt, halfWidth, blEdge.GetWidth() / 2);



        Vector3[] verts = new Vector3[]{
            HelperFunctions.projVec2(blInter, city.SampleElevation(blInter.x, blInter.y)), 
            HelperFunctions.projVec2(brInter, city.SampleElevation(brInter.x, brInter.y)), 
            HelperFunctions.projVec2(flInter, city.SampleElevation(flInter.x, flInter.y)), 
            HelperFunctions.projVec2(frInter, city.SampleElevation(frInter.x, frInter.y)),
            aPt,
            bPt};
        int[] tris = new int[]{
            0, 5, 4, 
            0, 2, 5,
            4, 3, 1,
            4, 5, 3};



        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    //t = -1, t = 1
    private Vector2 GetIntersectionPoint (float t, bool side)
    {
        float scaledT = (t - 0.5f) * edge.GetWidth();
        return Vector2.zero;
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
