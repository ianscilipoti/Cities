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

    TownSimulation town;
    public Dictionary<Plot, TownResident> plotResidentMapping;

    public City (CityEdge[] boundaryLoop) : base(boundaryLoop, null, true, 1) 
    {
        entrence = Vector2.zero;
        terrainGenerator = new TerrainGenerator(120f, 5f);
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
    //this is a static method instead of a constructor because the constructor needs to call the base with the
    //parameter argument. However, I don't want to ask that a the caller always generate this. This is one of the setbacks
    //of all of the inheritence 
    public static City GenerateCity (float radius, int seed) 
    {
        Random.InitState(seed);

        CityEdgeFactory factory = new CityEdgeFactory();
        CityEdge[] edges = GetPolygonEdges(20, radius, radius/2, 70f, factory, new System.Object[]{ CityEdgeType.Wall, 5.55f});
        City city = new City(edges);

        double startTime = Time.realtimeSinceStartup;
        int pass = 0;
        bool allGenerated = false;

        //pass 0: subdivide the City instance into a series of blocks, recursivly subdivide these blocks until all have been subdivided into a plot
        //after pass 1, subdivide plots into buildings and road segments 

        while (!allGenerated)
        {
            allGenerated = city.TryGenerateRecursive(pass);
            if (!allGenerated)
            {
                pass++;
            }

            //calculate neighbors and town stuff
            if (pass == 1)
            {
                List<Plot> plots = city.GetPlots();
                city.plotResidentMapping = new Dictionary<Plot, TownResident>();


                if (TownSimulation.HasSaveFile(seed))
                {
                    try
                    {
                        city.town = new TownSimulation(seed);

                        foreach (Plot plot in plots)
                        {
                            city.plotResidentMapping.Add(plot, city.town.residents[plot.GetHash()]);
                        }

                        Debug.Log("loaded from file successfully");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("tried to load city residen data but failed: " + e);
                    }
                }
                else //calculate new residents etc
                {
                    city.plotResidentMapping = new Dictionary<Plot, TownResident>();
                    List<TownResident> residents = new List<TownResident>();

                    //for each edge of each city plot, mark that this plot is adjacent to this edge
                    //ideally there should be some adjacency list in the CityEdge, this is ok for now
                    foreach (Plot plot in plots)
                    {
                        TownResident resident = new TownResident(plot.GetHash());
                        residents.Add(resident);
                        city.plotResidentMapping.Add(plot, resident);
                    }
                    int maxLoops = 0;
                    int numNoNeighbor = 0;
                    foreach (Plot plot in plots)
                    {
                        CityEdge[] plotEdges = plot.GetEdges();
                        bool hasPlotNeighbor = false;
                        foreach (CityEdge edge in plotEdges)
                        {
                            List<IEdgeLoop> neighbors = edge.GetAdjacentLoops();
                            foreach (IEdgeLoop neighborLoop in neighbors)
                            {
                                if (neighborLoop is Plot && neighborLoop != plot)
                                {
                                    hasPlotNeighbor = true;
                                    city.plotResidentMapping[plot].neighborHashes.Add(city.plotResidentMapping[(Plot)neighborLoop].hash);
                                }
                            }
                            maxLoops = Mathf.Max(maxLoops, neighbors.Count);
                        }

                        if (!hasPlotNeighbor)
                        {
                            numNoNeighbor++;
                        }
                    }

                    city.town = new TownSimulation(residents, seed);
                    city.town.simulateTown(150);
                }
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
