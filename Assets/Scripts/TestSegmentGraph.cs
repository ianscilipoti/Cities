using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSegmentGraph : MonoBehaviour
{
    public bool highlight = false;
    SegmentGraph<float> graph;
    List<LinkedGraphEdge> edges;
    List<EdgeLoopEdge> highlighted;
    List<EdgeLoopEdge> collectedEdges;

    public int highlightedInd = 0;
    public bool ccw = false;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(0);
        edges = new List<LinkedGraphEdge>();
        //graph = new SegmentGraph<float>();



        //graph.AddSegment(Vector2.zero, new Vector2(0, 10), 0f);
        //graph.AddSegment(new Vector2(-3f, 1f), new Vector2(3f, 1f), 0f);
        //graph.AddSegment(new Vector2(-3f, 2f), new Vector2(3f, 2f), 0f);
        //graph.AddSegment(new Vector2(-3f, 3f), new Vector2(3f, 3f), 0f);

        //graph.AddSegment(new Vector2(1f, 0), new Vector2(2f, 10f), 0f);


        //graph.AddSegment(new Vector2(-3f, 5f), new Vector2(0f, 5f), 0f);

        //graph.AddSegment(new Vector2(-5f, 10f), new Vector2(5f, 10f), 0f);
        EdgeLoopEdgeFactory factory = new EdgeLoopEdgeFactory();
        for (int i = 0; i < 20; i++)
        {
            LinkedGraphEdge.ConnectNewEdge(new Vector2(0, i/2f), new Vector2(10, i / 2f), factory, edges);
        }

        for (int i = 0; i < 20; i++)
        {
            LinkedGraphEdge.ConnectNewEdge(new Vector2(i / 2f, 0), new Vector2(i / 2f, 10), factory, edges);
        }

        //LinkedGraphEdge.ConnectNewEdge(Vector2.zero, new Vector2(0, 10), factory, edges);

        //LinkedGraphEdge.ConnectNewEdge(Vector2.zero, new Vector2(5, 5), factory, edges);





        print(edges.Count);

    }

	private void Update()
	{
        highlighted = ((EdgeLoopEdge)edges[highlightedInd]).GetLocalLoop(ccw);
        Debug.DrawLine((Vector3)edges[highlightedInd].a.pt + Vector3.forward, (Vector3)edges[highlightedInd].b.pt + Vector3.forward, Color.red);
        if(highlight)
        {
            
            if (highlighted != null)
            {
                List<LinkedGraphEdge> hiEd = new List<LinkedGraphEdge>();
                foreach (EdgeLoopEdge ed in highlighted)
                {
                    hiEd.Add(ed);
                }
                LinkedGraphEdge.DebugDraw<LinkedGraphEdge>(hiEd); 
            }
            else
            {
                List<EdgeLoopEdge> e = new List<EdgeLoopEdge>();
                e.Add((EdgeLoopEdge)edges[highlightedInd]);
                LinkedGraphEdge.DebugDraw<EdgeLoopEdge>(e);
            }
              
        }
        else
        {
            LinkedGraphEdge.DebugDraw<LinkedGraphEdge>(edges);
        }
       
	}
}
