using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Monocoord : TileElement
{
    private Vector3Int pos;

    public void SetCoords(int[] _pos)
    {
        pos = new Vector3Int(_pos[0], _pos[1], _pos[2]);
    }

    public Vector3Int GetPos ()
    {
        return pos;
    }
    
    public override void WarpToPos ()
    {
        if (model == null)
        {
            model = GameObject.Instantiate(Resources.Load("Models/" + TileName())) as GameObject;
            BindDataToModel();
            AdjustRender();
        }
        model.transform.position = pos;
    }

    public override void MoveToPos(bool accelerate)
    {
        //model.transform.position = pos;
        if (accelerate)
        {
            LevelManager.current.fallAnims.AddLast(new TileAnimationFall(this, new Vector3(pos.x, pos.y, pos.z)));
        }
        else
        {
            LevelManager.current.movementAnims.AddLast(new TileAnimationMovement(this, new Vector3(pos.x, pos.y, pos.z)));
        }
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
            temp?.Fall(board);
        }
        LevelManager.current.AddUndoData(new BoardDeletionState(this));
        RemoveModel();
    }

    public override bool Move(TileElement[,,] board, Facet direction)
    {
        Moving = true;
        LevelManager.current.AddUndoData(new BoardMovementState(this, pos));

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
        MoveToPos(false);
        return true;
    }

    public override bool Push(TileElement[,,] board, Facet direction, LinkedList<TileElement> evaluatedTiles)
    {
        Vector3Int pushCoords = new Vector3Int(pos.x, pos.y, pos.z);
        if (direction == Facet.North) { pushCoords.x += 1; }
        if (direction == Facet.South) { pushCoords.x -= 1; }
        if (direction == Facet.Up) { pushCoords.y += 1; }
        if (direction == Facet.Down) { pushCoords.y -= 1; }
        if (direction == Facet.West) { pushCoords.z += 1; }
        if (direction == Facet.East) { pushCoords.z -= 1; }

        TileElement temp = (pos.y == board.GetLength(1) - 1) ? null : board[pos.x, pos.y + 1, pos.z];
        
        if (pushCoords.x < 0 || pushCoords.x >= board.GetLength(0) ||
            pushCoords.y < 0 || pushCoords.y >= board.GetLength(1) ||
            pushCoords.z < 0 || pushCoords.z >= board.GetLength(2))
        {
            return false;
        }

        TileElement pushSubject = (board[pushCoords.x, pushCoords.y, pushCoords.z] == null) ?
            null :
            ((board[pushCoords.x, pushCoords.y, pushCoords.z] is IMonoSpacious) ?
            ((IMonoSpacious)board[pushCoords.x, pushCoords.y, pushCoords.z]).Helper.GetSolidOccupant() :
            board[pushCoords.x, pushCoords.y, pushCoords.z]);

        if (TryPush(board, direction, evaluatedTiles))
        {
            pushSubject?.Push(board, direction, evaluatedTiles);

            //temp?.Push(board, direction, evaluatedTiles);
            if (temp != null && temp.Pushable)
            {
                temp.Push(board, direction, evaluatedTiles);
            }

            if (!Moving)
            {
                Move(board, direction);
            }

            return true;
        }

        return false;
    }

    public override bool TryPush(TileElement[,,] board, Facet direction, LinkedList<TileElement> evaluatedTiles)
    {
        Vector3Int pushCoords = new Vector3Int(pos.x, pos.y, pos.z);
        if (direction == Facet.North) { pushCoords.x += 1; }
        if (direction == Facet.South) { pushCoords.x -= 1; }
        if (direction == Facet.Up) { pushCoords.y += 1; }
        if (direction == Facet.Down) { pushCoords.y -= 1; }
        if (direction == Facet.West) { pushCoords.z += 1; }
        if (direction == Facet.East) { pushCoords.z -= 1; }

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
            evaluatedTiles.AddFirst(this);
            Checked = true;
            return true;
        }

        if (!pushSubject.Pushable)
        {
            return false;
        }

        if (pushSubject.TryPush(board, direction, evaluatedTiles))
        {
            evaluatedTiles.AddFirst(this);
            Checked = true;
            return true;
        }

        if (pushSubject.Squishy)
        {
            evaluatedTiles.AddFirst(this);
            Checked = true;
            return true;
        }

        return false;
    }

    public override bool Fall (TileElement[,,] board)
    {
        // Massless objects can't fall
        if (Massless)
        { 
            return false;
        }

        TileElement fallSubject = (board[pos.x, pos.y, pos.z] is IMonoSpacious) ?
            ((IMonoSpacious)board[pos.x, pos.y, pos.z]).Helper.GetSolidOccupant() :
            board[pos.x, pos.y, pos.z];

        if (fallSubject == null || fallSubject.Massless)
        {
            return false;
        }

        LevelManager.current.AddUndoData(new BoardMovementState(this, pos));

        int y;
        for (y = pos.y - 1; y >= 0; y--)
        {
            if (!(board[pos.x, y, pos.z] == null || (board[pos.x, y, pos.z] is IMonoSpacious && ((IMonoSpacious)board[pos.x, y, pos.z]).Helper.GetSolidOccupant() != null)))
            {
                break;
            }
        }

        if (y == pos.y - 1)
        {
            Debug.Log("it doesnt need to fall");
            return false;
        }

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
            if (pos.y != y + 1)
            {
                pos.y = y + 1;
                MoveToPos(true);

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
        }

        temp?.Fall(board);
        return true;
    }

    // Starts a push chain
    public override bool InitiatePush(TileElement[,,] board, Facet direction, Monocoord newOccupant)
    {
        LinkedList<TileElement> evaluatedTiles = new LinkedList<TileElement>();
        if (Push(board, direction, evaluatedTiles))
        {
            // Sets the newOccupant to its position if push is successful
            if (newOccupant != null)
            {
                board[newOccupant.pos.x, newOccupant.pos.y, newOccupant.pos.z] = newOccupant;
            }

            // All pushed elements are dropped and their physics helper properties are reset
            foreach (TileElement te in evaluatedTiles)
            {
                te.Fall(board);
                te.Checked = false;
                te.Moving = false;
            }
            evaluatedTiles.Clear();

            // Executes movement animation
            LevelManager.current.StartCoroutine(LevelManager.current.AnimateTileStateChange());

            return true;
        }
        return false;
    }
}