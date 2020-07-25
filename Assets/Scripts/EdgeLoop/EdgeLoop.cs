using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

//should be a ccw loop of edges
public class EdgeLoop <EdgeType> : IEdgeSplitListener where EdgeType : EdgeLoopEdge
{
    protected List<EdgeType> edges;
    private Rect bounds;

    public EdgeLoop(EdgeType[] edges)
    {
        this.edges = new List<EdgeType>(edges);
        RecalculateBounds();

        if (!Verify())
        {
            Debug.LogWarning("Edge loop edges do not form a loop.");
        }

        foreach (EdgeType edge in edges)
        {
            edge.AddEdgeSplitListener(this);
        }
    }

    public List<EdgeType> GetLocalLoop(EdgeType startingEdge, bool ccw)
    {
        List<EdgeType> foundEdges = new List<EdgeType>();
        foundEdges.Add(startingEdge);

        LinkedGraphVertex firstVertex = startingEdge.a;
        LinkedGraphVertex lastVertex = startingEdge.b;
        bool foundLoop = false;

        while (!foundLoop)
        {
            EdgeLoopEdge lastEdge = foundEdges[foundEdges.Count - 1];
            Vector2 lastEdgeDirection = lastEdge.GetOppositeVertex(lastVertex).pt - lastVertex.pt;

            float minAngle = float.MaxValue;
            EdgeType minAngleEdge = null;

            if (lastVertex.NumConnections() <= 1)
            {
                return null;
            }

            foreach (LinkedGraphEdge connection in lastVertex.GetConnections())
            {
                if (connection == lastEdge)
                {
                    continue;//ignore the connection if it is this instance.
                }
                //all connections must share lastVertex with lastEdge
                Vector2 thisEdgeDirection = connection.GetOppositeVertex(lastVertex).pt - lastVertex.pt;

                float angle = HelperFunctions.AngleBetween(thisEdgeDirection, lastEdgeDirection);
                if (!ccw)
                {
                    //we want to invert the smallest angle when its cw since angleBetween gets the ccw angle
                    angle = Mathf.PI * 2 - angle;
                }

                if (angle < minAngle)
                {
                    if (connection is EdgeType)
                    {
                        minAngle = angle;
                        minAngleEdge = (EdgeType)connection;
                    }
                    else
                    {
                        Debug.LogWarning("Could not isolate loop because connected edges were not EdgeLoopEdges");
                    }
                }
            }

            foundEdges.Add(minAngleEdge);
            lastVertex = minAngleEdge.GetOppositeVertex(lastVertex);

            if (lastVertex == firstVertex)
            {
                foundLoop = true;
            }

        }
        //maintain the loops go ccw paradigm
        if (!ccw)
        {
            foundEdges.Reverse();
        }
        return foundEdges;
    }

    public EdgeLoop(Vector2[] points, ILinkedGraphEdgeFactory<EdgeType> factory, Object factoryParams)
    {
        LinkedGraphVertex[] verts = new LinkedGraphVertex[points.Length];
        for (int i = 0; i < verts.Length; i ++)
        {
            verts[i] = new LinkedGraphVertex(points[i]);
        }
        EdgeType[] newEdges = new EdgeType[points.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            newEdges[i] = factory.GetEdge(verts[i], verts[(i + 1) % points.Length], factoryParams);//new EdgeLoopEdge(verts[i], verts[(i + 1) % points.Length]);
        }

        this.edges = new List<EdgeType>(newEdges);
        RecalculateBounds();

        if (!Verify())
        {
            Debug.LogWarning("Edge loop edges do not form a loop.");
        }

        foreach (EdgeType edge in edges)
        {
            edge.AddEdgeSplitListener(this);
        }
    }

    public static EdgeType[] GetPolygonEdges (int sides, float radius, float radiusRandomness, ILinkedGraphEdgeFactory<EdgeType> factory, System.Object factoryParams)
    {
        LinkedGraphVertex[] verts = new LinkedGraphVertex[sides];
        EdgeType[] edges = new EdgeType[sides];

        for (int i = 0; i < sides; i++)
        {
            float angle = (i / (float)sides) * Mathf.PI * 2;
            verts[i] = new LinkedGraphVertex(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius + Random.Range(-radiusRandomness, radiusRandomness)));
        }

        for (int i = 0; i < sides; i++)
        {
            int firstVertInd = i;
            int secondVertInd = (i + 1) % sides;
            edges[i] = factory.GetEdge(verts[firstVertInd], verts[secondVertInd], factoryParams);
        }
        return edges;
    }

    //check to see if an edge in this loop follows the ccw
    public bool EdgeFollowsWinding (EdgeType edge)
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
    public void SplitEdge(LinkedGraphEdge edge, LinkedGraphEdge a, LinkedGraphEdge b)
    {
        if (!(edge is EdgeType && a is EdgeType && b is EdgeType))
        {
            Debug.LogWarning("Bad split. one of the params is not the same type defined edgeLoop type");
            return;
        }
        EdgeType castEdge = (EdgeType)edge;
        EdgeType castA = (EdgeType)a;
        EdgeType castB = (EdgeType)b;
        if (a.a != edge.a || b.b != edge.b)
        {
            Debug.LogWarning("Bad split. A or B don't represent a split of edge");
            return;
        }
        int ind = edges.IndexOf(castEdge);
        if (ind == -1)
        {
            Debug.Log("Could not split edge as this loop does not contain it.");
        }
        else
        {
            bool edgeFollowsWinding = EdgeFollowsWinding(castEdge);
            edges.RemoveAt(ind);

            //make sure we add the two replacing edges in correct order
            if (edgeFollowsWinding)
            {
                edges.Insert(ind, castA);
                edges.Insert(ind + 1, castB);
            }
            else
            {
                edges.Insert(ind, castB);
                edges.Insert(ind + 1, castA);
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

    public IEnumerable<EdgeType> GetEdgesEnumerable()
    {
        return edges;
    }

    public EdgeType[] GetEdges ()
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

    public static bool IsEqual (EdgeLoop<EdgeType> a, EdgeLoop<EdgeType> b)
    {
        return IsEqual(a.GetEdges(), b.GetEdges());
    }

    public static bool IsEqual (EdgeType[] a, EdgeType[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        int indexOfFirst = System.Array.IndexOf(b, a[0]);
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

    public Vector2[] GetSimplifiedPoints (float simplificationAngle)
    {
        List<Vector2> pts = new List<Vector2>(GetPoints());
        int i = 0;
        while(i < pts.Count)
        {
            int prevInd = i;
            int thisInd = (i + 1) % pts.Count;
            int nextInd = (i + 2) % pts.Count;

            float ccwAngle = HelperFunctions.AngleBetween(pts[nextInd] - pts[thisInd], pts[prevInd] - pts[thisInd]);
            if (Mathf.Abs(ccwAngle - Mathf.PI) < simplificationAngle)
            {
                i = 0;
                pts.RemoveAt(thisInd);
            }
            else
            {
                i++; 
            }
        }
        return pts.ToArray();
    }

    public bool IsConvex() 
    {
        Vector2[] pts = GetPoints();

        for (int i = 0; i < pts.Length; i ++)
        {
            int prevInd = i;
            int thisInd = (i + 1) % pts.Length;
            int nextInd = (i + 2) % pts.Length;
            float ccwAngle = HelperFunctions.AngleBetween(pts[nextInd] - pts[thisInd], pts[prevInd] - pts[thisInd]);
            if (ccwAngle > Mathf.PI + 0.05f)//small tolerance to avoid issues with edges that are subdivided
            {
                return false;
            }
        }
        return true;
    }

    public Vector2[] GetPoints () 
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
        return points;
    }

    public Polygon GetPolygon()
    {
        return new Polygon(GetPoints());
    }

    public void EnumerateEdges (System.Action<EdgeLoopEdge> action)
    {
        // Enumerate local points.
        foreach (EdgeLoopEdge eachEdge in edges)
        {
            action(eachEdge);
        }
    }

    public void DebugDraw (float strength)
    {
        Color drawCol = getDebugColor();

        EnumerateEdges((EdgeLoopEdge eachEdge_) =>
        {
            UnityEngine.Debug.DrawLine(HelperFunctions.projVec2(eachEdge_.a.pt), HelperFunctions.projVec2(eachEdge_.b.pt), drawCol);
        });
        drawCol = Color.white;
        if (!IsConvex())
        {
            drawCol = Color.red;
        }
        Debug.DrawLine(HelperFunctions.projVec2(GetPolygon().centroid), HelperFunctions.projVec2(GetPolygon().centroid) + Vector3.up * 0.1f, drawCol);
    }

    public virtual Color getDebugColor()
    {
        return Color.gray;
    }
}
