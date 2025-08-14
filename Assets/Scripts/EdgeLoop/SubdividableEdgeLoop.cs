using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using System.Linq;

//provides functionality to recursively subdivide a region defined by an edge loop
//requires a subdivision scheme to format the subdivision pattern
//and a getNextChild to randomly choose the types of objects representing the childrendd
public class SubdividableEdgeLoop<EdgeType> : EdgeLoop<EdgeType> where EdgeType : EdgeLoopEdge
{
    protected List<SubdividableEdgeLoop<EdgeType>> children;
    private bool isSubdividable;
    private bool isSubdivided;

    public SubdividableEdgeLoop(EdgeType[] edges, bool subdividable) : base(edges)
    {
        children = new List<SubdividableEdgeLoop<EdgeType>>();
        isSubdividable = subdividable;
    }

    public SubdividableEdgeLoop<EdgeType>[] GetChildren()
    {
        return children.ToArray();
    }

    public virtual SubdividableEdgeLoop<EdgeType> GetNextChild(EdgeType[] edges)
    {
        return new SubdividableEdgeLoop<EdgeType>(edges, true);
    }

    public bool IsSubdividable()
    {
        return isSubdividable;
    }

    public bool IsSubdivided()
    {
        return isSubdivided;
    }

    protected bool VerifyRecursive ()
    {
        if (!Verify())
        {
            return false;
        }
        else
        {
            foreach (var child in children)
            {
                if (!child.VerifyRecursive())
                {
                    return false;
                }
            }
        }
        return true;
    }

    //Given a parent edge loop, find all loops that are geometrically contained within this parent loop. Do this by finding all connected edges within the perimeter,
    //Then, find loops that may involve the cw and ccw directions of each contained child edge

    public List<EdgeType[]> GetInteriorEdgeLoops()
    {
        List<EdgeType[]> foundLoops = new List<EdgeType[]>(); ;

        //starting at the first edge, do a search outward to collect all edges that are within 
        List<EdgeType> allEdges = edges[0].CollectEdges<EdgeType>(true, EdgeWithinLoop); //exterior and interior
        List<EdgeType> exteriorEdges = edges;

        //get all edges that make up the interior of the loop
        List<EdgeType> interiorEdges = allEdges.Except(exteriorEdges).ToList();

        //clean up edges that can't be part of polygons
        for (int i = interiorEdges.Count - 1; i >= 0; i--)
        {
            EdgeType edge = interiorEdges[i];
            if (edge.a.NumConnections() <= 1 || edge.b.NumConnections() <= 1)
            {
                interiorEdges.Remove(edge);
                Debug.LogWarning("Detached an edge while getting interior edge loops");
            }
        }

        //keep track of the loops we've already found for each edge
        Dictionary<EdgeType, List<EdgeType[]>> edgeLoopCounts = new Dictionary<EdgeType, List<EdgeType[]>>();

        foreach (EdgeType edge in interiorEdges)
        {
            edgeLoopCounts.Add(edge, new List<EdgeType[]>());
        }

        foreach (EdgeType edge in interiorEdges)
        {
            if (edgeLoopCounts[edge].Count == 2)
            {
                continue;//we've found both loops associated with this edge
            }
            List<EdgeType> ccwLoopList = GetLocalLoop(edge, true);
            List<EdgeType> cwLoopList = GetLocalLoop(edge, false);

            EdgeType[] ccwLoop = null;
            EdgeType[] cwLoop = null;

            if (ccwLoopList == null || cwLoopList == null)
            {
                Debug.LogWarning("Loop was null.");
            }
            else
            {
                ccwLoop = ccwLoopList.ToArray();
                cwLoop = cwLoopList.ToArray(); 
            }

            bool existingCcwLoop = false;
            bool existingCwLoop = false;

            foreach (EdgeType[] foundLoop in edgeLoopCounts[edge])
            {
                if (ccwLoop != null && IsEqual(foundLoop, ccwLoop))
                {
                    existingCcwLoop = true;
                }
                if (cwLoop != null && IsEqual(foundLoop, cwLoop))
                {
                    existingCwLoop = true;
                }
            }

            if (!existingCcwLoop && ccwLoop != null)
            {
                foreach (EdgeType subEdge in ccwLoop)
                {
                    if (edgeLoopCounts.ContainsKey(subEdge))
                    {
                        edgeLoopCounts[subEdge].Add(ccwLoop);
                    }
                }

                foundLoops.Add(ccwLoop);
            }
            if (!existingCwLoop && cwLoop != null)
            {
                foreach (EdgeType subEdge in cwLoop)
                {
                    if (edgeLoopCounts.ContainsKey(subEdge))
                    {
                        edgeLoopCounts[subEdge].Add(cwLoop);
                    }
                }
                foundLoops.Add(cwLoop);
            }
        }

        return foundLoops;
    }

    public bool TrySubdivide()
    {
        if (isSubdivided)
        {
            return true; // Already subdivided
        }

        if (!IsSubdividable())
        {
            return false; // Can't subdivide
        }

        var newChildren = Subdivide();

        if (newChildren != null && newChildren.Count > 0)
        {
            children.AddRange(newChildren);
            isSubdivided = true;
            return true;
        }

        return false;
    }

    protected virtual List<SubdividableEdgeLoop<EdgeType>> Subdivide()
    {
        return new List<SubdividableEdgeLoop<EdgeType>>();
    }

    public void DebugDrawRecursive (float strength, int depth)
    {
        if (children.Count == 0 || depth == 0)
        {
            Color drawCol = getDebugColor();

            EnumerateEdges((EdgeLoopEdge eachEdge_) =>
            {
                UnityEngine.Debug.DrawLine(HelperFunctions.projVec2(eachEdge_.a.pt), HelperFunctions.projVec2(eachEdge_.b.pt), drawCol);
            });
            drawCol = Color.white;
            if (!IsConvex())
            {
                drawCol = Color.red;
            }
            if (isSubdividable)
            {
                drawCol = Color.green;
            }
            Debug.DrawLine(HelperFunctions.projVec2(GetPolygon().centroid), HelperFunctions.projVec2(GetPolygon().centroid) + Vector3.up * 0.1f, drawCol);
        }

        if (depth == 0)
        {
            return;
        }

        foreach (var child in children)
        {
            child.DebugDrawRecursive(strength, depth-1);
        }
    }
}
