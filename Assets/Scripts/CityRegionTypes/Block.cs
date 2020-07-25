using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class Block : CityRegion
{

    public Block (CityEdge[] boundaryLoop, City cityRoot) : base(boundaryLoop, cityRoot, true) {
    }

	public override Color getDebugColor()
	{
        return new Color(0, 0.4f, 1f, 1f);
	}

    public override int GetGenerationPass ()
    {
        return 0;
    }

	public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        //return new Park(edges, rootCity);
        Polygon polygonTemplate = GetPolygon();
        float childArea = Mathf.Abs(polygonTemplate.area);
        float parkChance = 0.07f;
        //return new Plot(boundary, rootCity, false);
        if (Random.value < parkChance)
        {
            return new Park(edges, rootCity);
        }
        else
        {
            if (childArea > 3f)
            {
                return new Block(edges, rootCity);
            }
            else
            {
                return new Plot(edges, rootCity);
            } 
        }
    }

    //Cities always subdivide with citySkeleton
    //this function could randomize what subdivscheme is returned easily
    public override ISubDivScheme<SubdividableEdgeLoop<CityEdge>> GetDivScheme () {
        CityEdgeFactory factory = new CityEdgeFactory();
        //return new GetPieSections<CityEdge>(factory, CityEdgeType.LandPath);
        if (IsConvex())
        {
            return new GetPieSections<CityEdge>(factory, CityEdgeType.LandPath);
        }
        else
        {
            if (Random.value > 0.5f)
            {
                if (Random.value > 0.5f)
                {
                    return new GetBlocks(4, 3);
                }
                else
                {
                    return new GetBlocks(3, 4);
                }

            }
            else
            {
                return new GetPieSections<CityEdge>(factory, CityEdgeType.LandPath);
            } 
        }
    }
}
