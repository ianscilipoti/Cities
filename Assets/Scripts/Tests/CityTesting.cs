using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityTesting : MonoBehaviour
{
    public City city;
    public bool showCity = true;
    public bool showRoads = false;
    public int debugDepth = 3;
    public int seed = 0;
    public float radius = 300f;
    List<EdgeLoopEdge> test;
    public bool refresh = false;
    public bool changeSeed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (changeSeed)
        {
            seed = Random.Range(0, 100000);
        }
       
        Random.InitState(seed);

        city = City.GenerateCity(radius);

    }

    // Update is called once per frame
    void Update()
    {  
        if(showCity)
        {
            city.DebugDrawRecursive(1f, debugDepth);
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

        }
        if (showRoads)
        {
            LinkedGraph<EdgeLoopEdge>.DebugDraw(test);
        }
    }
}
