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
    private List<IEdgeSplitListener> listeners;

    public const float VERT_MERGE_DIST_SQR = 0.001f;

    public delegate bool SearchFilter(LinkedGraphEdge theEdge);

    public LinkedGraphEdge (LinkedGraphVertex a, LinkedGraphVertex b)
    {
        this.a = a;
        this.b = b;
        segRef = new Segment();
        segRef.a = a.pt;
        segRef.b = b.pt;

        a.AddConnection(this);
        b.AddConnection(this);

        listeners = new List<IEdgeSplitListener>();
    }

    public void AddEdgeSplitListener(IEdgeSplitListener listener)
    {
        listeners.Add(listener);
    }

    public LinkedGraphVertex GetSharedVertex (LinkedGraphEdge other)
    {
        if (a == other.a || a == other.b)
        {
            return a;
        }
        else if (b == other.a || b == other.b)
        {
            return b;
        }

        return null;
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
        return GetSharedVertex(other) != null;
    }

    public List<EdgeType> CollectEdges <EdgeType> (bool allConsecutive, SearchFilter filter) where EdgeType : LinkedGraphEdge
    {
        List<EdgeType> collectedEdges = new List<EdgeType>();
        HashSet<LinkedGraphEdge> seenEdges = new HashSet<LinkedGraphEdge>();

        //List<LinkedGraphEdge> frontier = new List<LinkedGraphEdge>();

        Stack<LinkedGraphEdge> frontier = new Stack<LinkedGraphEdge>();
        frontier.Push(this);

        while (frontier.Count > 0)
        {
            LinkedGraphEdge next = frontier.Pop();

            EdgeType thisInstance = null;
            bool spread = true;
            if (next is EdgeType)
            {
                thisInstance = (EdgeType)next;
            }

            bool passesFilter = filter == null || filter(next);

            if (thisInstance == null)
            {
                if (allConsecutive)
                {
                    spread = false; 
                }
            }
            else if (passesFilter)
            {
                collectedEdges.Add(thisInstance);
            }
            else if (allConsecutive)//if all the passing edge should be attached, or consecutive, then if this fails, return.
            {
                spread = false;
            }

            seenEdges.Add(next);

            if (spread)
            {
                next.EnumerateNeighborEdges((LinkedGraphEdge edge) =>
                {
                    if (!seenEdges.Contains(edge) && !frontier.Contains(edge))
                    {
                        frontier.Push(edge);
                    }
                });
            }
        }

        //CollectEdgesR(collectedEdges, seenEdges, allConsecutive, filter, mask);

        return collectedEdges;
    }

    //private void CollectEdgesR <EdgeType> (List<EdgeType> collectedEdges, List<LinkedGraphEdge> seenEdges, bool allConsecutive, SearchFilter filter, HashSet<EdgeType> mask) where EdgeType : LinkedGraphEdge
    //{
    //    EdgeType thisInstance = null;
    //    if (this is EdgeType)
    //    {
    //        thisInstance = (EdgeType)this;
    //    }

    //    if ((thisInstance == null && allConsecutive) || collectedEdges.Contains(thisInstance) || seenEdges.Contains(this))
    //    {
    //        return;
    //    }
    //    else if ((filter == null || filter(this)) && (mask == null || mask.Contains(thisInstance)))
    //    {
    //        collectedEdges.Add(thisInstance);
    //    }
    //    else if (allConsecutive)//if all the passing edge should be attached, or consecutive, then if this fails, return.
    //    {
    //        return;
    //    }

    //    EnumerateNeighborEdges((LinkedGraphEdge edge) =>
    //    {
    //        if (!seenEdges.Contains(edge))
    //        {
    //            edge.CollectEdgesR(collectedEdges, seenEdges, allConsecutive, filter, mask); 
    //        }
    //    }); 
    //}

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
    //Ensure that edge1.a is equal to this.a and edge2.b is equal to this.b
    public void OnEdgeSplit (LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        foreach (IEdgeSplitListener listener in listeners)
        {
            listener.SplitEdge(this, edge1, edge2);
            edge1.AddEdgeSplitListener(listener);
            edge2.AddEdgeSplitListener(listener);
        }
        OnEdgeSplitCustom(edge1, edge2);
    }

    public virtual void OnEdgeSplitCustom (LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        
    }
}
