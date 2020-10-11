using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILinkedGraphEdgeFactory <EdgeType> where EdgeType : LinkedGraphEdge
{
    EdgeType GetEdge (LinkedGraphVertex a, LinkedGraphVertex b, System.Object[] data);
}
