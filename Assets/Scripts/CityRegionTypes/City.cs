using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class City : CityRegion
{
    public Vector2 entrence;
    public TextureSampler terrainGenerator;

    public static float MINSUBDIVAREA = 3500f;

    private List<CityEdge> foundEdgesCache;

    private const int MAXPASSES = 10;

    public Transform cityParent;

    public City (CityEdge[] boundaryLoop) : base(boundaryLoop, null, true, 1) 
    {
        entrence = Vector2.zero;
        terrainGenerator = new TerrainGenerator(120f, 30f);
        cityParent = new GameObject("CityParent", typeof(Transform)).transform;
    }
    
    public float SampleElevation (float x, float z)
    {
        return terrainGenerator.get(x, z);
    }

    public override int GetGenerationPass()
    {
        return 0;
    }

    //entry point of all generation. We excecute passes until all children are generated
    public static City GenerateCity (float radius) 
    {
        CityEdgeFactory factory = new CityEdgeFactory();
        CityEdge[] edges = GetPolygonEdges(20, radius, radius/2, 70f, factory, new System.Object[]{ CityEdgeType.Wall, 5.55f});
        City city = new City(edges);

        double startTime = Time.realtimeSinceStartup;
        int pass = 0;
        bool allGenerated = false;

        //pass 0: subdivide the City instance into a series of blocks, recursivly subdivide these blocks until all have been subdivided into a plot
        //after pass 1, generate clipping for roads

        while (!allGenerated)
        {
            allGenerated = city.TryGenerateRecursive(pass);
            if (!allGenerated)
            {
                pass++;
            }

            //calculate neighbors
            if (pass == 1)
            {
                var neighborMap = city.BuildNeighborMap();
                Debug.Log($"Computed neighbor map for {neighborMap.Count} non-park plots");
            }

            if (pass > MAXPASSES)
            {
                Debug.LogError("Too many passes. Failing.");
                return city;
            }
        }
        Debug.Log("Finished generating in " + (Time.realtimeSinceStartup - startTime) + " seconds using " + (pass+1) + " passes.");

        Debug.Log("City is verified (" + city.VerifyRecursive() + ")");

        return city;
    }

    public List<CityEdge> GetAllEdges ()
    {
        if (foundEdgesCache != null)
        {
            return foundEdgesCache;
        }
        else 
        {
            List<CityEdge> foundEdges = edges[0].CollectEdges<CityEdge>(false, null);
            return foundEdges; 
        }
    }

    public override Color getDebugColor()
    {
        return Color.red;
    }

    //Cities always subdivide into Block instances. Blocks are the generic term for a section of the city
    public override SubdividableEdgeLoop<CityEdge> GetNextChild(CityEdge[] edges)
    {
        return new Block(edges, this, depth + 1);
    }

    protected override List<SubdividableEdgeLoop<CityEdge>> Subdivide()
    {
        // Direct subdivision logic - no scheme needed
        CitySkeleton citySkeleton = new CitySkeleton(new Vector2[] { entrence }, Random.Range(10, 20), CityEdge.GetRoadFactoryParams(depth));

        return citySkeleton.GetChildren(this);
    }

    public List<Plot> GetPlots () 
    {
        List<Plot> plots = new List<Plot>();
        GetPlotsRecursive(this, plots);
        return plots;
    }

    private void GetPlotsRecursive (SubdividableEdgeLoop<CityEdge> some, List<Plot> storage)
    {
        if (some is Plot)
        {
            storage.Add((Plot)some);
        }
        else
        {
            SubdividableEdgeLoop<CityEdge>[] children = some.GetChildren();
            foreach (SubdividableEdgeLoop<CityEdge> child in children)
            {
                GetPlotsRecursive(child, storage);
            }
        }
    }

	// Build neighbor dictionary mapping non-park BuildablePlot to list of adjacent non-park BuildablePlots
	public Dictionary<BuildablePlot, List<BuildablePlot>> BuildNeighborMap()
	{
		var allBuildable = new List<BuildablePlot>();
		CollectBuildablePlotsRecursive(this, allBuildable);

		// Build edge -> plots map to find adjacency via shared CityEdge references
		var edgeToPlots = new Dictionary<CityEdge, List<BuildablePlot>>();
		foreach (var plot in allBuildable)
		{
			foreach (var edge in plot.GetEdges())
			{
				var cityEdge = edge as CityEdge;
				if (cityEdge == null) continue;
				if (!edgeToPlots.TryGetValue(cityEdge, out var list))
				{
					list = new List<BuildablePlot>(2);
					edgeToPlots[cityEdge] = list;
				}
				if (!list.Contains(plot)) list.Add(plot);
			}
		}

		var neighbors = new Dictionary<BuildablePlot, List<BuildablePlot>>();
		foreach (var plot in allBuildable)
		{
			if (plot.IsPark) continue; // skip parks as keys
			var set = new HashSet<BuildablePlot>();
			foreach (var edge in plot.GetEdges())
			{
				var cityEdge = edge as CityEdge;
				if (cityEdge == null) continue;
				if (edgeToPlots.TryGetValue(cityEdge, out var list))
				{
					foreach (var other in list)
					{
						if (other != plot && !other.IsPark) set.Add(other);
					}
				}
			}
			neighbors[plot] = new List<BuildablePlot>(set);
		}

		return neighbors;
	}

	private void CollectBuildablePlotsRecursive(SubdividableEdgeLoop<CityEdge> node, List<BuildablePlot> storage)
	{
		if (node is BuildablePlot)
		{
			storage.Add((BuildablePlot)node);
			return;
		}
		var children = node.GetChildren();
		for (int i = 0; i < children.Length; i++)
		{
			CollectBuildablePlotsRecursive(children[i], storage);
		}
	}
}
