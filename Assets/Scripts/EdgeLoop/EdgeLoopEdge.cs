using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EdgeLoopEdge : LinkedGraphEdge
{
    private HashSet<IEdgeLoop> adjacentLoops;

    public EdgeLoopEdge (LinkedGraphVertex a, LinkedGraphVertex b) : base(a, b)
    {
        adjacentLoops = new HashSet<IEdgeLoop>();
    }

    public void AddAdjacentLoop (IEdgeLoop adjacent)
    {
        adjacentLoops.Add(adjacent);
    }

    public List<IEdgeLoop> GetAdjacentLoops ()
    {
        return adjacentLoops.ToList();
    }
}