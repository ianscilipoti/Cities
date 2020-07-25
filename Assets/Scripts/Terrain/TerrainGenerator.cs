using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : TextureSampler
{
    private float scale;
    public TerrainGenerator (float scale)
    {
        this.scale = scale; 
    }
    public float get (float x, float y)
    {
        return Mathf.PerlinNoise(x / scale, y / scale); 
    }
}
