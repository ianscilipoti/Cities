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

    public bool RemoveConnection(LinkedGraphEdge connection)
    {
        return connections.Remove(connection);
    }


    public LinkedGraphEdge GetRightConnection (LinkedGraphEdge edge)
    {
        int index = connections.IndexOf(edge);
        if (index != -1)
        {
            return connections[(index + 1) % connections.Count];
        }
        return null;
    }

    public LinkedGraphEdge GetLeftConnection(LinkedGraphEdge edge)
    {
        int index = connections.IndexOf(edge);
        if (index != -1)
        {
            return connections[(index - 1 + connections.Count) % connections.Count];
        }
        return null;
    }

    public void AddConnection(LinkedGraphEdge connection)
    {
        int insertIndex = 0;
        LinkedGraphVertex otherVert = connection.GetOppositeVertex(this);
        float newAngle = HelperFunctions.AngleBetween(otherVert.pt - pt, Vector2.right) % (Mathf.PI*2);
        for (int i = 0; i < connections.Count; i ++)
        {
            LinkedGraphVertex thisOppositeVert = connections[i].GetOppositeVertex(this);
            float existingAngle = HelperFunctions.AngleBetween(thisOppositeVert.pt - pt, Vector2.right) % (Mathf.PI * 2);

            if (newAngle < existingAngle)
            {
                insertIndex++;
            }
            else
            {
                break;
            }
        }

        connections.Insert(insertIndex, connection);
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
