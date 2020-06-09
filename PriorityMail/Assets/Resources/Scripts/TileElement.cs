using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileElement
{
    public GameObject model;
    protected string tileName;

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

    public abstract void MoveToPos();

    public abstract TileElement GenerateTileElement(params object[] vars);

    public abstract TileElement LoadTileElement(params object[] vars);

    public abstract void DeleteTileElement(TileElement[,,] board);

    public abstract EditorTEIndices[] GetEditorTEIndices();

    public abstract string TileName();

    public abstract void Move(ref TileElement[,,] board, Facet direction);

    public abstract bool Push(ref TileElement[,,] board, Facet direction);

    public abstract void Fall(ref TileElement[,,] board);
}