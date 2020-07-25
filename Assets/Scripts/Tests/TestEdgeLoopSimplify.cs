using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestEdgeLoopSimplify : MonoBehaviour
{
    public float simpAngle = 20f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2[] pts = GetComponent<EdgeCollider2D>().points;

        //EdgeLoop testLoop = new EdgeLoop(pts);

        //Vector2[] simplifiedPts = testLoop.GetSimplifiedPoints(simpAngle * Mathf.Deg2Rad);
        //testLoop = new EdgeLoop(simplifiedPts);
        //testLoop.DebugDraw(1f);
    }
}
