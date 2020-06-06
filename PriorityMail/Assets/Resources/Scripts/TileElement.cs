using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileElement
{
    protected string tileName;
    protected bool weighted;
    protected bool full;
    protected Dictionary<string, MeshRenderer> models;

    public abstract TileElement GenerateTileElement(params object[] vars);
}
