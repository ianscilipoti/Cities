using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityTesting : MonoBehaviour
{
    public City city;
    private SegmentGraph<float> roadGraph;
    public bool showCity = true;
    public bool showRoads = false;
    public int seed = 0;
    public float radius = 300f;
    List<EdgeLoopEdge> test;
    public bool refresh = false;
    public bool changeSeed = false;
    // Start is called before the first frame update
    void Start()
    {
        //int numPoints = 15;
        //List<Vector2> bPoly = new List<Vector2>();
        //for (int i = 0; i < numPoints; i++)
        //{
        //    float angle = (i / ((float)numPoints)) * Mathf.PI * 2;
        //    float cos = Mathf.Cos(angle);
        //    float sin = Mathf.Sin(angle);
        //    float rnd = Random.Range(1f, 1.5f);
        //    bPoly.Add(new Vector2((int)(cos * 8f * rnd), (int)(sin * 8f * rnd)));
        //}
        Random.InitState(seed);
        city = City.GenerateCity(radius);

        List<CityEdge> testEdges = city.GetAllEdges();
        test = new List<EdgeLoopEdge>();
        foreach (CityEdge t in testEdges)
        {
            test.Add(t);
        }
        //city.SubdivideRecursive();
        //roadGraph = city.GetBoundaryGraph();

    }

    // Update is called once per frame
    void Update()
    {  
        if(showCity)
        {
            city.DebugDrawRecursiveLayered(1f); 
        }

        if (refresh)
        {
            if (changeSeed)
            {
                seed = Random.Range(0, 100000);
            }

            refresh = false;
            Random.InitState(seed);
            Destroy(city.cityParent.gameObject);
            city = City.GenerateCity(radius);

            List<CityEdge> testEdges = city.GetAllEdges();
            test = new List<EdgeLoopEdge>();
            foreach (CityEdge t in testEdges)
            {
                test.Add(t);
            }

        }
        if (showRoads)
        {
            LinkedGraph<EdgeLoopEdge>.DebugDraw(test);
        }
    }
}
