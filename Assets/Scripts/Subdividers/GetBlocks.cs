using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using EPPZ.Geometry;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class GetBlocks : ISubDivScheme
{
    public int columns;
    public int rows;

    public GetBlocks (int columns, int rows)
    {
        this.columns = columns;
        this.rows = rows;
    }

    public List<ISubdividable> GetChildren (ISubdividable parent, out List<Vector4> edges) 
    {
        List<ISubdividable> children = new List<ISubdividable>();
        Path polygonAsClip = ((Polygon)parent).ClipperPath(HelperFunctions.clipperScale);

        Edge longestEdge = parent.edges[0];
        float longestEdgeLength = 0;
        parent.EnumerateEdges((Edge edge) =>
        {
            float edgeLength = Vector2.Distance(edge.a, edge.b);
            if (edgeLength > longestEdgeLength) 
            {
                longestEdgeLength = edgeLength;
                longestEdge = edge;
            }
        });

        Vector2 edgeDirection = longestEdge.b - longestEdge.a;

        float angle = Mathf.Atan2(edgeDirection.y, edgeDirection.x) * Mathf.Rad2Deg;
        Rect bounds = parent.bounds;
        Rect expandedBounds = new Rect(bounds.center - bounds.size*2, bounds.size * 4);

        Paths edgePaths = new Paths();//paths forming edges between block areas

        Paths gridPaths = GetGridPaths(bounds, columns, rows, 0f, out edgePaths);

        Paths solution = new Paths();

        foreach (Path cell in gridPaths)
        {

            Paths thisSolution = new Paths();
            Clipper clipper = new Clipper();
            clipper.AddPath(cell, PolyType.ptSubject, true);
            clipper.AddPath(polygonAsClip, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, thisSolution);
            //possibility that this op produces more than one region. So we add all to final solution
            foreach (Path thisSolutionPart in thisSolution)
            {
                solution.Add(thisSolutionPart);
            }
        }

        foreach (Path solutionPath in solution)
        {
            children.Add(parent.GetNextChild(ClipperAddOns.PointsFromClipperPath(solutionPath, HelperFunctions.clipperScale)));
        }

        Paths clippedEdgePaths = new Paths();
        //---------get edges
        foreach (Path edgePath in edgePaths)
        {
            PolyTree clippedResults = new PolyTree();
            Clipper clipper = new Clipper();

            clipper.AddPath(edgePath, PolyType.ptSubject, false);
            clipper.AddPath(polygonAsClip, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, clippedResults);

            clippedEdgePaths.AddRange(Clipper.OpenPathsFromPolyTree(clippedResults));
        }
        edges = new List<Vector4>();
        foreach (Path edgePath in clippedEdgePaths)
        {
            for (int i = 0; i < edgePath.Count - 1; i++)
            {
                Vector2 p1 = HelperFunctions.GetPoint(edgePath[i]);
                Vector2 p2 = HelperFunctions.GetPoint(edgePath[i + 1]);
                edges.Add(new Vector4(p1.x, p1.y, p2.x, p2.y));
            }
        }

        return children;
    }

    private Paths GetGridPaths (Rect bounds, int columns, int rows, float rotDegrees, out Paths edgePaths)
    {
        Vector2 boundCenter = bounds.center;
        Paths allCells = new Paths();
        float cellWidth = bounds.width / columns;
        float cellHeight = bounds.height / rows;

        float clipperScale = HelperFunctions.clipperScale;

        edgePaths = new Paths();

        for (int x = 0; x < columns; x++)
        {
            Vector2 bottom = new Vector2(GetColumnPosition(x, cellWidth, bounds), bounds.yMin).RotatedAround(boundCenter, rotDegrees);
            Vector2 top = new Vector2(GetColumnPosition(x, cellWidth, bounds), bounds.yMax).RotatedAround(boundCenter, rotDegrees);
            Path edgePath = new Path();
            edgePath.Add(HelperFunctions.GetIntPoint(bottom));
            edgePath.Add(HelperFunctions.GetIntPoint(top));

            edgePaths.Add(edgePath);
        }
        for (int y = 0; y < rows; y++)
        {
            Vector2 bottom = new Vector2(bounds.xMin, GetRowPosition(y, cellHeight, bounds)).RotatedAround(boundCenter, rotDegrees);
            Vector2 top = new Vector2(bounds.xMax, GetRowPosition(y, cellHeight, bounds)).RotatedAround(boundCenter, rotDegrees);
            Path edgePath = new Path();
            edgePath.Add(HelperFunctions.GetIntPoint(bottom));
            edgePath.Add(HelperFunctions.GetIntPoint(top));

            edgePaths.Add(edgePath);
        }

        for (int x = 0; x < columns; x ++)
        {
            for (int y = 0; y < rows; y ++)
            {
                Vector2 bottomLeftPos = new Vector2(GetColumnPosition(x, cellWidth, bounds), GetRowPosition(y, cellHeight, bounds));
                Path cellPath = new Path();
                Vector2 bl = new Vector2(bottomLeftPos.x, bottomLeftPos.y).RotatedAround(boundCenter, rotDegrees);
                Vector2 br = new Vector2(bottomLeftPos.x + cellWidth, bottomLeftPos.y).RotatedAround(boundCenter, rotDegrees);
                Vector2 tr = new Vector2(bottomLeftPos.x + cellWidth, bottomLeftPos.y + cellHeight).RotatedAround(boundCenter, rotDegrees);
                Vector2 tl = new Vector2(bottomLeftPos.x, bottomLeftPos.y + cellHeight).RotatedAround(boundCenter, rotDegrees);

                cellPath.Add(HelperFunctions.GetIntPoint(bl));
                cellPath.Add(HelperFunctions.GetIntPoint(br));
                cellPath.Add(HelperFunctions.GetIntPoint(tr));
                cellPath.Add(HelperFunctions.GetIntPoint(tl));

                allCells.Add(cellPath);
            }
        }
        return allCells;
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
