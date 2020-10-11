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
            LinkedGraph<EdgeLoopEdge>.ConnectNewEdge(new Vector2(0, i/2f) * 10f, new Vector2(10, i / 2f) * 10f, factory, edges);
        }

        for (int i = 0; i < 20; i++)
        {
            LinkedGraph<EdgeLoopEdge>.ConnectNewEdge(new Vector2(i / 2f, 0) * 10f, new Vector2(i / 2f, 10) * 10f, factory, edges);
        }

        //LinkedGraphEdge.ConnectNewEdge(Vector2.zero, new Vector2(0, 10), factory, edges);

        //LinkedGraphEdge.ConnectNewEdge(Vector2.zero, new Vector2(5, 5), factory, edges);
        //LinkedGraph<EdgeLoopEdge>.ConnectNewEdge()

        //LinkedGraphVertex bl = new LinkedGraphVertex(Vector2.down + Vector2.left);
        //LinkedGraphVertex br = new LinkedGraphVertex(Vector2.down + Vector2.right);

        //LinkedGraphVertex tl = new LinkedGraphVertex(Vector2.up + Vector2.left);
        //LinkedGraphVertex tr = new LinkedGraphVertex(Vector2.up + Vector2.right);

        //LinkedGraphVertex mid = new LinkedGraphVertex(Vector2.zero);

        //EdgeLoopEdge b = LinkedGraph<EdgeLoopEdge>.AddEdge(br, bl, factory, edges);
        //EdgeLoopEdge r = LinkedGraph<EdgeLoopEdge>.AddEdge(br, tr, factory, edges);
        //EdgeLoopEdge t = LinkedGraph<EdgeLoopEdge>.AddEdge(tl, tr, factory, edges);
        //EdgeLoopEdge l = LinkedGraph<EdgeLoopEdge>.AddEdge(bl, tl, factory, edges);

        //EdgeLoopEdge blm = LinkedGraph<EdgeLoopEdge>.AddEdge(bl, mid, factory, edges);
        //EdgeLoopEdge brm = LinkedGraph<EdgeLoopEdge>.AddEdge(br, mid, factory, edges);
        //EdgeLoopEdge trm = LinkedGraph<EdgeLoopEdge>.AddEdge(tr, mid, factory, edges);
        //EdgeLoopEdge tlm = LinkedGraph<EdgeLoopEdge>.AddEdge(tl, mid, factory, edges);

        //SubdividableEdgeLoop<EdgeLoopEdge> squareLoop = new SubdividableEdgeLoop<EdgeLoopEdge>(new EdgeLoopEdge[] {b,r,t,l}, true);


        ////LinkedGraph<EdgeLoopEdge> test = (LinkedGraph<EdgeLoopEdge>)new LinkedGraph<CityEdge>();

        //print(squareLoop.EdgeFollowsWinding(b));
        //print(squareLoop.EdgeFollowsWinding(r));
        //print(squareLoop.EdgeFollowsWinding(t));
        //print(squareLoop.EdgeFollowsWinding(l));

        //List<EdgeLoopEdge[]> children = squareLoop.GetInteriorEdgeLoops();

        //List<float> test = new List<float>();
        //test.Add(0);
        //test.Add(1);
        //test.Add(2);

        //test.Insert(2, 1.5f);

        //test.RemoveAt(2);

        //test.Insert(2, 1.5f);
        //test.Insert(3, 1.7f);


        //foreach (float i in test)
        //{
        //    print(i);
        //}

        //print("poo");
        //for (int i = 0; i < 5; i ++)
        //{
        //    print(i + " %" + (i - 1) % 5);
        //}

    }

	private void Update()
	{
        //highlighted = ((EdgeLoopEdge)edges[highlightedInd]).GetLocalLoop(ccw);
        //Debug.DrawLine((Vector3)edges[highlightedInd].a.pt + Vector3.forward, (Vector3)edges[highlightedInd].b.pt + Vector3.forward, Color.red);
        //if(highlight)
        //{
            
        //    if (highlighted != null)
        //    {
        //        LinkedGraph<EdgeLoopEdge>.DebugDraw(highlighted); 
        //    }
        //    else
        //    {
        //        List<EdgeLoopEdge> e = new List<EdgeLoopEdge>();
        //        e.Add(edges[highlightedInd]);
        //        LinkedGraph<EdgeLoopEdge>.DebugDraw(e);
        //    }
              
        //}
        //else
        //{
        //    
        //}
        LinkedGraph<EdgeLoopEdge>.DebugDraw(edges);
	}
}
