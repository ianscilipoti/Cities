using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class Block : CityRegion
{

    public Block (CityEdge[] boundaryLoop, City cityRoot, int depth) : base(boundaryLoop, cityRoot, true, depth) {
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
        float parkChance = 0.025f;
        if (Random.value < parkChance)
        {
            return new Plot(edges, rootCity, true, depth + 1);
        }
        else
        {
            if (childArea > City.MINSUBDIVAREA)
            {
                return new Block(edges, rootCity, depth+1);
            }
            else
            {
                return new Plot(edges, rootCity, false, depth+1);
            } 
        }
    }

    //Cities always subdivide with citySkeleton
    //this function could randomize what subdivscheme is returned easily
    public override ISubDivScheme<SubdividableEdgeLoop<CityEdge>> GetDivScheme () {
        CityEdgeFactory factory = new CityEdgeFactory();
        System.Object[] factoryParams = CityEdge.GetRoadFactoryParams(depth);

        if (Random.value > 0.4 || GetPolygon().area < City.MINSUBDIVAREA * 2f)
        {
            return new GetBlocks(factoryParams);
        }
        else
        {
            return new CircularCenter<CityEdge>(factory, factoryParams);
        }
    }
}
