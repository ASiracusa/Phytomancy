using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Ground : Monocoord, IColorable
{
    private Shade[] facets;
    private int[] decalIds;
    private int[] decalRots;

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
        decalIds = new int[6];
        decalRots = new int[6];
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

    public void SetShades(Shade shade)
    {
        for (int i = 0; i < 6; i++)
        {
            facets[i] = shade;
        }
    }

    public Shade[] GetShades ()
    {
        return facets;
    }

    public void SetDecal (int index, int decalId, int decalRot)
    {
        decalIds[index] = decalId;
        decalRots[index] = decalRot;
    }

    public int[] GetDecalIds ()
    {
        return decalIds;
    }

    public int[] GetDecalRots ()
    {
        return decalRots;
    }

    public void ColorFacets(Color[] palette)
    {
        for (int i = 0; i < 6; i++)
        {
            Debug.Log(facets.Length);
            model.transform.GetChild(i).GetComponent<MeshRenderer>().material.color = palette[(int)facets[i]];
        }
    }

    public void ColorFacets(Material[] materials)
    {
        Debug.Log("coloring");
        for (int i = 0; i < 6; i++)
        {
            model.transform.GetChild(i).GetComponent<MeshRenderer>().material = materials[(int)facets[i]];
        }
    }
}