using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

public class LinkedGraphEdge
{
    public LinkedGraphVertex a;
    public LinkedGraphVertex b;
    public Vector3 center => (a.pt + b.pt) / 2;
    public Segment segRef;

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

    //called on the instance ofwhen it is subdivided
    public virtual void OnEdgeSplit (LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        Debug.LogWarning("No OnEdgeSplit() override defined");
    }
}
