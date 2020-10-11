using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class Divide <EdgeType> : EdgeLoopSubdivider<EdgeType> where EdgeType : EdgeLoopEdge
{
    private ILinkedGraphEdgeFactory<EdgeType> factory;
    private System.Object[] factoryParams;

    public Divide (ILinkedGraphEdgeFactory<EdgeType> factory, System.Object[] factoryParams)
    {
        this.factory = factory;
        this.factoryParams = factoryParams;
    }

    public override List<SubdividableEdgeLoop<EdgeType>> GetChildren(SubdividableEdgeLoop<EdgeType> parent)
    {

        Polygon parentPoly = new Polygon(parent.GetSimplifiedPoints(1f * Mathf.Deg2Rad));

        //build a list of dividing edges and pass it to the child collector
        List<DividingEdge> dividingEdges = new List<DividingEdge>();

        Vector2 center = parentPoly.centroid;
        float width = parentPoly.bounds.width;

        Vector2 a = center + Vector2.right * width;
        Vector2 b = center - Vector2.right * width;

        dividingEdges.Add(new DividingEdge(a, b, factory, factoryParams));

        return CollectChildren(parent, dividingEdges);
    }
}
