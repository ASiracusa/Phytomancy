﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sigil : Monocoord, IMonoSpacious
{
    private MonoSpaciousHelper _msh;
    private bool _expecting;

    public MonoSpaciousHelper Helper { get => _msh; set => _msh = value; }
    public bool Expecting { get => _expecting; set => _expecting = value; }

    public Sigil() { }

    public Sigil (params object[] vars)
    {
        SetCoords(new int[] {
            ((Vector3Int)vars[0]).x,
            ((Vector3Int)vars[0]).y,
            ((Vector3Int)vars[0]).z
        });
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Sigil(vars);
    }

    public override TileElement LoadTileElement(params object[] vars)
    {
        Sigil s = new Sigil(vars);
        s.Helper = new MonoSpaciousHelper();
        return s;
    }

    public override EditorTEIndices[] GetEditorTEIndices()
    {
        return new EditorTEIndices[]
        {
            EditorTEIndices.Pos1
        };
    }

    public override string TileName()
    {
        return "Sigil";
    }

    public void TileEnters (TileElement enterer) { }

    public void TileLeaves () { }
}
