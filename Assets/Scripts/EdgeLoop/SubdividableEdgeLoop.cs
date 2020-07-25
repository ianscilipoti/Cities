using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using System.Linq;

//provides functionality to recursively subdivide a region defined by an edge loop
//requires a subdivision scheme to format the subdivision pattern
//and a getNextChild to randomly choose the types of objects representing the children
public class SubdividableEdgeLoop <EdgeType> : EdgeLoop<EdgeType>, Subdividable where EdgeType : EdgeLoopEdge
{
    private Rect bounds;
    private List<SubdividableEdgeLoop<EdgeType>> children;
    private bool isSubdividable;
    private bool isSubdivided;

    public SubdividableEdgeLoop (EdgeType[] edges, bool subdividable) : base(edges)
    {
        children = new List<SubdividableEdgeLoop<EdgeType>>();
        isSubdividable = subdividable;
    }

    public SubdividableEdgeLoop<EdgeType>[] GetChildren ()
    {
        return children.ToArray();
    }

    public virtual ISubDivScheme<SubdividableEdgeLoop<EdgeType>> GetDivScheme()
    {
        return null;//new GetPieSections();
    }

    public virtual SubdividableEdgeLoop<EdgeType> GetNextChild (EdgeType[] edges)
    {
        return new SubdividableEdgeLoop<EdgeType>(edges, true);
    }

    public bool IsSubdividable ()
    {
        return isSubdividable;
    }

    public bool IsSubdivided () 
    {
        return isSubdivided;
    }

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
            EdgeType[] ccwLoop = GetLocalLoop(edge, true).ToArray();
            EdgeType[] cwLoop = GetLocalLoop(edge, false).ToArray();

            bool existingCcwLoop = false;
            bool existingCwLoop = false;

            foreach (EdgeType[] foundLoop in edgeLoopCounts[edge])
            {
                if (IsEqual(foundLoop, ccwLoop))
                {
                    existingCcwLoop = true;
                }
                if (IsEqual(foundLoop, cwLoop))
                {
                    existingCwLoop = true;
                }
            }

            if (!existingCcwLoop)
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
            if (!existingCwLoop)
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

	public bool Subdivide ()
	{
        if (IsSubdivided())
        {
            return true;
        }
        ISubDivScheme<SubdividableEdgeLoop<EdgeType>> subdivScheme = GetDivScheme();
        if (subdivScheme != null)
        {
            children = subdivScheme.GetChildren(this);
            isSubdivided = true;
            return true;
        }
        else
        {
            return false;
        }
	}

    //public void SubdivideRecursive ()
    //{
    //    if (!IsSubdividable())
    //    {
    //        return;
    //    }
    //    if (Subdivide())
    //    {
    //        foreach (SubdividableEdgeLoop child in children)
    //        {
    //            child.SubdivideRecursive();
    //        } 
    //    }
    //}

    public void DebugDrawRecursive (float strength)
    {
        if (children.Count == 0)
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
            Debug.DrawLine(HelperFunctions.projVec2(GetPolygon().centroid), HelperFunctions.projVec2(GetPolygon().centroid) + Vector3.up * 0.1f, drawCol);
        }

        foreach (var child in children)
        {
            child.DebugDrawRecursive(strength);
        }
    }
}
