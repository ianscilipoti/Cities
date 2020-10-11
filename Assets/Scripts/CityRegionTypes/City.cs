using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Polygon = EPPZ.Geometry.Model.Polygon;

public class City : CityRegion
{
    public Vector2 entrence;
    public TextureSampler terrainGenerator;

    public static float MINSUBDIVAREA = 1000f;

    private List<CityEdge> foundEdgesCache;

    private const int MAXPASSES = 10;

    public Transform cityParent;

    public City (CityEdge[] boundaryLoop) : base(boundaryLoop, null, true, 1) 
    {
        entrence = Vector2.zero;
        terrainGenerator = new TerrainGenerator(120f, 80f);
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

        //after pass1, generate clipping for roads

        while (!allGenerated)
        {
            allGenerated = city.TryGenerateRecursive(pass);
            if (!allGenerated)
            {
                pass++;
            }

            switch (pass)
            {
                case (1):
                    List<CityEdge> allEdges = city.GetAllEdges();
                    foreach (CityEdge edge in allEdges)
                    {
                        //edge.width = Random.value + 1.5f;
                        BoundaryBuilder builder = new BoundaryBuilder(edge, city, null);
                        builder.PlaceBoundary();
                    }
                    break;

                case (2):
                    break;
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

    public override SubdividableEdgeLoop<CityEdge> GetNextChild (CityEdge[] edges) 
    {
        //new Plot(edges, this, depth+1);//
        return new Block(edges, this, depth+1);
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
        //return new Divide<CityEdge>(new CityEdgeFactory(), CityEdgeType.LandPath);
        return new CitySkeleton(new Vector2[]{entrence}, Random.Range(20, 40), CityEdge.GetRoadFactoryParams(depth));
        //return new GetBlocks(10, 10);
    } 
}
