using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

public abstract class ISubdividable : Polygon
{
    private List<ISubdividable> children;
    public abstract ISubDivScheme GetDivScheme();
    public abstract ISubdividable GetNextChild (Vector2[] boundary);
    public abstract Color getDebugColor();

    private List<Vector4> subdividedEdges;


    public bool isSubdividable;

    public ISubdividable(Vector2[] boundary, bool isSubdividable) : base(boundary) {
        this.isSubdividable = isSubdividable;
        children = new List<ISubdividable>();
        subdividedEdges = new List<Vector4>();
    }

    public ISubdividable[] getChildren() {
        return children.ToArray();
    }

    public void Subdivide(ISubDivScheme scheme) {
        children = scheme.GetChildren(this, out subdividedEdges);
    }

    public List<Vector4> CollectEdges () {
        List<Vector4> edges = new List<Vector4>();
        CollectEdgesRecursive(edges);
        return edges;
    }

    private void CollectEdgesRecursive (List<Vector4> collector)
    {
        collector.AddRange(subdividedEdges);
        foreach (ISubdividable child in getChildren())
        {
            child.CollectEdgesRecursive(collector);
        }
    }

    public void Subdivide()
    {
        children = GetDivScheme().GetChildren(this, out subdividedEdges);
    }

    public virtual void SubdivideR()
    {
        if (!isSubdividable)
        {
            return;
        }
        Subdivide();
        ISubdividable[] childrenSubReg = getChildren();
        for (int i = 0; i < childrenSubReg.Length; i++)
        {
            childrenSubReg[i].SubdivideR();
        }
    }

    public void DebugDraw (float strength) 
    {
        if (children.Count == 0)
        {
            Color drawCol = getDebugColor();
            EnumerateEdges((Edge eachEdge_) =>
            {
                UnityEngine.Debug.DrawLine(HelperFunctions.projVec2(eachEdge_.a), HelperFunctions.projVec2(eachEdge_.b), drawCol);
            });

            Debug.DrawLine(HelperFunctions.projVec2(centroid), HelperFunctions.projVec2(centroid) + Vector3.up * 0.2f, drawCol);
        }

        foreach (var child in children)
        {
            child.DebugDraw(strength);
        }
    }
}
