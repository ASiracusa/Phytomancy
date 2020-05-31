using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : Monocoord
{
    private Shade[] facets;

    public Ground (params object[] vars)
    {
        facets = vars[0] as Shade[];
    }
}
