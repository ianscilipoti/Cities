using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;


public class EdgeLoop
{
    protected List<EdgeLoopEdge> edges;
    private Rect bounds;

    public EdgeLoop(EdgeLoopEdge[] edges)
    {
        this.edges = new List<EdgeLoopEdge>(edges);
        RecalculateBounds();

        if (!Verify())
        {
            Debug.LogWarning("Edge loop edges do not form a loop.");
        }
    }

    public bool Verify () 
    {
        for (int i = 0; i < edges.Count; i ++)
        {
            int nextIndex = (i + 1) % (edges.Count);
            if (!edges[i].isConnectedTo(edges[nextIndex]))
            {
                return false;
            }
        }
        return true;
    }

    public IEnumerable<EdgeLoopEdge> GetEdges()
    {
        return edges;
    }

    //search delegate
    protected bool EdgeWithinLoop (LinkedGraphEdge theEdge)
    {
        Polygon poly = GetPolygon();
        if ((poly.PermiterContainsPoint(theEdge.a.pt) || poly.ContainsPoint(theEdge.a.pt)) &&
            (poly.PermiterContainsPoint(theEdge.b.pt) || poly.ContainsPoint(theEdge.b.pt)))
        {
            return true;
        }
        return false;
    }

    public bool IsEqual (EdgeLoop other)
    {
        if (edges.Count != other.edges.Count)
        {
            return false;
        }
        int indexOfFirst = other.edges.IndexOf(edges[0]);
        if(indexOfFirst == -1)//our first edge is not in the other loop
        {
            return false;
        }
        else 
        {
            int offset = indexOfFirst;//now that we know the offset, we must see all edges match with same offset
            for (int i = 0; i < edges.Count; i ++)
            {
                if (edges[i] != other.edges[(i + offset) % edges.Count])
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void RecalculateBounds()
    {
        float rightExtent = float.NegativeInfinity;
        float leftExtent = float.PositiveInfinity;
        float upExtent = float.NegativeInfinity;
        float downExtent = float.PositiveInfinity;

        EnumerateEdges((EdgeLoopEdge edge) =>
        {
            rightExtent = Mathf.Max(rightExtent, Mathf.Max(edge.a.pt.x, edge.b.pt.x));
            leftExtent = Mathf.Min(leftExtent, Mathf.Min(edge.a.pt.x, edge.b.pt.x));

            upExtent = Mathf.Max(upExtent, Mathf.Max(edge.a.pt.y, edge.b.pt.y));
            downExtent = Mathf.Min(downExtent, Mathf.Min(edge.a.pt.y, edge.b.pt.y));
        });

        bounds = new Rect(leftExtent, downExtent, rightExtent - leftExtent, upExtent - downExtent);
    }


    public Vector3 GetCenter()
    {
        Vector3 total = Vector3.zero;
        foreach (EdgeLoopEdge edge in GetEdges())
        {
            total += edge.center;
        }
        return total / edges.Count;
    }

    public Rect GetBounds()
    {
        return bounds;
    }

    public Polygon GetPolygon()
    {
        Vector2[] points = new Vector2[edges.Count];
        points[0] = edges[0].GetSharedVertex(edges[1]).pt;
        for (int i = 1; i < points.Length; i++)
        {
            EdgeLoopEdge edge = edges[i];
            //ensure that each edge adds a unique point given the winding direction
            //is unknown
            if (edge.a.pt.Equals(points[i - 1]))
            {
                points[i] = edge.b.pt;
            }
            else
            {
                points[i] = edge.a.pt;
            }
        }

        return new Polygon(points);
    }

    public void EnumerateEdges (System.Action<EdgeLoopEdge> action)
    {
        // Enumerate local points.
        foreach (EdgeLoopEdge eachEdge in edges)
        {
            action(eachEdge);
        }
    }
}
