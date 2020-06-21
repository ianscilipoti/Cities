using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

public class SegmentGraphSegment <EdgeInfo> : Segment
{
    public SegmentGraphVertex<EdgeInfo> aVert;
    public SegmentGraphVertex<EdgeInfo> bVert;

    public EdgeInfo info;

    public Rect precalcBounds;

    public SegmentGraphSegment (SegmentGraphVertex<EdgeInfo> aVert, SegmentGraphVertex<EdgeInfo> bVert, EdgeInfo info) : base(){
        this.aVert = aVert;
        this.bVert = bVert;
        //loose connection. If aVert or bVert changes its position this won't changes
        //that's why segmentGraphVertex has immutable position
        a = aVert.position;
        b = bVert.position;
        this.info = info;

        precalcBounds = ExpandedBounds(0.001f);
    }
}
