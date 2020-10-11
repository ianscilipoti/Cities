using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : TextureSampler
{
    private float scale;
    private float maxElevation;
    public TerrainGenerator (float scale, float maxElevation)
    {
        this.scale = scale;
        this.maxElevation = maxElevation;
    }
    public float get (float x, float y)
    {
        return Mathf.PerlinNoise(x / scale, y / scale) * maxElevation; 
    }
}
