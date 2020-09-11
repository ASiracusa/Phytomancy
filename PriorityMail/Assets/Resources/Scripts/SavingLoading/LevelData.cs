using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public int[] brambleCoords;
    public Facet brambleDirection;
    public int[] sigilCoords;
    public Shade[,,][] grounds;
    public int[] availableVines;

    public TileElementNames[] tileTypes;
    public int[] dataInts;
    public Shade[] dataShades;

    public LevelData(string _levelName, int[] _brambleCoords, Facet _brambleDirection, int[] _sigilCoords, Shade[,,][] _grounds, int[] _availableVines, TileElementNames[] _tileTypes, int[] _dataInts, Shade[] _dataShades)
    {
        levelName = _levelName;
        brambleCoords = _brambleCoords;
        brambleDirection = _brambleDirection;
        sigilCoords = _sigilCoords;
        grounds = _grounds;
        availableVines = _availableVines;
        tileTypes = _tileTypes;
        dataInts = _dataInts;
        dataShades = _dataShades;
    }
}

[System.Serializable]
public class LevelData2
{
    public string levelName;

    public int x;
    public int y;
    public int z;
    public float startRotation;
    public float startZoom;

    public int[] brambleCoords;
    public Facet brambleDirection;
    public int[] sigilCoords;

    public int[] availableVines;

    public int[][] grounds;
    public int[][] decals;

    public TileElementNames[] tileTypes;
    public int[] dataInts;
    public Shade[] dataShades;

    public LevelData2(string _levelName, int _x, int _y, int _z, float _startRotation, float _startZoom, int[] _brambleCoords, Facet _brambleDirection, int[] _sigilCoords, int[] _availableVines, int[][] _grounds, int[][] _decals, TileElementNames[] _tileTypes, int[] _dataInts, Shade[] _dataShades)
    {
        levelName = _levelName;

        x = _x;
        y = _y;
        z = _z;
        startRotation = _startRotation;
        startZoom = _startZoom;

        brambleCoords = _brambleCoords;
        brambleDirection = _brambleDirection;
        sigilCoords = _sigilCoords;

        availableVines = _availableVines;

        grounds = _grounds;
        decals = _decals;

        tileTypes = _tileTypes;
        dataInts = _dataInts;
        dataShades = _dataShades;
    }
}

[System.Serializable]
public class WorldData
{
    public string worldName;
    public float[] reds;
    public float[] greens;
    public float[] blues;
    public string[] levelOrder;

    public WorldData(string _worldName, float[] _reds, float[] _greens, float[] _blues, string[] _levelOrder)
    {
        worldName = _worldName;
        reds = _reds;
        greens = _greens;
        blues = _blues;
        levelOrder = _levelOrder;
    }
}