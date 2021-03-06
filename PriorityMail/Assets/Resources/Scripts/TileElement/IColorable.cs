﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorable
{
    void SetShade(Shade shade, int index);

    void SetShades(Shade shade);

    void ColorFacets(Color[] palette);

    void ColorFacets(Material[] materials);
}
