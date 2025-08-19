using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeLoopEdgeFactory : ILinkedGraphEdgeFactory<EdgeLoopEdge>
{
    public EdgeLoopEdge GetEdge(LinkedGraphVertex a, LinkedGraphVertex b, System.Object[] data)
    {
        return new EdgeLoopEdge(a, b);
    }

    public EdgeLoopEdge GetEdgeFromParent (LinkedGraphVertex a, LinkedGraphVertex b, System.Object[] data, EdgeLoopEdge parent)
    {
        EdgeLoopEdge newEdge = new EdgeLoopEdge(a, b);
        List<IEdgeLoop> adjacentLoops = parent.GetAdjacentLoops();
        foreach (IEdgeLoop adj in adjacentLoops)
        {
            newEdge.AddAdjacentLoop(adj);
        }
        return newEdge;
    }
}

public class CityEdgeFactory : EdgeLoopEdgeFactory, ILinkedGraphEdgeFactory<CityEdge>
{
    public CityEdge GetEdge(LinkedGraphVertex a, LinkedGraphVertex b, System.Object[] data)
    {
        if (data == null)
        {
            return new CityEdge(a, b, CityEdgeType.Unspecified, 1f);
        }
        else
        {
            if (data.Length >= 2 && data[0] is CityEdgeType && data[1] is float)
            {
                return new CityEdge(a, b, (CityEdgeType)data[0], (float)data[1]); 
            }
            Debug.LogWarning("Bad data for creating CityEdge");
            return new CityEdge(a, b, CityEdgeType.Unspecified, 2f); 
        }
    }

    public CityEdge GetEdgeFromParent(LinkedGraphVertex a, LinkedGraphVertex b, System.Object[] data, CityEdge parent)
    {
        CityEdge newEdge;
        if (data == null)
        {
            newEdge = new CityEdge(a, b, CityEdgeType.Unspecified, 1f);
        }
        else
        {
            if (data.Length >= 2 && data[0] is CityEdgeType && data[1] is float)
            {
                return new CityEdge(a, b, (CityEdgeType)data[0], (float)data[1]);
            }
            Debug.LogWarning("Bad data for creating CityEdge");
            newEdge = new CityEdge(a, b, CityEdgeType.Unspecified, 2f);
        }

        List<IEdgeLoop> adjacentLoops = parent.GetAdjacentLoops();
        foreach (IEdgeLoop adj in adjacentLoops)
        {
            newEdge.AddAdjacentLoop(adj);
        }
        return newEdge;
    }
}