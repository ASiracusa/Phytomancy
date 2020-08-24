using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Vine : Monocoord
{
    private Vine next;
    private Shade color;
    private Facet origin;

    public Vine () { }

    public Vine (params object[] vars)
    {
        SetCoords(new int[] {
            ((Vector3Int)vars[0]).x,
            ((Vector3Int)vars[0]).y,
            ((Vector3Int)vars[0]).z
        });
        color = (Shade)(vars[1]);
        origin = (Facet)(vars[2]);
    }

    public override void AdjustRender()
    {
        model.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().materials = new Material[]
        {
            LevelManager.current.materials[(int)color],
            LevelManager.current.darkener
        };

        model.transform.GetChild(0).localPosition = Constants.VINE_ANCHORS[(int)origin];
        model.transform.GetChild(0).localScale = Constants.VINE_STRETCHES[(int)origin];
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        Vine v = new Vine(vars);
        v.SetPhysics(true, false, true, false);
        return v;
    }

    public override TileElement LoadTileElement(params object[] vars)
    {
        return new Vine(vars);
    }

    public override void CompileTileElement(ref LinkedList<int> dataInts, ref LinkedList<Shade> dataShades)
    {

    }

    public override TileElement DecompileTileElement(ref Queue<int> dataInts, ref Queue<Shade> dataShades)
    {
        return new Vine();
    }

    public override EditorTEIndices[] GetEditorTEIndices()
    {
        return new EditorTEIndices[]
        {
            EditorTEIndices.Pos1
        };
    }

    public Vine GetVine ()
    {
        return next;
    }

    public void SetVine (Vine _next)
    {
        next = _next;
    }

    public Shade GetColor ()
    {
        return color;
    }

    public Facet GetOrigin ()
    {
        return origin;
    }

    public override string TileName()
    {
        return "Vine";
    }

    public override TileElementNames TileID()
    {
        return TileElementNames.Vine;
    }

    public int RemoveVine (TileElement[,,] board)
    {
        if (next == null)
        {
            PlayerDeleteTileElement(board);
            return 1;
        }
        else
        {
            int v = next.RemoveVine(board);
            PlayerDeleteTileElement(board);
            return v + 1;
        }
    }
}
