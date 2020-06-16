using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : Dicoord
{
    public Boulder() { }

    private Boulder(params object[] vars)
    {
        SetCoords(new int[] {
            ((Vector3Int)vars[0]).x,
            ((Vector3Int)vars[0]).y,
            ((Vector3Int)vars[0]).z
        },
        new int[]
        {
            ((Vector3Int)vars[1]).x,
            ((Vector3Int)vars[1]).y,
            ((Vector3Int)vars[1]).z
        });
    }

    public override void AdjustRender()
    {
        model.transform.GetChild(0).localScale = GetPos2() - GetPos1() + new Vector3(0.6f, 1, 1);
        model.transform.GetChild(1).localScale = GetPos2() - GetPos1() + new Vector3(1, 0.6f, 1);
        model.transform.GetChild(2).localScale = GetPos2() - GetPos1() + new Vector3(1, 1, 0.6f);
        model.transform.GetChild(3).localPosition = new Vector3((GetPos2().x - GetPos1().x) / 2.0f + 0.4f, (GetPos2().y - GetPos1().y) / 2.0f + 0.4f, (GetPos2().z - GetPos1().z) / 2.0f + 0.4f);
        model.transform.GetChild(4).localPosition = new Vector3((GetPos2().x - GetPos1().x) / 2.0f + 0.4f, -((GetPos2().y - GetPos1().y) / 2.0f + 0.4f), (GetPos2().z - GetPos1().z) / 2.0f + 0.4f);
        model.transform.GetChild(5).localPosition = new Vector3(-((GetPos2().x - GetPos1().x) / 2.0f + 0.4f), (GetPos2().y - GetPos1().y) / 2.0f + 0.4f, (GetPos2().z - GetPos1().z) / 2.0f + 0.4f);
        model.transform.GetChild(6).localPosition = new Vector3(-((GetPos2().x - GetPos1().x) / 2.0f + 0.4f), -((GetPos2().y - GetPos1().y) / 2.0f + 0.4f), (GetPos2().z - GetPos1().z) / 2.0f + 0.4f);
        model.transform.GetChild(7).localPosition = new Vector3(-((GetPos2().x - GetPos1().x) / 2.0f + 0.4f), (GetPos2().y - GetPos1().y) / 2.0f + 0.4f, -((GetPos2().z - GetPos1().z) / 2.0f + 0.4f));
        model.transform.GetChild(8).localPosition = new Vector3(-((GetPos2().x - GetPos1().x) / 2.0f + 0.4f), -((GetPos2().y - GetPos1().y) / 2.0f + 0.4f), -((GetPos2().z - GetPos1().z) / 2.0f + 0.4f));
        model.transform.GetChild(9).localPosition = new Vector3((GetPos2().x - GetPos1().x) / 2.0f + 0.4f, (GetPos2().y - GetPos1().y) / 2.0f + 0.4f, -((GetPos2().z - GetPos1().z) / 2.0f + 0.4f));
        model.transform.GetChild(10).localPosition = new Vector3((GetPos2().x - GetPos1().x) / 2.0f + 0.4f, -((GetPos2().y - GetPos1().y) / 2.0f + 0.4f), -((GetPos2().z - GetPos1().z) / 2.0f + 0.4f));
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Boulder(vars);
    }

    public override TileElement LoadTileElement(params object[] vars)
    {
        Boulder boulder = new Boulder(vars);
        boulder.SetPhysics(false, true, false, false);
        return boulder;
    }

    public override void CompileTileElement(ref LinkedList<int> dataInts, ref LinkedList<Shade> dataShades)
    {
        dataInts.AddLast(GetPos1().x);
        dataInts.AddLast(GetPos1().y);
        dataInts.AddLast(GetPos1().z);
        dataInts.AddLast(GetPos2().x);
        dataInts.AddLast(GetPos2().y);
        dataInts.AddLast(GetPos2().z);
    }

    public override TileElement DecompileTileElement(ref Queue<int> dataInts, ref Queue<Shade> dataShades)
    {
        Boulder boulder = new Boulder();
        boulder.SetCoords(new int[]
        {
            dataInts.Dequeue(),
            dataInts.Dequeue(),
            dataInts.Dequeue()
        },
        new int[]
        {
            dataInts.Dequeue(),
            dataInts.Dequeue(),
            dataInts.Dequeue()
        });
        boulder.SetPhysics(false, true, false, false);
        return boulder;
    }

    public override EditorTEIndices[] GetEditorTEIndices()
    {
        return new EditorTEIndices[]
        {
            EditorTEIndices.Pos1,
            EditorTEIndices.Pos2
        };
    }

    public override string TileName()
    {
        return "Boulder";
    }

    public override TileElementNames TileID()
    {
        return TileElementNames.Boulder;
    }
}
