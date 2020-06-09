using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMonoSpacious
{
    MonoSpaciousHelper Helper { get; set; }
    bool Expecting { get; set; }

    void TileEnters(TileElement enterer);

    void TileLeaves(TileElement leaver);
}

public class MonoSpaciousHelper
{
    public TileElement Inhabitant { get; set; }

    public void RemoveInhabitant ()
    {
        Inhabitant = null;
    }

    public TileElement GetSolidOccupant()
    {
        if (Inhabitant != null && Inhabitant is IMonoSpacious) {
            return ((IMonoSpacious)Inhabitant).Helper.GetSolidOccupant();
        }
        else if (Inhabitant != null)
        {
            return Inhabitant;
        }
        return null;
    }
}