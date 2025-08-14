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

    protected override List<SubdividableEdgeLoop<CityEdge>> Subdivide()
    {
        CityEdgeFactory factory = new CityEdgeFactory();
        System.Object[] factoryParams = CityEdge.GetRoadFactoryParams(depth);

        EdgeLoopSubdivider<CityEdge> subDivScheme;

        if (Random.value > 0.4 || GetPolygon().area < City.MINSUBDIVAREA * 2f)
        {
            subDivScheme = new GetBlocks(factoryParams);
        }
        else
        {
            subDivScheme = new CircularCenter<CityEdge>(factory, factoryParams);
        }

        return subDivScheme.GetChildren(this);
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
}
