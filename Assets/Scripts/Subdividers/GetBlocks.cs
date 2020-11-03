using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class GetBlocks : EdgeLoopSubdivider<CityEdge>
{
    private System.Object[] factoryParams;

    public GetBlocks(System.Object[] factoryParams)
    {
        this.factoryParams = factoryParams;
    }

    public override List<SubdividableEdgeLoop<CityEdge>> GetChildren(SubdividableEdgeLoop<CityEdge> parent)
    {
        Polygon parentPoly = parent.GetPolygon();
        Path polygonAsClip = parentPoly.ClipperPath(HelperFunctions.clipperScale);

        EdgeLoopEdge[] edges = parent.GetEdges();
        EdgeLoopEdge longestEdge = edges[0];
        float longestEdgeLength = 0;
        parent.EnumerateEdges((EdgeLoopEdge edge) =>
        {
            float edgeLength = Vector2.Distance(edge.a.pt, edge.b.pt);
            if (edgeLength > longestEdgeLength)
            {
                longestEdgeLength = edgeLength;
                longestEdge = edge;
            }
        });

        Vector2 edgeDirection = longestEdge.b.pt - longestEdge.a.pt;

        float angle = Mathf.Atan2(edgeDirection.y, edgeDirection.x) * Mathf.Rad2Deg;
        Rect bounds = parentPoly.bounds;
        float maxDimension = Mathf.Max(bounds.width, bounds.height);
        bounds.width = maxDimension;
        bounds.height = maxDimension;

        Rect expandedBounds = new Rect(bounds.center - bounds.size * 0.55f, bounds.size * 1.1f);

        ILinkedGraphEdgeFactory<CityEdge> factory = new CityEdgeFactory();
        List<DividingEdge> edgePaths = new List<DividingEdge>();
        Vector2 centroid = parentPoly.centroid;

        float relativeBoundAngle = 0f;
        Rect parentRotatedBounds = HelperFunctions.GetOrientedBounds(new List<Vector2>(parentPoly.points), ref relativeBoundAngle);

        float rotation = relativeBoundAngle * Mathf.Rad2Deg;

        if (parentRotatedBounds.width > parentRotatedBounds.height * 0.7f)
        {
            //edgePaths.Add(new DividingEdge((centroid - Vector2.right * 1000f).RotatedAround(centroid, rotation), (centroid + Vector2.right * 1000f).RotatedAround(centroid, rotation), factory, factoryParams));
            edgePaths.Add(new DividingEdge((centroid - Vector2.up * 1000f).RotatedAround(centroid, rotation), (centroid + Vector2.up * 1000f).RotatedAround(centroid, rotation), factory, factoryParams));
        }
        if (parentRotatedBounds.height > parentRotatedBounds.width * 0.7f)
        {
            //edgePaths.Add(new DividingEdge((centroid - Vector2.up * 1000f).RotatedAround(centroid, rotation), (centroid + Vector2.up * 1000f).RotatedAround(centroid, rotation), factory, factoryParams));
            edgePaths.Add(new DividingEdge((centroid - Vector2.right * 1000f).RotatedAround(centroid, rotation), (centroid + Vector2.right * 1000f).RotatedAround(centroid, rotation), factory, factoryParams));
        }
        return CollectChildren(parent, edgePaths);
    }
}