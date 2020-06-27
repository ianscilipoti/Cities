﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class CitySkeleton : ISubDivScheme <SubdividableEdgeLoop>
{
    private Vector2[] cityEntrences;
    private int potentialRoadPoints;

    public CitySkeleton (Vector2[] cityEntrences, int potentialRoadPoints) {
        this.cityEntrences = cityEntrences;
        this.potentialRoadPoints = potentialRoadPoints;
    }


    public List<Subdividable> GetChildren(SubdividableEdgeLoop parent)
    {
        Polygon parentPoly = parent.GetPolygon();
        //generate points of interest
        List<RoadDestination> pointsOfInterest = new List<RoadDestination>();
        Vector2 centroid = parent.GetCenter();
        parent.EnumerateEdges((EdgeLoopEdge edge) =>
        {
            pointsOfInterest.Add(new RoadDestination(HelperFunctions.ScaleFrom(Vector2.Lerp(edge.a.pt, edge.b.pt, Random.Range(0.2f, 0.8f)), centroid, 5), 1, false, true));
        });
        Rect bounds = parent.GetBounds();
        int potentialRoadPointsRt = Mathf.CeilToInt(Mathf.Sqrt(potentialRoadPoints));

        for (int x = 0; x < potentialRoadPointsRt; x++)
        {
            for (int y = 0; y < potentialRoadPointsRt; y++)
            {
                Vector2 point = new Vector2((x / (float)potentialRoadPointsRt) * bounds.width + bounds.xMin,
                                            (y / (float)potentialRoadPointsRt) * bounds.height + bounds.yMin);
                float distBtwnPts = (bounds.width + bounds.height) / (potentialRoadPoints * 2);
                point = point + new Vector2(Random.Range(-1f, 1f), Random.Range(-1, 1f)) * distBtwnPts * 0.8f;
                if (parentPoly.ContainsPoint(point))
                {
                    pointsOfInterest.Add(new RoadDestination(point, 0, false, false));
                }
            } 
        }

        //triangulate points of interest to get potential road segments
        TriangleNet.Geometry.Polygon polygon = new TriangleNet.Geometry.Polygon();
        Dictionary<TriangleNet.Geometry.Vertex, RoadDestination> vertexDestMap = new Dictionary<TriangleNet.Geometry.Vertex, RoadDestination>();

        foreach (RoadDestination dest in pointsOfInterest)
        {
            TriangleNet.Geometry.Vertex vert = new TriangleNet.Geometry.Vertex(dest.point.x, dest.point.y);
            vertexDestMap.Add(vert, dest);
            polygon.Add(vert);
        }
        TriangleNet.Meshing.ConstraintOptions options =
            new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        TriangleNet.Meshing.GenericMesher mesher = new TriangleNet.Meshing.GenericMesher();
        TriangleNet.Meshing.IMesh mesh = mesher.Triangulate(polygon);

        Path polygonAsClip = parentPoly.ClipperPath(HelperFunctions.clipperScale);
        Paths solution = new Paths();

        List<Subdividable> children = new List<Subdividable>();

        foreach (TriangleNet.Topology.Triangle tri in mesh.Triangles) 
        {
            Path triPath = new Path();
            for (int j = 0; j < 3; j++)
            {
                TriangleNet.Geometry.Vertex vert = tri.GetVertex(j);
                triPath.Add(new IntPoint((int)(vert[0] * HelperFunctions.clipperScale), (int)(vert[1] * HelperFunctions.clipperScale)));
            }

            Paths thisSolution = new Paths();
            Clipper clipper = new Clipper();
            clipper.AddPath(triPath, PolyType.ptSubject, true);
            clipper.AddPath(polygonAsClip, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, thisSolution);
            //possibility that this op produces more than one region. So we add all to final solution
            foreach (Path thisSolutionPart in thisSolution)
            {
                solution.Add(thisSolutionPart);
            }
        }

        //foreach (Path solutionPath in solution) 
        //{
        //    children.Add(parent.GetNextChild(ClipperAddOns.PointsFromClipperPath(solutionPath, HelperFunctions.clipperScale)));
        //}

        ////get vertices as list
        //ICollection<TriangleNet.Geometry.Vertex> vertices = mesh.Vertices;
        //TriangleNet.Geometry.Vertex[] vertexList = new TriangleNet.Geometry.Vertex[vertices.Count];
        //vertices.CopyTo(vertexList, 0);
        //IEnumerable<TriangleNet.Geometry.Edge> meshEdges = mesh.Edges;

        //Paths edgePaths = new Paths();
        //foreach (TriangleNet.Geometry.Edge edge in meshEdges) {
        //    Vector2 a = new Vector2((float)vertexList[edge.P0].X, (float)vertexList[edge.P0].Y);
        //    Vector2 b = new Vector2((float)vertexList[edge.P1].X, (float)vertexList[edge.P1].Y);

        //    Path edgePath = new Path();
        //    edgePath.Add(HelperFunctions.GetIntPoint(a));
        //    edgePath.Add(HelperFunctions.GetIntPoint(b));

        //    PolyTree clippedResults = new PolyTree();
        //    Clipper clipper = new Clipper();

        //    clipper.AddPath(edgePath, PolyType.ptSubject, false);
        //    clipper.AddPath(polygonAsClip, PolyType.ptClip, true);
        //    clipper.Execute(ClipType.ctIntersection, clippedResults);

        //    edgePaths.AddRange(Clipper.OpenPathsFromPolyTree(clippedResults));
        //}

        //edges = new List<Vector4>();
        //foreach (Path edgePath in edgePaths)
        //{
        //    for (int i = 0; i < edgePath.Count-1; i ++)
        //    {
        //        Vector2 p1 = HelperFunctions.GetPoint(edgePath[i]);
        //        Vector2 p2 = HelperFunctions.GetPoint(edgePath[i+1]);
        //        edges.Add(new Vector4(p1.x, p1.y, p2.x, p2.y));
        //    }
        //}

        return new List<Subdividable>();
    }

    struct RoadDestination
    {
        public Vector2 point;
        public int priority;
        public bool entrence;
        public bool edge;

        public RoadDestination(Vector2 point, int priority, bool entrence, bool edge)
        {
            this.point = point;
            this.priority = priority;
            this.entrence = entrence;
            this.edge = edge;
        }
    }
}