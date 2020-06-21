using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class Plot : CityRegion
{
    public Plot (Vector2[] boundary, City cityRoot, bool subdividable) : base(boundary, cityRoot, subdividable) {
    }

    public override ISubdividable GetNextChild (Vector2[] boundary) 
    {
        return new Block(boundary, rootCity);
    }

	public override Color getDebugColor()
	{
        return new Color(0.5f, 0.5f, 0, 0.8f);
	}

	//Cities always subdivide with citySkeleton
	//this function could randomize what subdivscheme is returned easily
	public override ISubDivScheme GetDivScheme () {
        return new GetBlocks(10, 5);
    }
}
