using System.Collections.Generic;

public enum Facet
{
    Up = 0,
    Down = 1,
    North = 2,
    South = 3,
    West = 4,
    East = 5
}

public enum Shade
{
    Nothing,
    Color1,
    Color2,
    Color3,
    Color4,
    Color5,
    Color6,
    Color7,
    Color8,
    Color9,
    Color10
}

public enum Direction
{
    West = 1,
    East = -1,
    Up = 1,
    Down = -1,
    North = 1,
    South = -1
}

public enum TileElementNames
{
    Bramble,
    Sigil,
    Ground
}

public enum EditorTEIndices
{
    Pos1,
    Pos2,
    Direction
}

public delegate TileElement TileElementConstructor(params object[] vars);

public class Constants
{

    public static readonly TileElement[] TILE_MODELS = new TileElement[] {
        new Bramble(),
        new Sigil(),
        new Ground()
    };

}