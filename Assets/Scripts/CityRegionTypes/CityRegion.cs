using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polygon = EPPZ.Geometry.Model.Polygon;


public abstract class CityRegion : SubdividableEdgeLoop<CityEdge>
{
    public WealthLevel wealth { get; set; }
    public string descriptor { get; set; }

    public City rootCity;

    public CityRegion(CityEdge[] edges, City rootCity, bool isSubdividable) : base(edges, isSubdividable)
    {
        this.rootCity = rootCity;
    }

    //each city region type must specify when it generates
    public abstract int GetGenerationPass();

    public bool TryGenerateRecursive (int pass)
    {
        if (pass >= GetGenerationPass())
        {
            //subdivide protects against multiple subdivisions
            Subdivide();
            SubdividableEdgeLoop<CityEdge>[] children = GetChildren();
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

        return false;
    }

}

public enum WealthLevel
{
    Slums, Poor, Middleclass, Rich, SuperRich
}

