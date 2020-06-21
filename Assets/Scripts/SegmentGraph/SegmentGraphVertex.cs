using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentGraphVertex <EdgeInfo>
{
    public Vector2 position { get; }
    public List<SegmentGraphSegment<EdgeInfo>> connections;

    public SegmentGraphVertex (Vector2 position) 
    {
        this.position = position;
        this.connections = new List<SegmentGraphSegment<EdgeInfo>>();
    }


    public void RemoveConnection (SegmentGraphSegment<EdgeInfo> connection)
    {
        connections.Remove(connection);
    }

    public void AddConnection(SegmentGraphSegment<EdgeInfo> connection)
    {
        connections.Add(connection);
    }

    public int NumConnections () 
    {
        return connections.Count;
    }
}
