using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLines : MonoBehaviour
{
    List<LinkedGraphEdge> edges;
    List<Color> colors;
    List<float> elevations;

    public bool displaySimpleEdges = false;
    List<Vector4> simpleEdges;

    public int edgeHighlightIndex = 0;
    public bool vertHighlight = false;
    public int vertIndex = 0;

	private void Start()
	{
        edges = new List<LinkedGraphEdge>();
        colors = new List<Color>();
        elevations = new List<float>();
        simpleEdges = new List<Vector4>();
	}

    public void AddSimple ( Vector4 edge)
    {
        simpleEdges.Add(edge);
    }

    public void AddEdge (LinkedGraphEdge edge, Color color)
    {
        if (edges.Count > 1)
        {
            foreach(LinkedGraphEdge edgeIter in edges)
            {
                if (edge == edgeIter)
                {
                    print("HARD DUP");
                }
                else if (edge.IsEqual(edgeIter))
                {
                    print("SOFT DUP");
                }
            }
            //print("DUPLICATE HARD");
        }
        //if (edges.Count > 1 && edge.IsEqual(edges[edges.Count - 1]))
        //{
        //    print("DUPLICATE SOFT");
        //}
                    
        edges.Add(edge);
        if (color != null)
        {
            colors.Add(color);
        }
        else
        {
            colors.Add(Color.white);
        }
        elevations.Add(edges.Count/10f);

        //print(edge.a.pt + ", " + edge.b.pt);
    }

	private void Update()
	{
        LinkedGraph<LinkedGraphEdge>.DebugDraw(edges, colors, elevations);

        if (edges.Count > 0)
        {
            LinkedGraphEdge higlightedEdge = edges[edgeHighlightIndex];
            if (vertHighlight)
            {
                LinkedGraphVertex vert;
                if (vertIndex == 0)
                {
                    vert = higlightedEdge.a;
                }
                else
                {
                    vert = higlightedEdge.b;
                }

                foreach (LinkedGraphEdge edge in vert.GetConnections())
                {
                    Debug.DrawLine(HelperFunctions.projVec2(edge.a.pt) + Vector3.up * 1.1f, HelperFunctions.projVec2(edge.b.pt) + Vector3.up * 1.1f, Color.green);
                }

                Debug.Log(vert.NumConnections() + " " + Time.time);
            }
            else
            {
                Debug.DrawLine(HelperFunctions.projVec2(higlightedEdge.a.pt) + Vector3.up * 1.1f, HelperFunctions.projVec2(higlightedEdge.b.pt) + Vector3.up * 1.1f, Color.green);
            }
        }

        if (displaySimpleEdges)
        {
            foreach (Vector4 edge in simpleEdges)
            {
                Debug.DrawLine(new Vector3(edge.x, -1f, edge.y), new Vector3(edge.z, -1f, edge.w), Color.yellow);
            }
        }
	}

}
