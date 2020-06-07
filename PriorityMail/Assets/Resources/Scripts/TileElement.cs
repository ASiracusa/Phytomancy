using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileElement
{
    public GameObject model;
    protected string tileName;
    protected bool weighted;
    protected bool full;
    protected Dictionary<string, MeshRenderer> models;

    public void RemoveModel()
    {
        GameObject.Destroy(model);
        model = null;
    }

    public void BindDataToModel()
    {
        model.GetComponent<ModelTileBridge>().Data = this;
    }

    public abstract void MoveToPos();

    public abstract TileElement GenerateTileElement(params object[] vars);

    public abstract void DeleteTileElement(TileElement[,,] board);

    public abstract EditorTEIndices[] GetEditorTEIndices();

    public abstract string TileName();
}
