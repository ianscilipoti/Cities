using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

//a plot is a type of cityRegion that cannot be subdivided more. This is a technical classification that allows actual generation to happen
public class Plot : CityRegion
{
    public Plot (CityEdge[] edges, City cityRoot) : base(edges, cityRoot, false) {
    }

    public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        return null;//new Block(boundary, rootCity);
    }

    public override int GetGenerationPass()
    {
        return 1;
    }

	public override Color getDebugColor()
	{
        return new Color(0.5f, 0.5f, 0, 0.8f);
	}

	//Cities always subdivide with citySkeleton
	//this function could randomize what subdivscheme is returned easily
	public override ISubDivScheme<SubdividableEdgeLoop<CityEdge>> GetDivScheme () {
        return null;//new GetBlocks(10, 5);
    }
}
