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

    public override void MoveToPos()
    {
        model.transform.position = pos1;
    }

    public override void EditorDeleteTileElement(TileElement[,,] board)
    {
        for (int x = pos1.x; x <= pos2.x; x++)
        {
            for (int y = pos1.y; y <= pos2.y; y++)
            {
                for (int z = pos1.z; z <= pos2.z; z++)
                {
                    if (board.GetLength(0) > x && board.GetLength(1) > y && board.GetLength(2) > z)
                    {
                        board[x, y, z] = null;
                    }
                }
            }
        }
        RemoveModel();
    }
}
