using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

//a plot is a type of cityRegion that cannot be subdivided more. This is a technical classification that allows actual generation to happen
public class Road : CityRegion
{
    private City city;
    private Plot inFootprintPlot;
    bool door;
    public Road (CityEdge[] edges, City cityRoot, Plot isInFootprintPlot, bool isDoor) : base(edges, cityRoot, false, -1) {
        city = cityRoot;
        inFootprintPlot = isInFootprintPlot;
        door = isDoor;
    }

    public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        return null;
    }

    public override int GetGenerationPass()
    {
        return 1;
    }

    public override void GenerateMeshes()
    {
        base.GenerateMeshes();
        //build road
        BoundaryBuilder builder = new BoundaryBuilder(this, city, null);
        builder.PlaceBoundary();

        if (door && inFootprintPlot != null)
        {
            TownResident resident = city.plotResidentMapping[inFootprintPlot];
            TownResidentActor actor = Resources.Load<TownResidentActor>("ResidentActor");
            Vector2 centroid = GetCenter();
            TownResidentActor newActor = GameObject.Instantiate(actor, new Vector3(centroid.x, city.SampleElevation(centroid.x, centroid.y), centroid.y), Quaternion.identity);

            newActor.data = resident;
        }
    }

	public override Color getDebugColor()
	{
        return new Color(0.5f, 0.5f, 0, 0.8f);
	}
}
