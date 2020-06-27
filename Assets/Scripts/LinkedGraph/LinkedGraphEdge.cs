using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

public class LinkedGraphEdge
{
    public LinkedGraphVertex a;
    public LinkedGraphVertex b;
    public Vector3 center => (a.pt + b.pt) / 2;
    private Segment segRef;

    public const float VERT_MERGE_DIST_SQR = 0.001f;

    public delegate bool SearchFilter(LinkedGraphEdge theEdge);

    public LinkedGraphEdge (LinkedGraphVertex a, LinkedGraphVertex b)
    {
        this.a = a;
        this.b = b;
        segRef = new Segment();
        segRef.a = a.pt;
        segRef.b = b.pt;
    }

    public void CollectEdges (List<LinkedGraphEdge> collectedEdges, bool allConsecutive, SearchFilter filter)
    {
        List<LinkedGraphEdge> seenEdges = new List<LinkedGraphEdge>();
        CollectEdgesR(collectedEdges, seenEdges, allConsecutive, filter);
    }

    private void CollectEdgesR (List<LinkedGraphEdge> collectedEdges, List<LinkedGraphEdge> seenEdges, bool allConsecutive, SearchFilter filter)
    {
        if (collectedEdges.Contains(this) || seenEdges.Contains(this))
        {
            return;
        }
        else if (filter(this))
        {
            collectedEdges.Add(this);
        }
        else if (allConsecutive)//if all the passing edge should be attached, or consecutive, then if this fails, return.
        {
            return;
        }

        EnumerateNeighborEdges((LinkedGraphEdge edge) =>
        {
            if (!seenEdges.Contains(edge))
            {
                CollectEdgesR(collectedEdges, seenEdges, allConsecutive, filter); 
            }
        }); 
    }

    public void EnumerateNeighborEdges(System.Action<LinkedGraphEdge> action)
    {
        foreach (LinkedGraphEdge eachEdge in a.GetConnections())
        {
            action(eachEdge);
        }
        foreach (LinkedGraphEdge eachEdge in a.GetConnections())
        {
            action(eachEdge);
        }
    } 

    //disconnect this edge. Leaving it for the GC :(
    private void Detach ()
    {
        a.RemoveConnection(this);
        b.RemoveConnection(this);
    }

    private void SubdivideEdge (LinkedGraphVertex midPoint, ILinkedGraphEdgeFactory edgeFactory)
    {
        LinkedGraphVertex aVert = a;
        LinkedGraphVertex bVert = b;

        LinkedGraphEdge aEdge = AddEdge(aVert, midPoint, edgeFactory);
        LinkedGraphEdge bEdge = AddEdge(bVert, midPoint, edgeFactory);

        OnEdgeSplit(aEdge, bEdge);

        Detach();
    }

    private static LinkedGraphEdge AddEdge (LinkedGraphVertex aVert, LinkedGraphVertex bVert, ILinkedGraphEdgeFactory edgeFactory)
    {
        LinkedGraphEdge newEdge = edgeFactory.GetEdge(aVert, bVert);
        newEdge.a.AddConnection(newEdge);
        newEdge.b.AddConnection(newEdge);

        return newEdge;
    }

    //perform action on all vertices
    public static void EnumerateVertices (List<LinkedGraphEdge> edges, System.Action<LinkedGraphVertex> action) 
    {
        // Enumerate local points.
        foreach (LinkedGraphEdge eachEdge in edges)
        {
            action(eachEdge.a);
            action(eachEdge.b);
        }
    }

    //given a list of edges that may intersect with the new edge, connect the new edge
    public static void ConnectNewEdge (Vector2 a, Vector2 b, ILinkedGraphEdgeFactory edgeFactory, List<LinkedGraphEdge> possibleConnections)
    {
        LinkedGraphVertex aVert = HasVertex(possibleConnections, a);
        LinkedGraphVertex bVert = HasVertex(possibleConnections, b);

        if (aVert == null)
        {
            aVert = new LinkedGraphVertex(a);
        }
        if (bVert == null)
        {
            bVert = new LinkedGraphVertex(b);
        }

        //use for checking intersections
        Segment testingSegment = Segment.SegmentWithPoints(a, b);
        Rect testingSegBounds = testingSegment.ExpandedBounds(VERT_MERGE_DIST_SQR);

        List<LinkedGraphVertex> intersectionVertices = new List<LinkedGraphVertex>();
        intersectionVertices.Add(aVert);//these will be sorted later
        intersectionVertices.Add(bVert);

        //check if new vertices are on another line
        foreach (LinkedGraphEdge seg in possibleConnections)
        {
            
            if (!seg.segRef.bounds.Overlaps(testingSegBounds))
            {
                continue;
            }
            if (seg.segRef.ContainsPoint(a, VERT_MERGE_DIST_SQR))
            {
                seg.SubdivideEdge(aVert, edgeFactory);
            }
            else if (seg.segRef.ContainsPoint(b, VERT_MERGE_DIST_SQR))
            {
                seg.SubdivideEdge(bVert, edgeFactory);
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
                    seg.SubdivideEdge(midPtVert, edgeFactory);
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
            AddEdge(intersectionVertices[i], intersectionVertices[i + 1], edgeFactory);
        }
    }

    //given a list of edges, check if the vertex at pt already exists
    public static LinkedGraphVertex HasVertex (List<LinkedGraphEdge> inEdges, Vector2 pt)
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

    //called on the instance ofwhen it is subdivided
    public virtual void OnEdgeSplit (LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        Debug.LogWarning("No OnEdgeSplit() override defined");
    }
}
