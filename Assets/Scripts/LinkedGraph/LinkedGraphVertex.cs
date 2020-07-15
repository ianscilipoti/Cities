using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedGraphVertex 
{
    public Vector2 pt 
    {
        get;
    }

    public List<LinkedGraphEdge> connections;

    public LinkedGraphVertex(Vector2 position)
    {
        this.pt = position;
        this.connections = new List<LinkedGraphEdge>();
    }

    public void RemoveConnection(LinkedGraphEdge connection)
    {
        connections.Remove(connection);
    }

    public void AddConnection(LinkedGraphEdge connection)
    {
        connections.Add(connection);
    }

    public IEnumerable<LinkedGraphEdge> GetConnections ()
    {
        return (IEnumerable<LinkedGraphEdge>)connections;
    }

    public int NumConnections()
    {
        return connections.Count;
    }
}
