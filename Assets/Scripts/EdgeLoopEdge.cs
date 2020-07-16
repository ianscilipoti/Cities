using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeLoopEdge : LinkedGraphEdge
{
    private List<EdgeLoop> involvedEdges;
    public EdgeLoopEdge (LinkedGraphVertex a, LinkedGraphVertex b) : base(a, b)
    {
        involvedEdges = new List<EdgeLoop>();
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
        return foundEdges;
    }

    public override void OnEdgeSplit (LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        //Debug.Log("Implement me. Need to fix edgeLoops involved.");
    }
}

public class EdgeLoopEdgeFactory : ILinkedGraphEdgeFactory
{
    public LinkedGraphEdge GetEdge(LinkedGraphVertex a, LinkedGraphVertex b)
    {
        return new EdgeLoopEdge(a, b);
    }
}