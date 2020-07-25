using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class City : CityRegion
{
    public Vector2 entrence;
    public TextureSampler terrainGenerator;

    private List<CityEdge> foundEdgesCache;

    public City (CityEdge[] boundaryLoop) : base(boundaryLoop, null, true) 
    {
        entrence = Vector2.zero;
        terrainGenerator = new TerrainGenerator(20f);
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
    public static City GenerateCity () 
    {
        CityEdgeFactory factory = new CityEdgeFactory();
        CityEdge[] edges = GetPolygonEdges(8, 10f, 5f, factory, CityEdgeType.Wall);
        City city = new City(edges);

        double startTime = Time.realtimeSinceStartup;
        int pass = 0;
        bool allGenerated = false;
        while (!allGenerated)
        {
            allGenerated = city.TryGenerateRecursive(pass);
            if (!allGenerated)
            {
                pass++;
            }
        }
        Debug.Log("Finished generating in " + (Time.realtimeSinceStartup - startTime) + " seconds using " + (pass+1) + " passes.");
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

    public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        return new Block(edges, this);
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

    //Cities always subdivide with citySkeleton
    //this function could randomize what subdivscheme is returned easily
    public override ISubDivScheme<SubdividableEdgeLoop<CityEdge>> GetDivScheme () {
        return new CitySkeleton(new Vector2[]{entrence}, 1);
    } 
}
