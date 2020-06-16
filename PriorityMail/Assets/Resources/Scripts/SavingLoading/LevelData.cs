[System.Serializable]
public class LevelData
{
    public string levelName;
    public int[] brambleCoords;
    public Facet brambleDirection;
    public int[] sigilCoords;
    public Shade[,,][] grounds;

    public TileElementNames[] tileTypes;
    public int[] dataInts;
    public Shade[] dataShades;

    public LevelData(string _levelName, int[] _brambleCoords, Facet _brambleDirection, int[] _sigilCoords, Shade[,,][] _grounds, TileElementNames[] _tileTypes, int[] _dataInts, Shade[] _dataShades)
    {
        levelName = _levelName;
        brambleCoords = _brambleCoords;
        brambleDirection = _brambleDirection;
        sigilCoords = _sigilCoords;
        grounds = _grounds;
        tileTypes = _tileTypes;
        dataInts = _dataInts;
        dataShades = _dataShades;
    }
}