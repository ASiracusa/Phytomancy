using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
