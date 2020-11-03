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
    GameObject[] treeModels;

    public LandBuilder (EdgeLoop<CityEdge> footprint, City city)
    {
        this.footprint = footprint;
        this.city = city;
        pointsPerUnit = 0.3f;
        treeModels = new GameObject[] {Resources.Load<GameObject>("Trees/Pine1"), Resources.Load<GameObject>("Trees/Pine2") };
    }

    public void PlaceLand()
    {
        GameObject buildingObject = new GameObject("LandPlot", typeof(MeshRenderer), typeof(MeshFilter));
        Mesh mesh = GetMesh();
        buildingObject.GetComponent<MeshFilter>().mesh = mesh;
        buildingObject.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Grass");
        buildingObject.transform.parent = city.cityParent;

        float treeDensity = 0.015f;
        Polygon footPrintPoly = footprint.GetPolygon();
        int numTrees = Mathf.RoundToInt(treeDensity * footPrintPoly.area);
        int numPlaced = 0;
        while(numPlaced < numTrees)
        {
            Rect bounds = footprint.GetBounds();
            Vector2 position2D = new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
            if (footPrintPoly.ContainsPoint(position2D))
            {
                numPlaced++;
                Vector3 position3D = new Vector3(position2D.x, city.SampleElevation(position2D.x, position2D.y) - 0.2f, position2D.y);
                GameObject newTree = Object.Instantiate(treeModels[(int)(treeModels.Length * Random.value)], position3D, Quaternion.Euler(0, Random.value * 360f, 0), city.cityParent);
                newTree.transform.localScale = Vector3.one * Random.Range(0.6f, 1.3f);
            }
        }
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
