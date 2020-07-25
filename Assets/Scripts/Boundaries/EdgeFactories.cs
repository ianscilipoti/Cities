using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeLoopEdgeFactory : ILinkedGraphEdgeFactory<EdgeLoopEdge>
{
    public EdgeLoopEdge GetEdge(LinkedGraphVertex a, LinkedGraphVertex b, System.Object data)
    {
        return new EdgeLoopEdge(a, b);
    }
}

public class CityEdgeFactory : ILinkedGraphEdgeFactory<CityEdge>
{
    public CityEdge GetEdge(LinkedGraphVertex a, LinkedGraphVertex b, System.Object data)
    {
        if (data == null)
        {
            return new CityEdge(a, b, CityEdgeType.Unspecified);
        }
        else
        {
            if (!(data is CityEdgeType))
            {
                Debug.LogWarning("Bad data for creating CityEdge");
            }
            return new CityEdge(a, b, (CityEdgeType)data);  
        }
    }
}