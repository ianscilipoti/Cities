using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using System;

//should be a ccw loop of edges
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

        foreach (EdgeLoopEdge edge in edges)
        {
            edge.InvolveLoop(this);
        }
    }

    //check to see if an edge in this loop follows the ccw
    public bool EdgeFollowsWinding (EdgeLoopEdge edge)
    {
        int ind = edges.IndexOf(edge);
        if (ind == -1)
        {
            Debug.Log("Could not calculate if edge follows winding. Not part of loop.");
            return false;
        }
        //get the vertex on first edge that is shared by the first two edges
        LinkedGraphVertex lastVertex = edges[0].GetSharedVertex(edges[1]);
        for (int i = 0; i < edges.Count; i ++)
        {
            if (i > 0)
            {
                lastVertex = edges[i].GetOppositeVertex(lastVertex);
            }
            if (i == ind && edges[i].b == lastVertex)
            {
                return true;
            }
        }
        return false;
    }

    //split edge into a and b.
    public void SplitEdge(EdgeLoopEdge edge, EdgeLoopEdge a, EdgeLoopEdge b)
    {
        if (a.a != edge.a || b.b != edge.b)
        {
            Debug.LogWarning("Bad split. A or B don't represent a split of edge");
            return;
        }
        int ind = edges.IndexOf(edge);
        if (ind == -1)
        {
            Debug.Log("Could not split edge as this loop does not contain it.");
        }
        else
        {
            bool edgeFollowsWinding = EdgeFollowsWinding(edge);
            edges.RemoveAt(ind);

            //make sure we add the two replacing edges in correct order
            if (edgeFollowsWinding)
            {
                edges.Insert(ind, a);
                edges.Insert(ind + 1, b);
            }
            else
            {
                edges.Insert(ind, b);
                edges.Insert(ind + 1, a);
            }
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

    public IEnumerable<EdgeLoopEdge> GetEdgesEnumerable()
    {
        return edges;
    }

    public EdgeLoopEdge[] GetEdges ()
    {
        return edges.ToArray();
    }

    //search delegate
    protected bool EdgeWithinLoop (LinkedGraphEdge theEdge)
    {
        Polygon poly = GetPolygon();
        if ((poly.PermiterContainsPoint(theEdge.a.pt, Segment.defaultAccuracy*2) || poly.ContainsPoint(theEdge.a.pt)) &&
            (poly.PermiterContainsPoint(theEdge.b.pt, Segment.defaultAccuracy*2) || poly.ContainsPoint(theEdge.b.pt)))
        {
            return true;
        }
        return false;
    }

    public static bool IsEqual (EdgeLoop a, EdgeLoop b)
    {
        return IsEqual(a.GetEdges(), b.GetEdges());
    }

    public static bool IsEqual (EdgeLoopEdge[] a, EdgeLoopEdge[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        int indexOfFirst = Array.IndexOf(b, a[0]);
        if(indexOfFirst == -1)//our first edge is not in the other loop
        {
            return false;
        }
        else 
        {
            int offset = indexOfFirst;//now that we know the offset, we must see all edges match with same offset
            for (int i = 0; i < a.Length; i ++)
            {
                if (a[i] != b[(i + offset) % a.Length])
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
        foreach (EdgeLoopEdge edge in GetEdgesEnumerable())
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
