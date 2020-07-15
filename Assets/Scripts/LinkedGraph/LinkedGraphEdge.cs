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

    public LinkedGraphVertex GetOppositeVertex(LinkedGraphVertex vertex)
    {
        if (a != vertex && b != vertex)
        {
            Debug.LogWarning("Can't get opposite vertex.");
            return null;
        }
        else
        {
            if (a == vertex)
            {
                return b;
            }
            else
            {
                return a;
            }
        }
    }

    public bool isConnectedTo (LinkedGraphEdge other)
    {
        if (a == other.a || a == other.b || b == other.a || b == other.b)
        {
            return true;
        }
        return false;
    }

    public List<EdgeType> CollectEdges <EdgeType> (bool allConsecutive, SearchFilter filter, HashSet<EdgeType> mask) where EdgeType : LinkedGraphEdge
    {
        List<EdgeType> collectedEdges = new List<EdgeType>();
        List<LinkedGraphEdge> seenEdges = new List<LinkedGraphEdge>();

        CollectEdgesR(collectedEdges, seenEdges, allConsecutive, filter, mask);

        return collectedEdges;
    }

    private void CollectEdgesR <EdgeType> (List<EdgeType> collectedEdges, List<LinkedGraphEdge> seenEdges, bool allConsecutive, SearchFilter filter, HashSet<EdgeType> mask) where EdgeType : LinkedGraphEdge
    {
        EdgeType thisInstance = null;
        if (this is EdgeType)
        {
            thisInstance = (EdgeType)this;
        }

        if ((thisInstance == null && allConsecutive) || collectedEdges.Contains(thisInstance) || seenEdges.Contains(this))
        {
            return;
        }
        else if ((filter == null || filter(this)) && (mask == null || mask.Contains(thisInstance)))
        {
            collectedEdges.Add(thisInstance);
        }
        else if (allConsecutive)//if all the passing edge should be attached, or consecutive, then if this fails, return.
        {
            return;
        }

        EnumerateNeighborEdges((LinkedGraphEdge edge) =>
        {
            if (!seenEdges.Contains(edge))
            {
                edge.CollectEdgesR(collectedEdges, seenEdges, allConsecutive, filter, mask); 
            }
        }); 
    }

    public void EnumerateNeighborEdges(System.Action<LinkedGraphEdge> action)
    {
        foreach (LinkedGraphEdge edge in a.GetConnections())
        {
            if (edge != this)
            {
                action(edge);
            }
        }
        foreach (LinkedGraphEdge edge in b.GetConnections())
        {
            if (edge != this)
            {
                action(edge);
            }
        }
    } 

    //disconnect this edge. Leaving it for the GC :(
    public void Detach ()
    {
        a.RemoveConnection(this);
        b.RemoveConnection(this);
    }

    private void SubdivideEdge (LinkedGraphVertex midPoint, ILinkedGraphEdgeFactory edgeFactory, List<LinkedGraphEdge> knownEdges)
    {
        LinkedGraphVertex aVert = a;
        LinkedGraphVertex bVert = b;

        LinkedGraphEdge aEdge = AddEdge(aVert, midPoint, edgeFactory, knownEdges);
        LinkedGraphEdge bEdge = AddEdge(bVert, midPoint, edgeFactory, knownEdges);

        OnEdgeSplit(aEdge, bEdge);

        knownEdges.Remove(this);
        Detach();
    }

    private static LinkedGraphEdge AddEdge (LinkedGraphVertex aVert, LinkedGraphVertex bVert, ILinkedGraphEdgeFactory edgeFactory, List<LinkedGraphEdge> knownEdges)
    {
        LinkedGraphEdge newEdge = edgeFactory.GetEdge(aVert, bVert);
        newEdge.a.AddConnection(newEdge);
        newEdge.b.AddConnection(newEdge);

        knownEdges.Add(newEdge);

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
    public static void ConnectNewEdge (Vector2 a, Vector2 b, ILinkedGraphEdgeFactory edgeFactory, List<LinkedGraphEdge> knownEdges)
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

        LinkedGraphEdge[] edgesCopy = new LinkedGraphEdge[knownEdges.Count];
        knownEdges.CopyTo(edgesCopy);

        //check if new vertices are on another line
        foreach (LinkedGraphEdge seg in edgesCopy)
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
                seg.SubdivideEdge(aVert, edgeFactory, knownEdges);
            }
            else if (seg.segRef.ContainsPoint(b, VERT_MERGE_DIST_SQR))
            {
                seg.SubdivideEdge(bVert, edgeFactory, knownEdges);
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
                    seg.SubdivideEdge(midPtVert, edgeFactory, knownEdges);
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

    public static void DebugDraw <EdgeType> (List<EdgeType> edges) where EdgeType : LinkedGraphEdge
    {
        
        foreach (EdgeType edge in edges)
        {
            int colorCode = edge.GetHashCode();
            Random.InitState(colorCode);

            Debug.DrawLine(edge.a.pt, edge.b.pt, Random.ColorHSV());
        }
    }
}
