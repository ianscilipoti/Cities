using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILinkedGraphEdgeFactory 
{
    LinkedGraphEdge GetEdge (LinkedGraphVertex a, LinkedGraphVertex b);
}
