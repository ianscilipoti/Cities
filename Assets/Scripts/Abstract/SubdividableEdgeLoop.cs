using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

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
