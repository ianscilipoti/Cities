using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

public class LinkedGraph<EdgeType> where EdgeType : LinkedGraphEdge
{

    public const float VERT_MERGE_DIST_SQR = 0.001f;

    public delegate bool SearchFilter(LinkedGraphEdge theEdge);

    public static EdgeType AddEdge(LinkedGraphVertex aVert, LinkedGraphVertex bVert, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, List<EdgeType> knownEdges)
    {
        EdgeType newEdge = edgeFactory.GetEdge(aVert, bVert);

        if(knownEdges != null)
        {
            knownEdges.Add(newEdge); 
        }

        return newEdge;
    }

    public static void DebugDraw (List<EdgeType> edges)
    {

        foreach (EdgeType edge in edges)
        {
            int colorCode = edge.GetHashCode();
            Random.InitState(colorCode);

            Debug.DrawLine(HelperFunctions.projVec2(edge.a.pt), HelperFunctions.projVec2(edge.b.pt), Random.ColorHSV());
        }
    }

    //disconnect this edge. Leaving it for the GC :(
    public static void Detach(EdgeType edge)
    {
        edge.a.RemoveConnection(edge);
        edge.b.RemoveConnection(edge);
    }

    public static void SubdivideEdge(EdgeType edge, LinkedGraphVertex midPoint, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, List<EdgeType> knownEdges)
    {
        LinkedGraphVertex aVert = edge.a;
        LinkedGraphVertex bVert = edge.b;

        EdgeType aEdge = AddEdge(aVert, midPoint, edgeFactory, knownEdges);
        EdgeType bEdge = AddEdge(midPoint, bVert, edgeFactory, knownEdges);

        edge.OnEdgeSplit(aEdge, bEdge);

        knownEdges.Remove(edge);
        Detach(edge);
    }

    //given a list of edges that may intersect with the new edge, connect the new edge
    public static void ConnectNewEdge(Vector2 a, Vector2 b, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, List<EdgeType> knownEdges)
    {

        LinkedGraphVertex aVert = HasVertex(knownEdges, a);
        LinkedGraphVertex bVert = HasVertex(knownEdges, b);

        if (aVert == null)
        {
            aVert = new LinkedGraphVertex(a);
        }
        if (bVert == null)
        {
            bVert = new LinkedGraphVertex(b);
        }

        if (knownEdges == null || knownEdges.Count == 0)
        {
            AddEdge(aVert, bVert, edgeFactory, knownEdges);
            return;
        }

        //use for checking intersections
        Segment testingSegment = Segment.SegmentWithPoints(a, b);
        Rect testingSegBounds = testingSegment.ExpandedBounds(VERT_MERGE_DIST_SQR);

        List<LinkedGraphVertex> intersectionVertices = new List<LinkedGraphVertex>();
        intersectionVertices.Add(aVert);//these will be sorted later
        intersectionVertices.Add(bVert);

        EdgeType[] edgesCopy = new EdgeType[knownEdges.Count];
        knownEdges.CopyTo(edgesCopy);

        //check if new vertices are on another line
        foreach (EdgeType seg in edgesCopy)
        {

            if (!seg.segRef.bounds.Overlaps(testingSegBounds))
            {
                continue;
            }
            if (seg.a == aVert || seg.a == bVert || seg.b == aVert || seg.b == bVert)
            {
                //do nothing in this case. 
                //this is captured in the base case of finding existing verts
            }
            else if (seg.segRef.ContainsPoint(a, VERT_MERGE_DIST_SQR))
            {
                SubdivideEdge(seg, aVert, edgeFactory, knownEdges);
            }
            else if (seg.segRef.ContainsPoint(b, VERT_MERGE_DIST_SQR))
            {
                SubdivideEdge(seg, bVert, edgeFactory, knownEdges);
            }
            else if (testingSegment.ContainsPoint(seg.a.pt))
            {
                intersectionVertices.Add(seg.a);
            }
            else if (testingSegment.ContainsPoint(seg.b.pt))
            {
                intersectionVertices.Add(seg.b);
            }
            else
            {
                Vector2 intersectionPoint = Vector2.zero;
                if (seg.segRef.IntersectionWithSegment(testingSegment, out intersectionPoint))
                {
                    LinkedGraphVertex midPtVert = new LinkedGraphVertex(intersectionPoint);
                    SubdivideEdge(seg, midPtVert, edgeFactory, knownEdges);
                    intersectionVertices.Add(midPtVert);
                }
            }
        }
        Vector2 sortDirection = b - a;

        intersectionVertices.Sort((first, second) =>
                                  (first.pt - a).sqrMagnitude.CompareTo(
                                      (second.pt - a).sqrMagnitude));

        for (int i = 0; i < intersectionVertices.Count - 1; i++)
        {
            AddEdge(intersectionVertices[i], intersectionVertices[i + 1], edgeFactory, knownEdges);
        }
    }

    //given a list of edges, check if the vertex at pt already exists
    public static LinkedGraphVertex HasVertex(List<EdgeType> inEdges, Vector2 pt)
    {
        LinkedGraphVertex matchingVert = null;
        EnumerateVertices(inEdges, (LinkedGraphVertex vert) =>
        {
            if ((vert.pt - pt).sqrMagnitude < VERT_MERGE_DIST_SQR)
            {
                matchingVert = vert;
            }
        });
        return matchingVert;
    }

    //perform action on all vertices
    public static void EnumerateVertices(List<EdgeType> edges, System.Action<LinkedGraphVertex> action)
    {
        // Enumerate local points.
        foreach (LinkedGraphEdge eachEdge in edges)
        {
            action(eachEdge.a);
            action(eachEdge.b);
        }
    }
}
