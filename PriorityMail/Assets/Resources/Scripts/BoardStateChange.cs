using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoardStateChange
{
    protected TileElement tileElement;

    public abstract void Revert(TileElement[,,] board);
}

public class BoardMovementState : BoardStateChange
{
    private Vector3Int oldPos;

    public BoardMovementState (TileElement _tileElement, Vector3Int _oldPos)
    {
        tileElement = _tileElement;
        oldPos = _oldPos;
    }

    public override void Revert (TileElement[,,] board)
    {
        if (tileElement is Monocoord)
        {
            Monocoord monocoord = (Monocoord)tileElement;

            if (board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z] is IMonoSpacious)
            {
                ((IMonoSpacious)board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z]).Expecting = true;
            }
            else
            {
                board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z] = null;
            }

            monocoord.SetCoords(new int[]
            {
                oldPos.x,
                oldPos.y,
                oldPos.z
            });

            if (board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z] != null && board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z] is IMonoSpacious)
            {
                ((IMonoSpacious)board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z]).Helper.Inhabitant = monocoord;
                ((IMonoSpacious)board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z]).TileEnters(monocoord);
            }
            else
            {
                board[monocoord.GetPos().x, monocoord.GetPos().y, monocoord.GetPos().z] = monocoord;
            }
        }
        else
        {
            Dicoord dicoord = ((Dicoord)tileElement);

            for (int x = dicoord.GetPos1().x; x <= dicoord.GetPos2().x; x++)
            {
                for (int y = dicoord.GetPos1().y; y <= dicoord.GetPos2().y; y++)
                {
                    for (int z = dicoord.GetPos1().z; z <= dicoord.GetPos2().z; z++)
                    {
                        if (board[x, y, z] is IMonoSpacious)
                        {
                            ((IMonoSpacious)board[x, y, z]).Expecting = true;
                        }
                        else
                        {
                            board[x, y, z] = null;
                        }
                    }
                }
            }

            dicoord.SetCoords(new int[]
            {
                oldPos.x,
                oldPos.y,
                oldPos.z
            },
            new int[] {
                oldPos.x + (dicoord.GetPos2().x - dicoord.GetPos1().x),
                oldPos.y + (dicoord.GetPos2().y - dicoord.GetPos1().y),
                oldPos.z + (dicoord.GetPos2().z - dicoord.GetPos1().z)
            });

            for (int x = dicoord.GetPos1().x; x <= dicoord.GetPos2().x; x++)
            {
                for (int y = dicoord.GetPos1().y; y <= dicoord.GetPos2().y; y++)
                {
                    for (int z = dicoord.GetPos1().z; z <= dicoord.GetPos2().z; z++)
                    {
                        if (board[x, y, z] != null && board[x, y, z] is IMonoSpacious)
                        {
                            ((IMonoSpacious)board[x, y, z]).Helper.Inhabitant = dicoord;
                            ((IMonoSpacious)board[x, y, z]).TileEnters(dicoord);
                        } 
                        else
                        {
                            board[x, y, z] = dicoord;
                        }
                    }
                }
            }
        }
        tileElement.WarpToPos();
    }
}

public class BoardCreationState : BoardStateChange
{
    public BoardCreationState (TileElement _tileElement)
    {
        tileElement = _tileElement;
    }

    public override void Revert(TileElement[,,] board)
    {
        tileElement.EditorDeleteTileElement(board);
        if (tileElement is Vine)
        {
            LevelManager.current.AdjustAvailableVinesUI(((Vine)tileElement).GetColor(), 1);
        }
    }
}

public class BoardDeletionState : BoardStateChange
{
    public BoardDeletionState (TileElement _tileElement)
    {
        tileElement = _tileElement;
    }

    public override void Revert(TileElement[,,] board)
    {
        tileElement.model = GameObject.Instantiate(Resources.Load("Models/" + tileElement.TileName())) as GameObject;
        tileElement.BindDataToModel();
        tileElement.WarpToPos();
        tileElement.AdjustRender();
        if (tileElement is Monocoord)
        {
            Monocoord monoTile = (Monocoord)tileElement;
            board[monoTile.GetPos().x, monoTile.GetPos().y, monoTile.GetPos().z] = tileElement;
            if (tileElement is Vine)
            {
                LevelManager.current.AdjustAvailableVinesUI(((Vine)tileElement).GetColor(), -1);
            }
        }
        else
        {
            Dicoord diTile = (Dicoord)tileElement;
            for (int x = diTile.GetPos1().x; x <= diTile.GetPos2().x; x++)
            {
                for (int y = diTile.GetPos1().y; y <= diTile.GetPos2().y; y++)
                {
                    for (int z = diTile.GetPos1().z; z <= diTile.GetPos2().z; z++)
                    {
                        board[x, y, z] = tileElement;
                    }
                }
            }
        }
    }
}