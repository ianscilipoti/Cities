using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Building
{
    private EdgeLoop<CityEdge> footprint;
    private Vector3[] basePoints;
    private float elevation;
    private float floorHeight; //height of each floor
    private int floors; //num floors
    private float foundationFirstFloorGap;//how tall is the foundation

    //mesh data
    //public List<Vector3> verts;
    //public List<Vector2> uvs;
    //public List<int> tris;
    public Dictionary<string, BuildingComponent> buildingCatagories; //walls, roof, trim, etc

    //catagories
    const string wallCat = "wall";
    const string trimCat = "trim";
    const string roofCat = "roof";

    private City city;

    public Building(EdgeLoop<CityEdge> footprint, float floorHeight, int floors, City city)
    {
        this.footprint = footprint;
        Vector2[] footprintPoints = footprint.GetPoints();
        basePoints = new Vector3[footprintPoints.Length];
        float highestBasePoint = float.MinValue;

        for (int i = 0; i < basePoints.Length; i++)
        {
            Vector2 fpPt = footprintPoints[i];
            basePoints[i] = new Vector3(fpPt.x, city.SampleElevation(fpPt.x, fpPt.y), fpPt.y);
            if (basePoints[i].y > highestBasePoint)
            {
                highestBasePoint = basePoints[i].y;
            }
        }

        this.foundationFirstFloorGap = 0.2f;
        this.elevation = highestBasePoint;
        this.floorHeight = floorHeight;
        this.floors = floors;
        this.city = city;
    }

    /// <summary>
    /// Adds a component to the building
    /// </summary>
    /// <param name="component"></param>
    /// <param name="catagory"></param>
    private void AddComponent(BuildingComponent component, string catagory)
    {
        buildingCatagories[catagory].verts.AddRange(component.verts);
        buildingCatagories[catagory].uvs.AddRange(component.uvs);
        int triIndexOffset = buildingCatagories[catagory].verts.Count;
        for (int i = 0; i < component.verts.Count; i++)
        {
            buildingCatagories[catagory].tris.Add(component.tris[i] + triIndexOffset);
        }
    }

    private void BuildWalls()
    {

    }

    public void PlaceBuilding()
    {
        GameObject buildingObject = new GameObject("Building", typeof(MeshRenderer), typeof(MeshFilter));
        Mesh mesh = GetMesh();
        buildingObject.GetComponent<MeshFilter>().mesh = mesh;
        buildingObject.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
        buildingObject.transform.parent = city.cityParent;
    }

    public Mesh GetMesh()
    {
        Vector2[] footprintVerts = footprint.GetPoints();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        int[,] floorLoopVerts = new int[floors + 2, footprintVerts.Length];

        for (int i = 0; i < floors + 2; i++)
        {
            float vertElev = elevation + floorHeight * (i - 1) + foundationFirstFloorGap;

            for (int j = 0; j < footprintVerts.Length; j++)
            {
                if (i == 0)
                {
                    vertElev = basePoints[j].y;
                }
                verts.Add(new Vector3(footprintVerts[j].x, vertElev, footprintVerts[j].y));
                floorLoopVerts[i, j] = verts.Count - 1;
            }
        }
        for (int i = 0; i < floors + 1; i++)
        {
            for (int j = 0; j < footprintVerts.Length; j++)
            {
                int nextI = (i + 1);
                int nextJ = (j + 1) % footprintVerts.Length;
                tris.Add(floorLoopVerts[i, j]);
                tris.Add(floorLoopVerts[nextI, nextJ]);
                tris.Add(floorLoopVerts[i, nextJ]);

                tris.Add(floorLoopVerts[i, j]);
                tris.Add(floorLoopVerts[nextI, j]);
                tris.Add(floorLoopVerts[nextI, nextJ]);

            }

        }

        Vector3 center = footprint.GetCenter();
        center = new Vector3(center.x, (floors + 1) * floorHeight + elevation + foundationFirstFloorGap, center.y);
        verts.Add(center);
        int centerVertIndex = verts.Count - 1;

        for (int j = 0; j < footprintVerts.Length; j++)
        {
            int nextJ = (j + 1) % footprintVerts.Length;
            tris.Add(floorLoopVerts[floors + 1, nextJ]);
            tris.Add(floorLoopVerts[floors + 1, j]);
            tris.Add(centerVertIndex);
        }


        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
}