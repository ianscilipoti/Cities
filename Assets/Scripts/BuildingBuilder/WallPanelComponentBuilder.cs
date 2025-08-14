using System;
using UnityEngine;

public class WallPanelComponentBuilder
{
    private DecorationType decType;
    private Vector2 leftPoint;//facing out
    private Vector2 rightPoint;
    private float height;

    //static BuildingComponent GetBuildingComponent (Vector2 LeftPoint, Vector2 rightPoint, float height, DecorationType decType)
    //{
    //    BuildingComponent component = new BuildingComponent();


    //    return component;
    //}
}

public enum DecorationType
{
    Window,
    Door
}