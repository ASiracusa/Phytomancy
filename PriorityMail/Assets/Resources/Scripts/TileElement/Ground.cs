using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ground : Monocoord, IColorable
{
    private Shade[] facets;

    public Ground() { }

    public Ground(params object[] vars)
    {
        SetCoords(new int[] {
            ((Vector3Int)vars[0]).x,
            ((Vector3Int)vars[0]).y,
            ((Vector3Int)vars[0]).z
        });
        facets = new Shade[]
        {
            Shade.Color1,
            Shade.Color1,
            Shade.Color1,
            Shade.Color1,
            Shade.Color1,
            Shade.Color1
        };
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Ground(vars);
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
        return "Ground";
    }

    public void SetShade (Shade shade, int index)
    {
        facets[index] = shade;
    }

    public Shade[] GetShades ()
    {
        return facets;
    }
}