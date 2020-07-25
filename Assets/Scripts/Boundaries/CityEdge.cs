using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CityEdge : EdgeLoopEdge
{
    private CityEdgeType type;
    private string id;

    public CityEdge(LinkedGraphVertex a, LinkedGraphVertex b, CityEdgeType type) : base(a, b)
    {
        this.type = type;
        this.id = getID();
    }

    //hacky way of getting unique IDs
    private string getID ()
    {
        return "Edge" + a.pt.x + a.pt.y + b.pt.x + b.pt.y;
    }
    //add functionality to ensure that children edges inheret our id and type
    public override void OnEdgeSplitCustom (LinkedGraphEdge edge1, LinkedGraphEdge edge2)
    {
        if (edge1 is CityEdge)
        {
            CityEdge ce1 = (CityEdge)edge1;
            ce1.id = id;
            ce1.type = type;
        }
        if (edge2 is CityEdge)
        {
            CityEdge ce2 = (CityEdge)edge1;
            ce2.id = id;
            ce2.type = type;

        }
    }
}

public enum CityEdgeType 
{
    Unspecified = 0,
    LandPath = 1,
    WaterPath = 2,
    Wall = 3
}