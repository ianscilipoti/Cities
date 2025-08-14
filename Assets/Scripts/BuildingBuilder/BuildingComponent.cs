using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingComponent
{
    public List<Vector3> verts;
    public List<Vector2> uvs;
    public List<int> tris;

    public BuildingComponent (List<Vector3> verts, List<Vector2> uvs, List<int> tris)
    {
        this.verts = verts;
        this.uvs = uvs;
        this.tris = tris;
    }

    public BuildingComponent(Vector3[] verts, Vector2[] uvs, int[] tris)
    {
        this.verts = new List<Vector3>(verts);
        this.uvs = new List<Vector2>(uvs);
        this.tris = new List<int>(tris);
    }

    //static BuildingComponent GetQuad (Vector3[] corners)
    //{
    //    if (corners.Length != 4)
    //    {
    //        Debug.LogError("More/less than 4 corners. Not a quad.");
    //        return null;
    //    }
    //    Vector2[] uvs = new Vector2[4];
    //    Vector3 vector3
    //}

    //public BuildingComponent ()
    //{
    //    this.verts = new List<Vector3>();
    //    this.uvs = new List<Vector2>();
    //    this.tris = new List<int>();
    //}
}
