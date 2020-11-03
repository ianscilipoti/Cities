using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polygon = EPPZ.Geometry.Model.Polygon;


public abstract class CityRegion : SubdividableEdgeLoop<CityEdge>
{
    public WealthLevel wealth { get; set; }
    public string descriptor { get; set; }

    public City rootCity;
    public int depth { get; }

    public bool meshGenerated { get; set; }

    public CityRegion(CityEdge[] edges, City rootCity, bool isSubdividable, int depth) : base(edges, isSubdividable)
    {
        this.rootCity = rootCity;
        this.depth = depth;
    }

    //each city region type must specify when it generates
    public abstract int GetGenerationPass();

    public bool TryGenerateRecursive (int pass)
    {
        if (pass >= GetGenerationPass())
        {
            //subdivide protects against multiple subdivisions
            if (!meshGenerated)
            {
                GenerateMeshes();
            }
            if (IsSubdividable())
            {
                if (!IsSubdivided())
                {
                    Subdivide();
                }

                //SubdividableEdgeLoop<CityEdge>[] children = GetChildren();
                bool allChildrenGenerated = true;
                //if there are no children, this returns true
                foreach (SubdividableEdgeLoop<CityEdge> child in children)
                {
                    if (child is CityRegion)
                    {
                        CityRegion cityChild = (CityRegion)child;
                        bool childGenerated = cityChild.TryGenerateRecursive(pass);
                        allChildrenGenerated = allChildrenGenerated & childGenerated;
                    }
                }

                return allChildrenGenerated;
            }
            else
            {
                return true;
            }

        }
        return false;
    }

    public virtual void GenerateMeshes()
    {
        meshGenerated = true;
    }

    public void DebugDrawRecursiveLayered(float strength)
    {
        Color drawCol = getDebugColor();

        EnumerateEdges((EdgeLoopEdge eachEdge_) =>
        {
            UnityEngine.Debug.DrawLine(HelperFunctions.projVec2(eachEdge_.a.pt) + Vector3.up * depth * 40, HelperFunctions.projVec2(eachEdge_.b.pt) + Vector3.up * depth * 40, drawCol);
        });
        //drawCol = Color.white;
        //if (!IsConvex())
        //{
        //    drawCol = Color.red;
        //}
        //Debug.DrawLine(HelperFunctions.projVec2(GetPolygon().centroid), HelperFunctions.projVec2(GetPolygon().centroid) + Vector3.up * 0.1f, drawCol);

        foreach (var child in children)
        {
            if (child is CityRegion)
            {
                ((CityRegion)child).DebugDrawRecursiveLayered(strength);
            }
            else
            {
                child.DebugDrawRecursive(strength);
            }

        }
    }

}

public enum WealthLevel
{
    Slums, Poor, Middleclass, Rich, SuperRich
}

