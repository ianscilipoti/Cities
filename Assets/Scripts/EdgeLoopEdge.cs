using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeLoopEdge : LinkedGraphEdge
{
    private List<EdgeLoop> involvedLoops;
    public EdgeLoopEdge (LinkedGraphVertex a, LinkedGraphVertex b) : base(a, b)
    {
        involvedLoops = new List<EdgeLoop>();
    }

    public void InvolveLoop (EdgeLoop loop)
    {
        involvedLoops.Add(loop);
    }

    public static EdgeLoopEdge[] GetPolygonEdges (int sides, float radius, float radiusRandomness)
    {
        LinkedGraphVertex[] verts = new LinkedGraphVertex[sides];
        EdgeLoopEdge[] edges = new EdgeLoopEdge[sides];
        ILinkedGraphEdgeFactory<EdgeLoopEdge> factory = new EdgeLoopEdgeFactory();
        for (int i = 0; i < sides; i ++)
        {
            float angle = (i / (float)sides) * Mathf.PI * 2;
            verts[i] = new LinkedGraphVertex(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius + Random.Range(-radiusRandomness, radiusRandomness)));
        }

        for (int i = 0; i < sides; i++)
        {
            int firstVertInd = i;
            int secondVertInd = (i + 1) % sides;
            edges[i] = new EdgeLoopEdge(verts[firstVertInd], verts[secondVertInd]);
        }
        return edges;
    }

    //gets a list of edges representing the loop. Construction of the EdgeLoop Object is left to the user
    public List<EdgeLoopEdge> GetLocalLoop (bool ccw)
    {
        List<EdgeLoopEdge> foundEdges = new List<EdgeLoopEdge>();
        foundEdges.Add(this);

        LinkedGraphVertex firstVertex = this.a;
        LinkedGraphVertex lastVertex = this.b;
        bool foundLoop = false;

        while (!foundLoop)
        {
            EdgeLoopEdge lastEdge = foundEdges[foundEdges.Count - 1];
            Vector2 lastEdgeDirection = lastEdge.GetOppositeVertex(lastVertex).pt - lastVertex.pt;

            float minAngle = float.MaxValue;
            EdgeLoopEdge minAngleEdge = null;

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
                    if (connection is EdgeLoopEdge)
                    {
                        minAngle = angle;
                        minAngleEdge = (EdgeLoopEdge)connection;
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
    //called when this edge is being split into edge1 and edge2
    public override void OnEdgeSplit (LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        if (!(edge1 is EdgeLoopEdge) || !(edge2 is EdgeLoopEdge))
        {
            Debug.LogWarning("Bad Split. Edge1 or Edge2 aren't an EdgeLoopEdge");
            return;
        }
        EdgeLoopEdge edgeL1 = (EdgeLoopEdge)edge1;
        EdgeLoopEdge edgeL2 = (EdgeLoopEdge)edge2;

        if (involvedLoops.Count > 0)
        {
            foreach (EdgeLoop involved in involvedLoops)
            {
                involved.SplitEdge(this, edgeL1, edgeL2);
            }
        }
    }
}

public class EdgeLoopEdgeFactory : ILinkedGraphEdgeFactory<EdgeLoopEdge>
{
    public EdgeLoopEdge GetEdge(LinkedGraphVertex a, LinkedGraphVertex b)
    {
        return new EdgeLoopEdge(a, b);
    }
}