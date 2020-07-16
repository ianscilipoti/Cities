using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILinkedGraphEdgeFactory <EdgeType> where EdgeType : LinkedGraphEdge
{
    LinkedGraphEdge GetEdge (LinkedGraphVertex a, LinkedGraphVertex b);
}
