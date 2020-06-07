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

    public Vector3Int GetPos ()
    {
        return pos;
    }
    
    public override void MoveToPos ()
    {
        model.transform.position = pos;
    }

    public override void DeleteTileElement(TileElement[,,] board)
    {
        if (board.GetLength(0) > pos.x && board.GetLength(1) > pos.y && board.GetLength(2) > pos.z) {
            board[pos.x, pos.y, pos.z] = null;
        }
        RemoveModel();
    }
}
