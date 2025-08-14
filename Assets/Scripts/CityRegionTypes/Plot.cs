using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

//a plot is a type of cityRegion that cannot be subdivided more. This is a technical classification that allows actual generation to happen
public class Plot : CityRegion
{
    private City city;
    private bool park;
    public Plot (CityEdge[] edges, City cityRoot, bool park, int depth) : base(edges, cityRoot, true, depth) {
        city = cityRoot;
        this.park = park;
    }

    public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        return new BuildablePlot(edges, rootCity, park, depth+1);
    }

    protected override List<SubdividableEdgeLoop<CityEdge>> Subdivide()
    {
        return new GetBuildablePlot(city).GetChildren(this);
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
	//public override ISubDivScheme<SubdividableEdgeLoop<CityEdge>> GetDivScheme () {
 //       return new GetBuildablePlot(city);
 //   }
}
