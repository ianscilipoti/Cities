using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CityEdge : EdgeLoopEdge
{
    private CityEdgeType type;
    private string id;
    protected float width;

    public CityEdge(LinkedGraphVertex a, LinkedGraphVertex b, CityEdgeType type, float width) : base(a, b)
    {
        this.type = type;
        this.id = getID();
        this.width = width;
    }

    public static System.Object[] GetRoadFactoryParams (int depth)
    {
        if (depth == 1)
        {
            return new System.Object[] { CityEdgeType.LandPath, 12f };
        }
        if (depth == 2)
        {
            return new System.Object[] { CityEdgeType.LandPath, 6f };
        }
        return new System.Object[] { CityEdgeType.LandPath, 4f };
    }

    public CityEdgeType GetRoadType ()
    {
        return type;
    }

    public float GetWidth ()
    {
        return width;
    }

    //hacky way of getting unique IDs
    private string getID()
    {
        return "Edge" + a.pt.x + a.pt.y + b.pt.x + b.pt.y;
    }

    //hacky way of getting unique IDs
    public string GetID()
    {
        return id;
    }

    //add functionality to ensure that children edges inheret our id and type
    public override void OnEdgeSplitCustom(LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        if (edge1 is CityEdge)
        {
            CityEdge ce1 = (CityEdge)edge1;
            ce1.id = id;
            ce1.type = type;
            ce1.width = width;
        }
        else
        {
            Debug.Log("couldn't assign data to sub edge on split.");
        }
        if (edge2 is CityEdge)
        {
            CityEdge ce2 = (CityEdge)edge2;
            ce2.id = id;
            ce2.type = type;
            ce2.width = width;

        }
        else
        {
            Debug.Log("couldn't assign data to sub edge on split.");
        }
    }
}

public enum CityEdgeType
{
    Unspecified = 0,
    LandPath = 1,
    WaterPath = 2,
    Wall = 3,
    PlotBoundary = 4,
    EdgeCap = 5
}

