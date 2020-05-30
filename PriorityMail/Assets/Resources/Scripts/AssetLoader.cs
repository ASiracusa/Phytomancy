using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AssetLoader
{
    public delegate TileElement TileElementConstructor();

    public abstract Dictionary<string, TileElementConstructor> LoadAssets();
}
