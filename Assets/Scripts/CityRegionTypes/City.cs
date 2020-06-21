using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class City : CityRegion
{

    public Vector2 entrence;

    public City (Vector2[] boundary) : base(boundary, null, true) 
    {
        entrence = (edges[0].a + edges[0].b) / 2;
    }

    public override Color getDebugColor()
    {
        return Color.red;
    }

    public override ISubdividable GetNextChild (Vector2[] boundary) 
    {
        return new Block(boundary, this);
    }

    //Cities always subdivide with citySkeleton
    //this function could randomize what subdivscheme is returned easily
    public override ISubDivScheme GetDivScheme () {
        return new CitySkeleton(new Vector2[]{entrence}, 16);
    } 

    public SegmentGraph<float> GetBoundaryGraph () 
    {
        SegmentGraph<float> roadGraph = new SegmentGraph<float>();
        List<Vector4> allEdges = CollectEdges();
        double test = Time.realtimeSinceStartup;
        foreach (Vector4 edge in allEdges)
        {
            roadGraph.AddSegment(new Vector2(edge.x, edge.y), new Vector2(edge.z, edge.w), 0f);
        }
        Debug.Log(Time.realtimeSinceStartup - test);
        return roadGraph;
    }
}
