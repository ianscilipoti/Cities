using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEdgeSplitListener
{
    void SplitEdge(LinkedGraphEdge splitEdge, LinkedGraphEdge a, LinkedGraphEdge b);
}
