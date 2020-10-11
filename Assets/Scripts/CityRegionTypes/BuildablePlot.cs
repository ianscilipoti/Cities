using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

//a plot is a type of cityRegion that cannot be subdivided more. This is a technical classification that allows actual generation to happen
public class BuildablePlot : CityRegion
{
    private City city;
    private bool park;
    const float minBuildingArea = 25f;
    public BuildablePlot (CityEdge[] edges, City cityRoot, bool park, int depth) : base(edges, cityRoot, false, depth) {
        city = cityRoot;
        this.park = park;
    }

    public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        return null;//new Block(boundary, rootCity);
    }

    public override int GetGenerationPass()
    {
        return 2;
    }

	public override void GenerateMeshes()
	{
		base.GenerateMeshes();
        if (!park)
        {
            Polygon asPoly = GetPolygon();
            Vector2 centroid = asPoly.centroid;
            if (asPoly.area > minBuildingArea)
            {
                int floors = Mathf.CeilToInt(1 + Mathf.Pow(Random.value, 8) * 5);

                Building building = new Building(this, Random.Range(2.5f, 3f), floors, city);
                building.PlaceBuilding();
            }
            else
            {
                LandBuilder land = new LandBuilder(this, city);
                land.PlaceLand();
            }
        }
        else
        {
            LandBuilder land = new LandBuilder(this, city);
            land.PlaceLand();
        }
	}

	public override Color getDebugColor()
	{
        return new Color(0.5f, 0.5f, 0, 0.8f);
	}

	//Cities always subdivide with citySkeleton
	//this function could randomize what subdivscheme is returned easily
	public override ISubDivScheme<SubdividableEdgeLoop<CityEdge>> GetDivScheme () {
        return null;
    }
}
