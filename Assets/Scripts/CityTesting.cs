using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityTesting : MonoBehaviour
{
    public City city;
    private SegmentGraph<float> roadGraph;
    public bool showCity = true;
    public bool showRoads;
    // Start is called before the first frame update
    void Start()
    {
        int numPoints = 15;
        List<Vector2> bPoly = new List<Vector2>();
        for (int i = 0; i < numPoints; i++)
        {
            float angle = (i / ((float)numPoints)) * Mathf.PI * 2;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            float rnd = Random.Range(1f, 1.5f);
            bPoly.Add(new Vector2((int)(cos * 8f * rnd), (int)(sin * 8f * rnd)));
        }
        //city = new City(bPoly.ToArray());
        //city.SubdivideR();

        //roadGraph = city.GetBoundaryGraph();

    }

    // Update is called once per frame
    void Update()
    {  
        if(showCity)
        {
            city.DebugDraw(1f); 
        }
        if(showRoads)
        {
            roadGraph.DebugGraph(0);
        }
    }
}
