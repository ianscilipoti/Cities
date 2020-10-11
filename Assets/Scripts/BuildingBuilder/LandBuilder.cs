using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using UnityEditor;

public class LandBuilder
{
    private EdgeLoop<CityEdge> footprint;
    private City city;
    private float pointsPerUnit;

    public LandBuilder (EdgeLoop<CityEdge> footprint, City city)
    {
        this.footprint = footprint;
        this.city = city;
        pointsPerUnit = 0.3f;
    }

    public void PlaceLand()
    {
        GameObject buildingObject = new GameObject("LandPlot", typeof(MeshRenderer), typeof(MeshFilter));
        Mesh mesh = GetMesh();
        buildingObject.GetComponent<MeshFilter>().mesh = mesh;
        buildingObject.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Grass");
        buildingObject.transform.parent = city.cityParent;
    }

    public Mesh GetMesh ()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        HashSet<int> borderVerts = new HashSet<int>();

        Polygon footprintPoly = footprint.GetPolygon();

        Rect bounds = footprint.GetBounds();
        Vector2 centroid = footprintPoly.centroid;
        Vector2[] points = footprint.GetPoints();

        for (int i = 0; i < points.Length; i ++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Length];
            float dist = Vector2.Distance(p1, p2) * pointsPerUnit;

            float p1Elev = city.SampleElevation(p1.x, p1.y);
            float p2Elev = city.SampleElevation(p2.x, p2.y);

            int numPointsAlongEdge = Mathf.CeilToInt(dist);
            for (int j = 0; j < numPointsAlongEdge; j++)
            {
                float t = (float)j / (numPointsAlongEdge);
                Vector2 lerpP = Vector2.Lerp(p1, p2, t);
                Vector3 p3D = new Vector3(lerpP.x, Mathf.Lerp(p1Elev, p2Elev, t), lerpP.y);
                verts.Add(p3D);
                borderVerts.Add(verts.Count-1);
            }
        }
        int numPointsX = Mathf.CeilToInt(bounds.width * pointsPerUnit);
        int numPointsY = Mathf.CeilToInt(bounds.height * pointsPerUnit);

        Polygon[] triangleFan = new Polygon[points.Length];
        for (int i = 0; i < points.Length; i ++)
        {
            triangleFan[i] = new Polygon(new Vector2[] {points[i], points[(i+1)%points.Length], centroid});
        }

        float edgeBlendDist = 3f;

        for (int x = 0; x < numPointsX; x ++)
        {
            for (int y = 0; y < numPointsY; y++)
            {
                float xT = (float)x / (numPointsX - 1);
                float yT = (float)y / (numPointsY - 1);

                Vector2 realPos = new Vector2(xT * bounds.width + bounds.xMin, yT * bounds.height + bounds.yMin);
                float perimDist = footprint.DistToPerimeter(realPos);
                if (footprintPoly.ContainsPoint(realPos) && perimDist > 0.5f)
                {
                    float elevation = city.SampleElevation(realPos.x, realPos.y);

                    if (perimDist < edgeBlendDist)
                    {
                        for (int i = 0; i < triangleFan.Length; i++)
                        {
                            if (triangleFan[i].ContainsPoint(realPos))
                            {
                                Vector2 vert1 = triangleFan[i].points[0];
                                Vector2 vert2 = triangleFan[i].points[1];
                                Vector2 vert3 = triangleFan[i].points[2];

                                float elev1 = city.SampleElevation(vert1.x, vert1.y);
                                float elev2 = city.SampleElevation(vert2.x, vert2.y);
                                float elev3 = city.SampleElevation(vert3.x, vert3.y);

                                float blendElevation = HelperFunctions.TriangleInterp(realPos, vert1, vert2, vert3, elev1, elev2, elev3);
                                elevation = Mathf.Lerp(blendElevation, elevation, perimDist / edgeBlendDist);
                            }
                        } 
                    }
                    verts.Add(new Vector3(realPos.x, elevation, realPos.y));
                }
            }
        }
        return HelperFunctions.GetTriangulationMesh(verts.ToArray(), borderVerts, footprintPoly);
    }
}
