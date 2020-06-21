using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSegmentGraph : MonoBehaviour
{
    public int hi = 0;
    SegmentGraph<float> graph;
    // Start is called before the first frame update
    void Start()
    {
        graph = new SegmentGraph<float>();

        for (int i = 0; i < 200; i ++)
        {
            graph.AddSegment(Random.insideUnitCircle*10, Random.insideUnitCircle*10, 0f);
        }

        graph.AddSegment(Vector2.zero, new Vector2(0, 10), 0f);
        graph.AddSegment(new Vector2(-3f, 1f), new Vector2(3f, 1f), 0f);
        graph.AddSegment(new Vector2(-3f, 2f), new Vector2(3f, 2f), 0f);
        graph.AddSegment(new Vector2(-3f, 3f), new Vector2(3f, 3f), 0f);

        graph.AddSegment(new Vector2(1f, 0), new Vector2(2f, 10f), 0f);


        graph.AddSegment(new Vector2(-3f, 5f), new Vector2(0f, 5f), 0f);

        graph.AddSegment(new Vector2(-5f, 10f), new Vector2(5f, 10f), 0f);
    }

	private void Update()
	{
        graph.DebugGraph(hi);
	}
}
