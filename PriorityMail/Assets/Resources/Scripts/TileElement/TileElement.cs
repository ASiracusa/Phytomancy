using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileElement
{
    public GameObject model;
    protected string tileName;
    public bool Checked { get; set; }
    public bool Moving { get; set; }

    // PHYSICS VARIABLES
    public bool Massless { get; protected set; }
    public bool Pushable { get; protected set; }
    public bool Weedblocked { get; protected set; }
    public bool Squishy { get; protected set; }

    public void RemoveModel()
    {
        GameObject.Destroy(model);
        model = null;
    }

    public void BindDataToModel()
    {
        model.GetComponent<ModelTileBridge>().Data = this;
    }

    protected void SetPhysics(bool massless, bool pushable, bool weedblocked, bool squishy)
    {
        Massless = massless;
        Pushable = pushable;
        Weedblocked = weedblocked;
        Squishy = squishy;
    }

    public void FaceTowards (Facet direction)
    {
        if (model != null)
        {
            model.transform.eulerAngles = new Vector3(0, Constants.DirectionToDegree(direction), 0);
        }
    }

    public abstract void WarpToPos();

    public abstract void MoveToPos(bool accelerate);

    public abstract void AdjustRender();

    public abstract bool InitiatePush(TileElement[,,] board, Facet direction, Monocoord newOccupant);

    public abstract bool Move(TileElement[,,] board, Facet direction);

    public abstract bool Push(TileElement[,,] board, Facet direction, LinkedList<TileElement> evaluatedTiles);

    public abstract bool TryPush(TileElement[,,] board, Facet direction, LinkedList<TileElement> evaluatedTiles);

    public abstract bool Fall(TileElement[,,] board);

    public abstract void EditorDeleteTileElement(TileElement[,,] board);

    public abstract void PlayerDeleteTileElement(TileElement[,,] board);

    public abstract TileElement GenerateTileElement(params object[] vars);

    public abstract TileElement LoadTileElement(params object[] vars);

    public abstract void CompileTileElement(ref LinkedList<int> dataInts, ref LinkedList<Shade> dataShades);

    public abstract TileElement DecompileTileElement(ref Queue<int> dataInts, ref Queue<Shade> dataShades);

    public abstract EditorTEIndices[] GetEditorTEIndices();

    public abstract string TileName();

    public abstract TileElementNames TileID();
    
}