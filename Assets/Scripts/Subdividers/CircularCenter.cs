using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class CircularCenter <EdgeType> : EdgeLoopSubdivider<EdgeType> where EdgeType : EdgeLoopEdge
{
    private ILinkedGraphEdgeFactory<EdgeType> factory;
    private System.Object[] factoryParams;

    public CircularCenter (ILinkedGraphEdgeFactory<EdgeType> factory, System.Object[] factoryParams)
    {
        this.factory = factory;
        this.factoryParams = factoryParams;
    }

    public override List<SubdividableEdgeLoop<EdgeType>> GetChildren(SubdividableEdgeLoop<EdgeType> parent)
    {
        Vector2[] simplifiedPoints = parent.GetSimplifiedPoints(1f * Mathf.Deg2Rad);
        Polygon parentPoly = new Polygon(simplifiedPoints);
        Vector2 centroid = parentPoly.centroid;

        float centroidDistance = parent.DistToPerimeter(centroid);

        float circleRadius = centroidDistance / 2f;

        int circleResolution = Random.Range(3, 8);
        int numRays = Mathf.CeilToInt(circleResolution / 3f);

        Vector2[] circlePoints = new Vector2[circleResolution];
        for (int i = 0; i < circleResolution; i ++)
        {
            float angle = (i / (float)circleResolution) * Mathf.PI * 2;
            circlePoints[i] = centroid + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * circleRadius;
        }

        //build a list of dividing edges and pass it to the child collector
        List<DividingEdge> dividingEdges = new List<DividingEdge>();
        for (int i = 0; i < circleResolution; i++)
        {
            dividingEdges.Add(new DividingEdge(circlePoints[i], circlePoints[(i+1)%circleResolution], factory, factoryParams));

            Vector2 extended = (circlePoints[i] - centroid) * 100f + centroid;
            dividingEdges.Add(new DividingEdge(circlePoints[i], extended, factory, factoryParams));
        }

        return CollectChildren(parent, dividingEdges);
    }
}
