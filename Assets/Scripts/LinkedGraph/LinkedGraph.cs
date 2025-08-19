using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

/*
 * LinkedGraph is a template 
 *
 */

public class LinkedGraph<EdgeType> where EdgeType : LinkedGraphEdge
{

    public const float VERT_MERGE_DIST_SQR = 0.001f;

    public delegate bool SearchFilter(LinkedGraphEdge theEdge);

    public static EdgeType AddEdge(LinkedGraphVertex aVert, LinkedGraphVertex bVert, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, List<EdgeType> knownEdges)
    {
        return AddEdge(aVert, bVert, edgeFactory, null, knownEdges);
    }

    public static EdgeType AddEdge(LinkedGraphVertex aVert, LinkedGraphVertex bVert, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, System.Object[] factoryParams, List<EdgeType> knownEdges)
    {
        EdgeType newEdge = edgeFactory.GetEdge(aVert, bVert, factoryParams);

        if(knownEdges != null)
        {
            knownEdges.Add(newEdge); 
        }

        return newEdge;
    }

    public static EdgeType AddEdge(EdgeType newEdgeInstance, List<EdgeType> knownEdges)
    {
        if (knownEdges != null)
        {
            knownEdges.Add(newEdgeInstance);
        }
        return newEdgeInstance;
    }

    public static void DebugDraw(List<EdgeType> edges)
    {
        DebugDraw(edges, null, null);
    }

    public static void DebugDraw (List<EdgeType> edges, List<Color> colors, List<float> elevations)
    {

        for (int i = 0; i < edges.Count; i ++)
        {
            EdgeType edge = edges[i];
            Color col = Color.white;
            float verticalOffset = 0f;
            if (colors != null)
            {
                col = colors[i];
            }
            if (elevations != null)
            {
                verticalOffset = elevations[i];
            }
            Debug.DrawLine(HelperFunctions.projVec2(edge.a.pt) + Vector3.up * verticalOffset, HelperFunctions.projVec2(edge.b.pt) + Vector3.up * verticalOffset, col);
            Random.InitState(edge.GetHashCode());
            Vector3 center = HelperFunctions.projVec2(edge.a.pt) / 2 + HelperFunctions.projVec2(edge.b.pt) / 2;

            Debug.DrawLine(center, center + new Vector3(Random.Range(-0.1f, 0.1f), -1f, 0), new Color(1f, 0.1f, 1f));
            DebugVert(edge.a);
            DebugVert(edge.b);
        }
    }

    private static void DebugVert (LinkedGraphVertex vert)
    {
        Random.InitState(vert.GetHashCode());
        Color vCol = Color.red;
        float len = 2f;
        if (vert.connections.Count == 1)
        {
            len = 10f;
        }
        else if (vert.connections.Count == 2)
        {
            vCol = Color.green;
        }
        else if (vert.connections.Count == 3)
        {
            vCol = Color.yellow;
        }
        else if (vert.connections.Count == 4)
        {
            vCol = Color.black;
        }
        else if (vert.connections.Count == 5)
        {
            vCol = Color.white;
        }
        else if (vert.connections.Count > 5)
        {
            vCol = Color.gray;
        }
        Debug.DrawLine(HelperFunctions.projVec2(vert.pt), HelperFunctions.projVec2(vert.pt) + (Vector3.up + new Vector3(Random.Range(-0.3f, 0.3f), 0, 0)) * len, vCol);
    }

    //disconnect this edge. Leaving it for the GC :(
    public static void Detach(EdgeType edge)
    {
        bool removeA = edge.a.RemoveConnection(edge);
        bool removeB = edge.b.RemoveConnection(edge);

        if (!removeA || !removeB)
        {
            Debug.Log("Failed remove");
        }
    }

    public static void SubdivideEdge(EdgeType edge, LinkedGraphVertex midPoint, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, List<EdgeType> knownEdges)
    {
        LinkedGraphVertex aVert = edge.a;
        LinkedGraphVertex bVert = edge.b;

        EdgeType aEdge = AddEdge(edgeFactory.GetEdgeFromParent(aVert, midPoint, null, edge), knownEdges);
        EdgeType bEdge = AddEdge(edgeFactory.GetEdgeFromParent(midPoint, bVert, null, edge), knownEdges);

        //EdgeType aEdge = AddEdge(aVert, midPoint, edgeFactory, null, knownEdges);
        //EdgeType bEdge = AddEdge(midPoint, bVert, edgeFactory, null, knownEdges);

        edge.OnEdgeSplit(aEdge, bEdge);

        knownEdges.Remove(edge);
        Detach(edge);
    }

    public static void ConnectNewEdge(Vector2 a, Vector2 b, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, List<EdgeType> knownEdges)
    {
        ConnectNewEdge(a, b, edgeFactory, null, knownEdges);
    }

    //given a list of edges that may intersect with the new edge, connect the new edge
    public static void ConnectNewEdge(Vector2 a, Vector2 b, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, System.Object[] factoryParams, List<EdgeType> knownEdges)
    {

        LinkedGraphVertex aVert = HasVertex(knownEdges, a);
        LinkedGraphVertex bVert = HasVertex(knownEdges, b);

        if (aVert != null && bVert != null && aVert == bVert || (a - b).sqrMagnitude < VERT_MERGE_DIST_SQR)
        {
            return;
        }

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
            AddEdge(aVert, bVert, edgeFactory, factoryParams, knownEdges);
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
            else if (testingSegment.ContainsPoint(seg.a.pt, VERT_MERGE_DIST_SQR))
            {
                intersectionVertices.Add(seg.a);
            }
            else if (testingSegment.ContainsPoint(seg.b.pt, VERT_MERGE_DIST_SQR))
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
            AddEdge(intersectionVertices[i], intersectionVertices[i + 1], edgeFactory, factoryParams, knownEdges);
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
