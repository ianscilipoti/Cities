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
    public int columns;
    public int rows;

    public GetBlocks (int columns, int rows)
    {
        this.columns = columns;
        this.rows = rows;
    }

    public override List<SubdividableEdgeLoop<CityEdge>> GetChildren (SubdividableEdgeLoop<CityEdge> parent) 
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
        Rect expandedBounds = new Rect(bounds.center - bounds.size*2, bounds.size * 4);

        List<DividingEdge> edgePaths = GetGridDividers(bounds, 0f);

        return CollectChildren(parent, edgePaths);
    }

    private List<DividingEdge> GetGridDividers (Rect bounds, float rotDegrees)
    {
        Vector2 boundCenter = bounds.center;
        Paths allCells = new Paths();
        float cellWidth = bounds.width / columns;
        float cellHeight = bounds.height / rows;

        float clipperScale = HelperFunctions.clipperScale;

        List<DividingEdge> allDividers = new List<DividingEdge>();

        ILinkedGraphEdgeFactory<CityEdge> factory = new CityEdgeFactory();

        for (int x = 0; x < columns; x++)
        {
            Vector2 bottom = new Vector2(GetColumnPosition(x, cellWidth, bounds), bounds.yMin).RotatedAround(boundCenter, rotDegrees);
            Vector2 top = new Vector2(GetColumnPosition(x, cellWidth, bounds), bounds.yMax).RotatedAround(boundCenter, rotDegrees);

            allDividers.Add(new DividingEdge(top, bottom, factory, CityEdgeType.LandPath));
        }
        for (int y = 0; y < rows; y++)
        {
            Vector2 bottom = new Vector2(bounds.xMin, GetRowPosition(y, cellHeight, bounds)).RotatedAround(boundCenter, rotDegrees);
            Vector2 top = new Vector2(bounds.xMax, GetRowPosition(y, cellHeight, bounds)).RotatedAround(boundCenter, rotDegrees);

            allDividers.Add(new DividingEdge(top, bottom, factory, CityEdgeType.LandPath));
        }


        return allDividers;
    }

    private float GetColumnPosition (int x, float cellWidth, Rect bounds) 
    {
        return x * cellWidth + bounds.xMin;
    }

    private float GetRowPosition(int y, float cellHeight, Rect bounds)
    {
        return y * cellHeight + bounds.yMin;
    }
}
