﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class Block : CityRegion
{

    public Block (EdgeLoopEdge[] boundaryLoop, City cityRoot) : base(boundaryLoop, cityRoot, false) {
    }

	public override Color getDebugColor()
	{
        return new Color(0, 0.4f, 1f, 0.6f);
	}

	public override SubdividableEdgeLoop GetNextChild (EdgeLoopEdge[] edges) 
    {
        return null;
        //Polygon polygonTemplate = new Polygon(boundary);
        //float childArea = Mathf.Abs(polygonTemplate.area);
        //float parkChance = 0.07f;
        ////return new Plot(boundary, rootCity, false);
        //if (Random.value < parkChance)
        //{
        //    return new Park(boundary, rootCity);
        //}
        //else
        //{
        //    if (childArea > 0.1f)
        //    {
        //        return new Block(boundary, rootCity);
        //    }
        //    else
        //    {
        //        return new Plot(boundary, rootCity, false);
        //    } 
        //}
    }

    //Cities always subdivide with citySkeleton
    //this function could randomize what subdivscheme is returned easily
    public override ISubDivScheme<SubdividableEdgeLoop> GetDivScheme () {
        //if (Random.value > 0.5f)
        //{
        //    if (Random.value > 0.5f)
        //    {
        //        return new GetBlocks(4, 3);
        //    }
        //    else
        //    {
        //        return new GetBlocks(3, 4);
        //    }

        //}
        //else
        //{
        //    return new GetPieSections();
        //}
        return null;
    }
}