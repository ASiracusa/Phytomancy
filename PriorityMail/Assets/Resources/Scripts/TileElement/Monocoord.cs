using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public override void EditorDeleteTileElement(TileElement[,,] board)
    {
        if (board.GetLength(0) > pos.x && board.GetLength(1) > pos.y && board.GetLength(2) > pos.z) {
            board[pos.x, pos.y, pos.z] = null;
        }
        RemoveModel();
    }

    public override void PlayerDeleteTileElement(TileElement[,,] board)
    {
        if (board.GetLength(0) > pos.x && board.GetLength(1) > pos.y && board.GetLength(2) > pos.z)
        {
            TileElement temp = (pos.y == board.GetLength(1) - 1) ? null : board[pos.x, pos.y + 1, pos.z];
            board[pos.x, pos.y, pos.z] = null;
            temp?.Fall(ref board);
        }
        RemoveModel();
    }

    public override void Move(ref TileElement[,,] board, Facet direction)
    {
        Vector3Int newPos = new Vector3Int(pos.x, pos.y, pos.z);
        if (direction == Facet.North) { newPos.x += 1; }
        if (direction == Facet.South) { newPos.x -= 1; }
        if (direction == Facet.Up) { newPos.y += 1; }
        if (direction == Facet.Down) { newPos.y -= 1; }
        if (direction == Facet.West) { newPos.z += 1; }
        if (direction == Facet.East) { newPos.z -= 1; }

        TileElement moveSubject = (board[pos.x, pos.y, pos.z] is IMonoSpacious) ?
            ((IMonoSpacious)board[pos.x, pos.y, pos.z]).Helper.GetSolidOccupant() :
            board[pos.x, pos.y, pos.z];

        if (board[pos.x, pos.y, pos.z] is IMonoSpacious)
        {
            IMonoSpacious carrier = (IMonoSpacious)board[pos.x, pos.y, pos.z];
            carrier.Expecting = true;
        }
        else
        {
            board[pos.x, pos.y, pos.z] = null;
        }

        if (board[newPos.x, newPos.y, newPos.z] is IMonoSpacious)
        {
            IMonoSpacious carrier = (IMonoSpacious)board[newPos.x, newPos.y, newPos.z];
            carrier.Expecting = false;
            carrier.Helper.Inhabitant = this;
            carrier.TileEnters(this);
        }
        else
        {
            board[newPos.x, newPos.y, newPos.z] = this;
        }
        pos = newPos;
        MoveToPos();
    }

    public override bool Push(ref TileElement[,,] board, Facet direction, Monocoord newOccupant)
    {
        Vector3Int pushCoords = new Vector3Int(pos.x, pos.y, pos.z);
        if (direction == Facet.North) { pushCoords.x += 1; }
        if (direction == Facet.South) { pushCoords.x -= 1; }
        if (direction == Facet.Up) { pushCoords.y += 1; }
        if (direction == Facet.Down) { pushCoords.y -= 1; }
        if (direction == Facet.West) { pushCoords.z += 1; }
        if (direction == Facet.East) { pushCoords.z -= 1; }

        TileElement temp = (pos.y == board.GetLength(1) - 1) ? null : board[pos.x, pos.y + 1, pos.z];
        
        if (pushCoords.x < 0 || pushCoords.x > board.GetLength(0) ||
            pushCoords.y < 0 || pushCoords.y > board.GetLength(1) ||
            pushCoords.z < 0 || pushCoords.z > board.GetLength(2))
        {
            return false;
        }

        TileElement pushSubject = (board[pushCoords.x, pushCoords.y, pushCoords.z] == null) ?
            null :
            ((board[pushCoords.x, pushCoords.y, pushCoords.z] is IMonoSpacious) ?
            ((IMonoSpacious)board[pushCoords.x, pushCoords.y, pushCoords.z]).Helper.GetSolidOccupant() :
            board[pushCoords.x, pushCoords.y, pushCoords.z]);

        if (pushSubject == null)
        {
            if (newOccupant != null)
            {
                newOccupant.pos = pos;
            }
            Move(ref board, direction);
            if (newOccupant != null)
            {
                board[newOccupant.pos.x, newOccupant.pos.y, newOccupant.pos.z] = newOccupant;
            }
            Fall(ref board);
            temp?.Push(ref board, direction, null);
            return true;
        }

        if (!pushSubject.Pushable)
        {
            return false;
        }

        if (pushSubject.Push(ref board, direction, null))
        {
            if (newOccupant != null)
            {
                newOccupant.pos = pos;
            }
            Move(ref board, direction);
            if (newOccupant != null)
            {
                board[newOccupant.pos.x, newOccupant.pos.y, newOccupant.pos.z] = newOccupant;
            }
            Fall(ref board);
            temp?.Push(ref board, direction, null);
            return true;
        }

        if (pushSubject.Squishy)
        {
            if (newOccupant != null)
            {
                newOccupant.MoveToPos();
            }
            board[pushCoords.x, pushCoords.y, pushCoords.z].EditorDeleteTileElement(board);
            Move(ref board, direction);
            if (newOccupant != null)
            {
                board[newOccupant.pos.x, newOccupant.pos.y, newOccupant.pos.z] = newOccupant;
            }
            Fall(ref board);
            temp?.Push(ref board, direction, null);
            return true;
        }

        return false;
    }

    public override void Fall (ref TileElement[,,] board)
    {
        TileElement fallSubject = (board[pos.x, pos.y, pos.z] is IMonoSpacious) ?
            ((IMonoSpacious)board[pos.x, pos.y, pos.z]).Helper.GetSolidOccupant() :
            board[pos.x, pos.y, pos.z];

        if (fallSubject.Massless)
        {
            return;
        }

        int y;
        for (y = pos.y - 1; y >= 0; y--)
        {
            if (!(board[pos.x, y, pos.z] == null || (board[pos.x, y, pos.z] is IMonoSpacious && ((IMonoSpacious)board[pos.x, y, pos.z]).Helper.GetSolidOccupant() != null)))
            {
                break;
            }
        }
        Debug.Log("y = " + y);

        TileElement temp = (pos.y == board.GetLength(1) - 1) ? null : board[pos.x, pos.y + 1, pos.z];



        if (board[pos.x, pos.y, pos.z] is IMonoSpacious)
        {
            IMonoSpacious carrier = (IMonoSpacious)board[pos.x, pos.y, pos.z];
            carrier.Expecting = true;
        }
        else
        {
            board[pos.x, pos.y, pos.z] = null;
        }

        if (y == -1)
        {
            if (board[pos.x, pos.y, pos.z] is IMonoSpacious)
            {
                RemoveModel();
                IMonoSpacious carrier = (IMonoSpacious)board[pos.x, pos.y, pos.z];
                carrier.Helper.RemoveInhabitant();
                carrier.TileLeaves();
            }
            else
            {
                EditorDeleteTileElement(board);
            }
        }
        else
        {
            pos.y = y + 1;
            MoveToPos();

            if (board[pos.x, pos.y, pos.z] == null)
            {
                board[pos.x, pos.y, pos.z] = this;
            }
            else if (board[pos.x, y, pos.z] is IMonoSpacious)
            {
                IMonoSpacious carrier = (IMonoSpacious)board[pos.x, pos.y, pos.z];
                carrier.Helper.Inhabitant = this;
                carrier.TileEnters(this);
            }
        }
        temp?.Fall(ref board);
    }
}
