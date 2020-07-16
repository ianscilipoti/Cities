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

    public SubdividableEdgeLoop(EdgeLoop edgeLoop, bool subdividable) : base(edgeLoop.GetEdges().ToArray())
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

    public List<EdgeLoop> GetInteriorEdgeLoops()
    {
        List<EdgeLoop> foundLoops = new List<EdgeLoop>(); ;

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
        Dictionary<EdgeLoopEdge, List<EdgeLoop>> edgeLoopCounts = new Dictionary<EdgeLoopEdge, List<EdgeLoop>>();

        foreach (EdgeLoopEdge edge in interiorEdges)
        {
            edgeLoopCounts.Add(edge, new List<EdgeLoop>());
        }

        foreach (EdgeLoopEdge edge in interiorEdges)
        {
            if (edgeLoopCounts[edge].Count == 2)
            {
                continue;//we've found both loops associated with this edge
            }
            EdgeLoop ccwLoop = new EdgeLoop(edge.GetLocalLoop(true).ToArray());
            EdgeLoop cwLoop = new EdgeLoop(edge.GetLocalLoop(false).ToArray());

            bool existingCcwLoop = false;
            bool existingCwLoop = false;

            foreach (EdgeLoop foundLoop in edgeLoopCounts[edge])
            {
                if (foundLoop.IsEqual(ccwLoop))
                {
                    existingCcwLoop = true;
                }
                if (foundLoop.IsEqual(cwLoop))
                {
                    existingCwLoop = true;
                }
            }

            if (!existingCcwLoop)
            {
                foreach (EdgeLoopEdge subEdge in ccwLoop.GetEdges())
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
                foreach (EdgeLoopEdge subEdge in cwLoop.GetEdges())
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
        return GetDivScheme().GetChildren(this).ToArray();
	}

    public void DebugDraw (float strength) 
    {
        //if (children.Count == 0)
        //{
        //    Color drawCol = getDebugColor();
        //    EnumerateEdges((Edge eachEdge_) =>
        //    {
        //        UnityEngine.Debug.DrawLine(HelperFunctions.projVec2(eachEdge_.a), HelperFunctions.projVec2(eachEdge_.b), drawCol);
        //    });

        //    Debug.DrawLine(HelperFunctions.projVec2(centroid), HelperFunctions.projVec2(centroid) + Vector3.up * 0.2f, drawCol);
        //}

        foreach (var child in children)
        {
            child.DebugDraw(strength);
        }
    }

    public virtual Color getDebugColor()
    {
        return Color.gray;
    }

    public static List<SubdividableEdgeLoop> CollectInteriorLoops (EdgeLoop boundary)
    {
        //List<EdgeLoopEdge> interiorEdges = boundary.GetInteriorEdgeLoopEdges();
        //List<EdgeLoopEdge> exteriorEdges = new List<EdgeLoopEdge>(boundary.GetEdges());

        return null;
    }
}
