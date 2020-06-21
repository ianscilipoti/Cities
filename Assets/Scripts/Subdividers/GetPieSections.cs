using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using EPPZ.Geometry.AddOns;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class GetPieSections : ISubDivScheme
{
    public GetPieSections () {
    }

    public List<ISubdividable> GetChildren(ISubdividable parent, out List<Vector4> edges)
    {
        //generate points of interest
        List<Vector2> points = new List<Vector2>();
        Vector2 centroid = parent.centroid;
        List<Vector2> edgeCrossings = new List<Vector2>();
        parent.EnumerateEdges((Edge edge) =>
        {
            Vector2 edgeCrossing = Vector2.Lerp(edge.a, edge.b, Random.Range(0.4f, 0.6f));
            edgeCrossings.Add(edgeCrossing);
            points.Add(HelperFunctions.ScaleFrom(edgeCrossing, centroid, 5));
        });
        points.Add(centroid);

        //triangulate points of interest to get potential road segments
        TriangleNet.Geometry.Polygon polygon = new TriangleNet.Geometry.Polygon();
        foreach (Vector2 pt in points)
        {
            TriangleNet.Geometry.Vertex vert = new TriangleNet.Geometry.Vertex(pt.x, pt.y);
            polygon.Add(vert);
        }

        TriangleNet.Meshing.ConstraintOptions options =
            new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        TriangleNet.Meshing.GenericMesher mesher = new TriangleNet.Meshing.GenericMesher();
        TriangleNet.Meshing.IMesh mesh = mesher.Triangulate(polygon);

        Path polygonAsClip = parent.ClipperPath(HelperFunctions.clipperScale);
        Paths solution = new Paths();

        List<ISubdividable> children = new List<ISubdividable>();

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

        foreach (Path solutionPath in solution) 
        {
            children.Add(parent.GetNextChild(ClipperAddOns.PointsFromClipperPath(solutionPath, HelperFunctions.clipperScale)));
        }

        edges = new List<Vector4>();
        foreach (Vector2 crossing in edgeCrossings)
        {
            edges.Add(new Vector4(crossing.x, crossing.y, centroid.x, centroid.y));
        }

        return children;
    }
}
