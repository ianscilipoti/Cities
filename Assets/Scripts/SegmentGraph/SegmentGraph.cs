using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;
    
public class SegmentGraph <EdgeInfo>
{
    public HashSet<SegmentGraphSegment<EdgeInfo>> segments;
    public HashSet<SegmentGraphVertex<EdgeInfo>> vertices;

    //the square of the minimum merge dist. sqr bc it allows for faster distance calculations
    public const float VERT_MERGE_DIST_SQR = 0.001f;

    public SegmentGraph () {
        segments = new HashSet<SegmentGraphSegment<EdgeInfo>>();
        vertices = new HashSet<SegmentGraphVertex<EdgeInfo>>();
    }

    //returns a pointer to the vertex at pt if it exists. null otherwise
    private SegmentGraphVertex<EdgeInfo> hasVertex (Vector2 pt) 
    {
        foreach (SegmentGraphVertex<EdgeInfo> vert in vertices)
        {
            if ((vert.position - pt).sqrMagnitude < VERT_MERGE_DIST_SQR)
            {
                return vert;
            }
        }
        return null;
    }

    private void SubdivSegment (SegmentGraphSegment<EdgeInfo> segment, SegmentGraphVertex<EdgeInfo> midPoint) 
    {
        SegmentGraphVertex<EdgeInfo> aVert = segment.aVert;
        SegmentGraphVertex<EdgeInfo> bVert = segment.bVert;
        RemoveSegment(segment, false);

        AddSegment(aVert, midPoint, segment.info);
        AddSegment(bVert, midPoint, segment.info);
    }

    private void RemoveSegment (SegmentGraphSegment<EdgeInfo> segment, bool cleanVertices)
    {
        segment.aVert.RemoveConnection(segment);
        segment.bVert.RemoveConnection(segment);
        segments.Remove(segment);
        if (cleanVertices)
        {
            //remove verts no longer connected
            if (segment.aVert.NumConnections() == 0)
            {
                RemoveVertex(segment.aVert);
            }

            if (segment.bVert.NumConnections() == 0)
            {
                RemoveVertex(segment.bVert);
            } 
        }
    }

    public void AddSegment (SegmentGraphVertex<EdgeInfo> aVert, SegmentGraphVertex<EdgeInfo> bVert, EdgeInfo info) 
    {
        SegmentGraphSegment<EdgeInfo> newSegment = new SegmentGraphSegment<EdgeInfo>(aVert, bVert, info);
        segments.Add(newSegment);
        aVert.AddConnection(newSegment);
        bVert.AddConnection(newSegment);
    }

    private SegmentGraphVertex<EdgeInfo> AddVertex (Vector2 pt) 
    {
        SegmentGraphVertex<EdgeInfo> vert = new SegmentGraphVertex<EdgeInfo>(pt);
        vertices.Add(vert);
        return vert;
    }

    private void RemoveVertex (SegmentGraphVertex<EdgeInfo> vert) 
    {
        vertices.Remove(vert);
    }

    public void AddSegment (Vector2 a, Vector2 b, EdgeInfo info)
    {
        SegmentGraphVertex<EdgeInfo> aVert = hasVertex(a);
        SegmentGraphVertex<EdgeInfo> bVert = hasVertex(b); 

        if (aVert == null) {
            aVert = AddVertex(a);
        }
        if (bVert == null)
        {
            bVert = AddVertex(b);
        }

        //use for checking intersections
        Segment testingSegment = Segment.SegmentWithPoints(a, b);
        Rect testingSegBounds = testingSegment.ExpandedBounds(VERT_MERGE_DIST_SQR);

        List<SegmentGraphVertex<EdgeInfo>> intersectionVertices = new List<SegmentGraphVertex<EdgeInfo>>();
        intersectionVertices.Add(aVert);//these will be sorted later
        intersectionVertices.Add(bVert);

        SegmentGraphSegment<EdgeInfo>[] segmentsCpy = new SegmentGraphSegment<EdgeInfo>[segments.Count];
        segments.CopyTo(segmentsCpy);
        //check if new vertices are on another line
        foreach (SegmentGraphSegment<EdgeInfo> seg in segmentsCpy)
        {
            if (!seg.precalcBounds.Overlaps(testingSegBounds)) {
                continue;
            }
            if (seg.ContainsPoint(a, VERT_MERGE_DIST_SQR)) {
                SubdivSegment(seg, aVert);
            }
            else if (seg.ContainsPoint(b, VERT_MERGE_DIST_SQR))
            {
                SubdivSegment(seg, bVert);
            }
            else if (testingSegment.ContainsPoint(seg.a))
            {
                intersectionVertices.Add(seg.aVert);
            }
            else if (testingSegment.ContainsPoint(seg.b))
            {
                intersectionVertices.Add(seg.bVert);
            }
            else
            {
                Vector2 intersectionPoint = Vector2.zero;
                if (seg.IntersectionWithSegment(testingSegment, out intersectionPoint))
                {
                    SegmentGraphVertex<EdgeInfo> midPtVert = AddVertex(intersectionPoint);
                    SubdivSegment(seg, midPtVert);
                    intersectionVertices.Add(midPtVert);
                }
            }
        }
        Vector2 sortDirection = b - a;

        intersectionVertices.Sort((first, second) =>
                                  (first.position - a).sqrMagnitude.CompareTo(
                                      (second.position - a).sqrMagnitude));

        for (int i = 0; i < intersectionVertices.Count-1; i ++)
        {
            AddSegment(intersectionVertices[i], intersectionVertices[i + 1], info);
        }
    }

    public void DebugGraph (int highlightVert) {

        Color[] cols = new Color[] {Color.black, Color.red, Color.green, Color.blue, Color.yellow, Color.gray };
        //SegmentGraphVertex<EdgeInfo> highlightVertex = vertices[Mathf.Clamp(highlightVert, 0, vertices.Count-1)];

        foreach (SegmentGraphSegment<EdgeInfo> seg in segments)
        {
            Color col = Color.white;

            //if (highlightVertex.connections.Contains(seg))
            //{
            //    col = Color.red;
            //}
            
            Debug.DrawLine(HelperFunctions.projVec2(seg.a), HelperFunctions.projVec2(seg.b), col);
        }

        foreach (SegmentGraphVertex<EdgeInfo> vert in vertices)
        {
            Debug.DrawLine(HelperFunctions.projVec2(vert.position), HelperFunctions.projVec2(vert.position) + Vector3.up * 0.1f, cols[Mathf.Min(vert.NumConnections(), cols.Length-1)]);
        }
    }
}
