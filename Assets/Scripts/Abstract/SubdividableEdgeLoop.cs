using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
using System.Linq;

//provides functionality to recursively subdivide a region defined by an edge loop
//requires a subdivision scheme to format the subdivision pattern
//and a getNextChild to randomly choose the types of objects representing the children
public class SubdividableEdgeLoop : EdgeLoop, Subdividable
{
    private Rect bounds;
    private List<SubdividableEdgeLoop> children;
    private bool isSubdividable;

    public SubdividableEdgeLoop (EdgeLoopEdge[] edges, bool subdividable) : base(edges)
    {
        children = new List<SubdividableEdgeLoop>();
        isSubdividable = subdividable;
    }

    public SubdividableEdgeLoop(EdgeLoop edgeLoop, bool subdividable) : base(edgeLoop.GetEdgesEnumerable().ToArray())
    {
        children = new List<SubdividableEdgeLoop>();
        isSubdividable = subdividable;
    }

    public virtual ISubDivScheme<SubdividableEdgeLoop> GetDivScheme()
    {
        return new GetPieSections();
    }

    public virtual SubdividableEdgeLoop GetNextChild (EdgeLoopEdge[] edges)
    {
        return new SubdividableEdgeLoop(edges, true);
    }

    public bool IsSubdividable ()
    {
        return isSubdividable;
    }

    public List<EdgeLoopEdge[]> GetInteriorEdgeLoops()
    {
        List<EdgeLoopEdge[]> foundLoops = new List<EdgeLoopEdge[]>(); ;

        //starting at the first edge, do a search outward to collect all edges that are within 
        List<EdgeLoopEdge> allEdges = edges[0].CollectEdges<EdgeLoopEdge>(true, EdgeWithinLoop); //exterior and interior
        List<EdgeLoopEdge> exteriorEdges = edges;

        //get all edges that make up the interior of the loop
        List<EdgeLoopEdge> interiorEdges = allEdges.Except(exteriorEdges).ToList();

        //clean up edges that can't be part of polygons
        for (int i = interiorEdges.Count - 1; i >= 0; i--)
        {
            EdgeLoopEdge edge = interiorEdges[i];
            if (edge.a.NumConnections() <= 1 || edge.b.NumConnections() <= 1)
            {
                interiorEdges.Remove(edge);
                Debug.LogWarning("Detached an edge while getting interior edge loops");
            }
        }

        //keep track of the loops we've already found for each edge
        Dictionary<EdgeLoopEdge, List<EdgeLoopEdge[]>> edgeLoopCounts = new Dictionary<EdgeLoopEdge, List<EdgeLoopEdge[]>>();

        foreach (EdgeLoopEdge edge in interiorEdges)
        {
            edgeLoopCounts.Add(edge, new List<EdgeLoopEdge[]>());
        }

        foreach (EdgeLoopEdge edge in interiorEdges)
        {
            if (edgeLoopCounts[edge].Count == 2)
            {
                continue;//we've found both loops associated with this edge
            }
            EdgeLoopEdge[] ccwLoop = edge.GetLocalLoop(true).ToArray();
            EdgeLoopEdge[] cwLoop = edge.GetLocalLoop(false).ToArray();

            bool existingCcwLoop = false;
            bool existingCwLoop = false;

            foreach (EdgeLoopEdge[] foundLoop in edgeLoopCounts[edge])
            {
                if (EdgeLoop.IsEqual(foundLoop, ccwLoop))
                {
                    existingCcwLoop = true;
                }
                if (EdgeLoop.IsEqual(foundLoop, cwLoop))
                {
                    existingCwLoop = true;
                }
            }

            if (!existingCcwLoop)
            {
                foreach (EdgeLoopEdge subEdge in ccwLoop)
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
                foreach (EdgeLoopEdge subEdge in cwLoop)
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

	public Subdividable[] Subdivide()
	{
        children = GetDivScheme().GetChildren(this);
        return children.ToArray();
	}

    public void DebugDraw (float strength) 
    {
        if (children.Count == 0)
        {
            Color drawCol = getDebugColor();
            EnumerateEdges((EdgeLoopEdge eachEdge_) =>
            {
                UnityEngine.Debug.DrawLine(HelperFunctions.projVec2(eachEdge_.a.pt), HelperFunctions.projVec2(eachEdge_.b.pt), drawCol);
            });

            Debug.DrawLine(HelperFunctions.projVec2(GetPolygon().centroid), HelperFunctions.projVec2(GetPolygon().centroid) + Vector3.up * 0.2f, Color.white);
        }

        foreach (var child in children)
        {
            child.DebugDraw(strength);
        }
    }

    public virtual Color getDebugColor()
    {
        return Color.gray;
    }
}
