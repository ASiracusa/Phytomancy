using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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

    public override void AdjustRender()
    {

    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Ground(vars);
    }

    public override TileElement LoadTileElement(params object[] vars)
    {
        Ground g = new Ground(vars);
        g.SetPhysics(true, false, true, false);
        object[] s = ((IEnumerable)vars[1]).Cast<object>().Select(x => x == null? x : (Shade)x).ToArray();
        for (int i = 0; i < 6; i++)
        {
            g.SetShade((Shade)s[i], i);
        }
        return g;
    }

    public override void CompileTileElement(ref LinkedList<int> dataInts, ref LinkedList<Shade> dataShades)
    {

    }

    public override TileElement DecompileTileElement(ref Queue<int> dataInts, ref Queue<Shade> dataShades)
    {
        return new Ground();
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

    public override TileElementNames TileID()
    {
        return TileElementNames.Ground;
    }

    public void SetShade (Shade shade, int index)
    {
        facets[index] = shade;
    }

    public Shade[] GetShades ()
    {
        return facets;
    }

    public void ColorFacets(Color[] palette)
    {
        for (int i = 0; i < 6; i++)
        {
            Debug.Log(facets.Length);
            model.transform.GetChild(i).GetComponent<MeshRenderer>().material.color = palette[(int)facets[i]];
        }
    }
}