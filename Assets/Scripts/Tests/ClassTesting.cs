using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassTesting : MonoBehaviour
{
	private void Start()
	{
        LinkedGraphVertex a = new LinkedGraphVertex(Vector2.zero);
        LinkedGraphVertex b = new LinkedGraphVertex(Vector2.one);
        EdgeLoopEdge test = new EdgeLoopEdge(a, b);

        LinkedGraphEdge test1 = test;

        if (test == test1)
        {
            print("yes!");
        }
	}
}