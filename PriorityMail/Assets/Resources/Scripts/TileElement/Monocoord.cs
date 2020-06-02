using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monocoord : TileElement
{
    private Vector3Int pos;

    protected void SetCoords(int[] _pos)
    {
        pos = new Vector3Int(_pos[0], _pos[1], _pos[2]);
    }
}
