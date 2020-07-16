using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedGraph<EdgeType> where EdgeType : LinkedGraphEdge
{
    public static EdgeType AddEdge(LinkedGraphVertex aVert, LinkedGraphVertex bVert, ILinkedGraphEdgeFactory<EdgeType> edgeFactory, List<EdgeType> knownEdges)
    {
        LinkedGraphEdge newEdge = edgeFactory.GetEdge(aVert, bVert);
        newEdge.a.AddConnection(newEdge);
        newEdge.b.AddConnection(newEdge);

        knownEdges.Add(newEdge);

        return newEdge;
    }
}
