using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class GetPieSections <EdgeType> : EdgeLoopSubdivider<EdgeType> where EdgeType : EdgeLoopEdge
{
    private ILinkedGraphEdgeFactory<EdgeType> factory;
    private System.Object[] factoryParams;

    public GetPieSections (ILinkedGraphEdgeFactory<EdgeType> factory, System.Object[] factoryParams)
    {
        this.factory = factory;
        this.factoryParams = factoryParams;
    }

    public override List<SubdividableEdgeLoop<EdgeType>> GetChildren(SubdividableEdgeLoop<EdgeType> parent)
    {
        Vector2[] simplifiedPoints = parent.GetSimplifiedPoints(1f * Mathf.Deg2Rad);
        Polygon parentPoly = new Polygon(simplifiedPoints);
        Vector2 centroid = parentPoly.centroid;
        List<Vector2> edgeCrossings = new List<Vector2>();
        parentPoly.EnumerateEdges((Edge edge) =>
        {
            Vector2 edgeCrossing = HelperFunctions.ScaleFrom(Vector2.Lerp(edge.a, edge.b, Random.Range(0.4f, 0.6f)), centroid, 1.5f);
            edgeCrossings.Add(edgeCrossing);
            //points.Add(HelperFunctions.ScaleFrom(edgeCrossing, centroid, 5));
        });

        //build a list of dividing edges and pass it to the child collector
        List<DividingEdge> dividingEdges = new List<DividingEdge>();

        foreach (Vector2 crossing in edgeCrossings)
        {
            dividingEdges.Add(new DividingEdge(crossing, centroid, factory, factoryParams));
        }

        return CollectChildren(parent, dividingEdges);
    }
}
