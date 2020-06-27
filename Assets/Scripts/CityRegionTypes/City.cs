using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class City : CityRegion
{

    public Vector2 entrence;

    public City (EdgeLoopEdge[] boundaryLoop) : base(boundaryLoop, null, true) 
    {
        entrence = Vector2.zero;
    }

    public override Color getDebugColor()
    {
        return Color.red;
    }

    public override SubdividableEdgeLoop GetNextChild (EdgeLoopEdge[] edges) 
    {
        return new Block(edges, this);
    }

    //Cities always subdivide with citySkeleton
    //this function could randomize what subdivscheme is returned easily
    public override ISubDivScheme<SubdividableEdgeLoop> GetDivScheme () {
        return (ISubDivScheme<SubdividableEdgeLoop>)new CitySkeleton(new Vector2[]{entrence}, 16);
    } 
}
