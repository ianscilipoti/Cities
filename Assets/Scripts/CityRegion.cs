using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polygon = EPPZ.Geometry.Model.Polygon;


public abstract class CityRegion : ISubdividable
{
    public WealthLevel wealth { get; set; }
    public string descriptor { get; set; }

    public City rootCity;

    public CityRegion(Vector2[] boundary, City rootCity, bool isSubdividable) : base(boundary, isSubdividable)
    {
        this.rootCity = rootCity;
    }

}

public enum WealthLevel
{
    Slums, Poor, Middleclass, Rich, SuperRich
}

