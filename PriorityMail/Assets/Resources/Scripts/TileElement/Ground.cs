using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ground : Monocoord
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
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Ground(vars);
    }

    public void ColorFacet(Facet facet, Shade shade)
    {
        facets[(int)facet] = shade;
    }

    public void GenerateModel()
    {

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
}