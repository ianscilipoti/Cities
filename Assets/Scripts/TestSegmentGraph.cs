using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSegmentGraph : MonoBehaviour
{
    public bool highlight = false;
    SegmentGraph<float> graph;
    List<EdgeLoopEdge> edges;
    List<EdgeLoopEdge> highlighted;
    List<EdgeLoopEdge> collectedEdges;

    public int highlightedInd = 0;
    public bool ccw = false;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(0);
        edges = new List<EdgeLoopEdge>();
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
            LinkedGraph<EdgeLoopEdge>.ConnectNewEdge(new Vector2(0, i/2f), new Vector2(10, i / 2f), factory, edges);
        }

        for (int i = 0; i < 20; i++)
        {
            LinkedGraph<EdgeLoopEdge>.ConnectNewEdge(new Vector2(i / 2f, 0), new Vector2(i / 2f, 10), factory, edges);
        }

        //LinkedGraphEdge.ConnectNewEdge(Vector2.zero, new Vector2(0, 10), factory, edges);

        //LinkedGraphEdge.ConnectNewEdge(Vector2.zero, new Vector2(5, 5), factory, edges);
        //LinkedGraph<EdgeLoopEdge>.ConnectNewEdge()

        LinkedGraphVertex bl = new LinkedGraphVertex(Vector2.down + Vector2.left);
        LinkedGraphVertex br = new LinkedGraphVertex(Vector2.down + Vector2.right);

        LinkedGraphVertex tl = new LinkedGraphVertex(Vector2.up + Vector2.left);
        LinkedGraphVertex tr = new LinkedGraphVertex(Vector2.up + Vector2.right);

        LinkedGraphVertex mid = new LinkedGraphVertex(Vector2.zero);

        //List<LinkedGraphEdge> knownEdges = new List<EdgeLoopEdge>();

        //EdgeLoopEdge b = EdgeLoopEdge.AddEdge(bl, br, factory, knownEdges);



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
                LinkedGraph<EdgeLoopEdge>.DebugDraw(highlighted); 
            }
            else
            {
                List<EdgeLoopEdge> e = new List<EdgeLoopEdge>();
                e.Add(edges[highlightedInd]);
                LinkedGraph<EdgeLoopEdge>.DebugDraw(e);
            }
              
        }
        else
        {
            LinkedGraph<EdgeLoopEdge>.DebugDraw(edges);
        }
       
	}
}
