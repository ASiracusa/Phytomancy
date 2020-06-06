using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sigil : Monocoord
{
    public Sigil() { }

    public Sigil (params object[] vars)
    {
        SetCoords((int[])vars[0]);
    }

    public override TileElement GenerateTileElement(params object[] vars)
    {
        return new Sigil(vars);
    }
}
