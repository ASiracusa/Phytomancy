using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Dicoord : TileElement
{
    private Vector3Int pos1;
    private Vector3Int pos2;

    public void SetCoords(int[] _pos1, int[] _pos2)
    {
        pos1 = new Vector3Int(_pos1[0], _pos1[1], _pos1[2]);
        pos2 = new Vector3Int(_pos2[0], _pos2[1], _pos2[2]);
    }

    public Vector3Int GetPos1 ()
    {
        return pos1;
    }

    public Vector3Int GetPos2()
    {
        return pos2;
    }

    public override void WarpToPos()
    {
        model.transform.position = new Vector3((pos1.x + pos2.x) / 2.0f, (pos1.y + pos2.y) / 2.0f, (pos1.z + pos2.z) / 2.0f);
    }

    public override void MoveToPos(bool accelerate)
    {
        //model.transform.position = new Vector3((pos1.x + pos2.x) / 2.0f, (pos1.y + pos2.y) / 2.0f, (pos1.z + pos2.z) / 2.0f);
        if (accelerate)
        {
            LevelManager.current.fallAnims.AddLast(new TileAnimationFall(this, new Vector3((pos1.x + pos2.x) / 2.0f, (pos1.y + pos2.y) / 2.0f, (pos1.z + pos2.z) / 2.0f)));
        }
        else
        {
            LevelManager.current.movementAnims.AddLast(new TileAnimationMovement(this, new Vector3((pos1.x + pos2.x) / 2.0f, (pos1.y + pos2.y) / 2.0f, (pos1.z + pos2.z) / 2.0f)));
        }
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

    public override void PlayerDeleteTileElement(TileElement[,,] board)
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
        LevelManager.current.AddUndoData(new BoardDeletionState(this));
        RemoveModel();

        if (GetPos2().y != board.GetLength(1))
        {
            PerformOnFacet(ref board, Facet.Up, true, (int x, int y, int z) => board[x, y, z].Fall(board));
        }
    }

    public override bool Move(TileElement[,,] board, Facet direction)
    {
        Moving = true;
        LevelManager.current.AddUndoData(new BoardMovementState(this, pos1));

        PerformOnFacet(ref board, Constants.FlipDirection(direction), false, (int x, int y, int z) =>
        {
            if (board[x, y, z] is IMonoSpacious) {
                IMonoSpacious carrier = (IMonoSpacious)board[x, y, z];
                carrier.Expecting = true;
            }
            else
            {
                board[x, y, z] = null;
            }
            return true;
        });

        PerformOnFacet(ref board, direction, true, (int x, int y, int z) =>
        {
            if (board[x, y, z] is IMonoSpacious)
            {
                IMonoSpacious carrier = (IMonoSpacious)board[x, y, z];
                carrier.Expecting = false;
                carrier.Helper.Inhabitant = this;
                carrier.TileEnters(this);
            }
            else
            {
                board[x, y, z] = this;
            }
            return true;
        });

        Vector3Int newPos1 = GetPos1() + Constants.FacetToVector(direction);
        Vector3Int newPos2 = GetPos2() + Constants.FacetToVector(direction);
        SetCoords(new int[] {
            newPos1.x, newPos1.y, newPos1.z
        }, new int[] {
            newPos2.x, newPos2.y, newPos2.z
        });
        MoveToPos(false);
        return true;
    }

    public override bool Push(TileElement[,,] board, Facet direction, LinkedList<TileElement> evaluatedTiles)
    {
        if (TryPush(board, direction, evaluatedTiles))
        {
            PerformOnFacet(ref board, direction, true, (int x, int y, int z) =>
            {
                if (board[x, y, z] == null)
                {
                    return true;
                }
                else if (board[x, y, z].Checked)
                {
                    return board[x, y, z].Push(board, direction, evaluatedTiles);
                }
                return true;
            });

            if (pos2.y != board.GetLength(1) - 1)
            {
                for (int x = pos1.x; x <= pos2.x; x++)
                {
                    for (int z = pos1.z; z <= pos2.z; z++)
                    {
                        if (board[x, pos2.y + 1, z] != null && !board[x, pos2.y + 1, z].Checked)
                        {
                            board[x, pos2.y + 1, z].Push(board, direction, evaluatedTiles);
                        }
                    }
                }
            }

            if (!Moving)
            {
                Move(board, direction);
            }

            return true;
        }

        evaluatedTiles.AddFirst(this);
        //Fall(board);
        return false;
    }

    public override bool TryPush(TileElement[,,] board, Facet direction, LinkedList<TileElement> evaluatedTiles)
    {
        // Test if a push is possible
        LinkedList<Boolean> tryPushAttempt = PerformOnFacet(ref board, direction, true, (int x, int y, int z) =>
        {
            if (x < 0 || y < 0 || z < 0 || x >= board.GetLength(0) || y >= board.GetLength(1) || z >= board.GetLength(2))
            {
                return false;
            }
            else if (board[x, y, z] == null)
            {
                return true;
            }
            else if (!board[x, y, z].Pushable)
            {
                return false;
            }
            else if (!board[x, y, z].Checked)
            {
                return board[x, y, z].TryPush(board, direction, evaluatedTiles);
            }
            return true;
        });
        
        if (tryPushAttempt != null && !tryPushAttempt.Contains(false))
        {
            evaluatedTiles.AddFirst(this);
            Checked = true;
        }
        return (tryPushAttempt != null && !tryPushAttempt.Contains(false));
    }

    public override bool Fall(TileElement[,,] board)
    {
        if (Massless)
        {
            return false;
        }

        LevelManager.current.AddUndoData(new BoardMovementState(this, pos1));

        int yBelow;
        for (yBelow = pos1.y - 1; yBelow >= 0; yBelow--)
        {
            bool canLand = PerformOnFacet(ref board, Facet.Down, true, (int x, int y, int z) =>
            {
                return !(board[x, yBelow, z] == null || (board[x, yBelow, z] is IMonoSpacious && ((IMonoSpacious)board[x, yBelow, z]).Helper.GetSolidOccupant() != null));
            }).Contains(true);

            if (canLand)
            {
                break;
            }
        }

        if (yBelow == pos1.y - 1)
        {
            return false;
        }

        int yAbove = GetPos2().y + 1;
        
        for (int x = pos1.x; x <= pos2.x; x++)
        {
            for (int y = pos1.y; y <= pos2.y; y++)
            {
                for (int z = pos1.z; z <= pos2.z; z++)
                {
                    if (board[x, y, z] is IMonoSpacious)
                    {
                        IMonoSpacious carrier = (IMonoSpacious)board[x, y, z];
                        carrier.Expecting = true;
                    }
                    else
                    {
                        board[x, y, z] = null;
                    }
                }
            }
        }

        if (yBelow == -1)
        {
            for (int x = pos1.x; x <= pos2.x; x++)
            {
                for (int y = pos1.y; y <= pos2.y; y++)
                {
                    for (int z = pos1.z; z <= pos2.z; z++)
                    {
                        if (board[x, y, z] is IMonoSpacious)
                        {
                            IMonoSpacious carrier = (IMonoSpacious)board[x, y, z];
                            carrier.Helper.RemoveInhabitant();
                            carrier.TileLeaves();
                        }
                        else
                        {
                            board[x, y, z] = null;
                        }
                    }
                }
            }

            RemoveModel();
        }
        else
        {
            pos2.y = yBelow + 1 + (pos2.y - pos1.y);
            pos1.y = yBelow + 1;
            MoveToPos(true);

            for (int x = pos1.x; x <= pos2.x; x++)
            {
                for (int y = pos1.y; y <= pos2.y; y++)
                {
                    for (int z = pos1.z; z <= pos2.z; z++)
                    {
                        if (board[x, y, z] == null)
                        {
                            board[x, y, z] = this;
                        }
                        else if (board[x, y, z] is IMonoSpacious)
                        {
                            IMonoSpacious carrier = (IMonoSpacious)board[x, y, z];
                            carrier.Helper.Inhabitant = this;
                            carrier.TileEnters(this);
                        }
                    }
                }
            }
        }

        LinkedList<TileElement> evaluatedTileElements = new LinkedList<TileElement>();
        for (int x = pos1.x; x <= pos2.x; x++)
        {
            for (int z = pos1.z; z <= pos2.z; z++)
            {
                if (board[x, yAbove, z] != null && !board[x, yAbove, z].Checked)
                {
                    //board[x, yAbove, z].Checked = true;
                    board[x, yAbove, z].Fall(board);
                }
            }
        }
        // Uncheck all of the TileElements before returning
        foreach (TileElement te in evaluatedTileElements)
        {
            //te.Checked = false;
        }

        return true;
    }

    public LinkedList<OutType> PerformOnFacet<OutType> (ref TileElement[,,] board, Facet facet, bool adjacent, Func<int, int, int, OutType> func)
    {
        // Get the mins and maxes of the area of affected tiles on a given facet
        // One side will always be one tile long and the other two will be the size of the Dicoord's lengths
        int adjMod = adjacent ? 1 : 0;
        int xMin =  (facet == Facet.North) ?
                        GetPos2().x + adjMod :
                    (facet == Facet.South) ?
                        GetPos1().x - adjMod :
                        GetPos1().x;
        int xMax = (facet == Facet.North) ?
                        GetPos2().x + adjMod :
                    (facet == Facet.South) ?
                        GetPos1().x - adjMod :
                        GetPos2().x;
        int yMin = (facet == Facet.Up) ?
                        GetPos2().y + adjMod :
                    (facet == Facet.Down) ?
                        GetPos1().y - adjMod :
                        GetPos1().y;
        int yMax = (facet == Facet.Up) ?
                        GetPos2().y + adjMod :
                    (facet == Facet.Down) ?
                        GetPos1().y - adjMod :
                        GetPos2().y;
        int zMin = (facet == Facet.West) ?
                        GetPos2().z + adjMod :
                    (facet == Facet.East) ?
                        GetPos1().z - adjMod :
                        GetPos1().z;
        int zMax = (facet == Facet.West) ?
                        GetPos2().z + adjMod :
                    (facet == Facet.East) ?
                        GetPos1().z - adjMod :
                        GetPos2().z;

        // Create array of results for each tile space
        LinkedList<OutType> outs = new LinkedList<OutType>();

        // Stop execution if the area is out of bounds
        if (xMin < 0 || xMax >= board.GetLength(0) || yMin < 0 || yMax >= board.GetLength(1) || zMin < 0 || zMax >= board.GetLength(2))
        {
            return null;
        }

        // Perform function on each tile in the array and save their outputs
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    outs.AddLast(func.Invoke(x, y, z));
                }
            }
        }

        // Return the array of outputs
        return outs;
    }

    public override bool InitiatePush(TileElement[,,] board, Facet direction, Monocoord newOccupant)
    {
        LinkedList<TileElement> evaluatedTiles = new LinkedList<TileElement>();
        if (Push(board, direction, evaluatedTiles))
        {
            if (newOccupant != null)
            {
                board[newOccupant.GetPos().x, newOccupant.GetPos().y, newOccupant.GetPos().z] = newOccupant;
            }

            foreach (TileElement te in evaluatedTiles)
            {
                te.Fall(board);
                te.Checked = false;
                te.Moving = false;
            }
            evaluatedTiles.Clear();

            LevelManager.current.StartCoroutine(LevelManager.current.AnimateTileStateChange());

            return true;
        }
        return false;
    }
}
