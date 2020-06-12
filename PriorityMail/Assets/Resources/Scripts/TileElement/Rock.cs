using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Monocoord
{
    public Rock() { }

    private Rock(params object[] vars)
    {
        SetCoords(new int[] {
            ((Vector3Int)vars[0]).x,
            ((Vector3Int)vars[0]).y,
            ((Vector3Int)vars[0]).z
        });
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Rock(vars);
    }

    public override TileElement LoadTileElement(params object[] vars)
    {
        Rock rock = new Rock(vars);
        rock.SetPhysics(false, true, false, true);
        return rock;
    }

    public override void CompileTileElement(ref LinkedList<int> dataInts, ref LinkedList<Shade> dataShades)
    {
        dataInts.AddLast(GetPos().x);
        dataInts.AddLast(GetPos().y);
        dataInts.AddLast(GetPos().z);
    }

    public override TileElement DecompileTileElement(ref Queue<int> dataInts, ref Queue<Shade> dataShades)
    {
        Rock rock = new Rock();
        rock.SetCoords(new int[]
        {
            dataInts.Dequeue(),
            dataInts.Dequeue(),
            dataInts.Dequeue()
        });
        rock.SetPhysics(false, true, false, true);
        return rock;
    }

    public override EditorTEIndices[] GetEditorTEIndices()
    {
        return new EditorTEIndices[]
        {
            EditorTEIndices.Pos1,
        };
    }

    public override string TileName()
    {
        return "Rock";
    }

    public override TileElementNames TileID()
    {
        return TileElementNames.Rock;
    }

}
