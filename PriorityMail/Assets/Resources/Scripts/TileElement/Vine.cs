using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Vine : Monocoord
{
    private Vine next;
    private Shade color;
    private Facet origin;

    public Vine (params object[] vars)
    {
        next = vars[0] as Vine;
        color = (Shade)(vars[1]);
        origin = (Facet)(vars[2]);
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Vine(vars);
    }

    public override TileElement LoadTileElement(params object[] vars)
    {
        return new Vine(vars);
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
        return "Vine";
    }
}
