using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class Park : CityRegion
{
    public Park (CityEdge[] edges, City cityRoot) : base(edges, cityRoot, false) {
    }

	public override Color getDebugColor()
	{
        return new Color(0, 1f, 0.1f, 0.6f);
	}

    public override int GetGenerationPass()
    {
        return 0;
    }

    public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        return null;//unsubdividable
    }

    //Cities always subdivide with citySkeleton
    //this function could randomize what subdivscheme is returned easily
    public override ISubDivScheme<SubdividableEdgeLoop<CityEdge>> GetDivScheme () {
        return null;//unsubdividable
    }
}
