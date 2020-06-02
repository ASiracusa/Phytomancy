using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : Monocoord
{
    private Shade[] facets;

    public Ground (params object[] vars)
    {
        SetCoords((int[])vars[0]);
        facets = (Shade[])vars[1];
    }

    public static TileElement GenerateTileElement(params object[] vars)
    {
        return new Ground(vars);
    }

    public void ColorFacet (Facet facet, Shade shade)
    {
        facets[(int)facet] = shade;
    }
}
