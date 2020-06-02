using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Dicoord : TileElement
{
    private Vector3Int pos1;
    private Vector3Int pos2;

    protected void SetCoords(int[] _pos1, int[] _pos2)
    {
        pos1 = new Vector3Int(_pos1[0], _pos1[1], _pos1[2]);
        pos2 = new Vector3Int(_pos2[0], _pos2[1], _pos2[2]);
    }
}
