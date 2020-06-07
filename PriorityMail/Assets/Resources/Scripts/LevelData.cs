[System.Serializable]
public class LevelData
{
    public string levelName;
    public int[] brambleCoords;
    public Facet brambleDirection;
    public int[] sigilCoords;
    Shade[,,][] grounds;

    public LevelData(string _levelName, int[] _brambleCoords, Facet _brambleDirection, int[] _sigilCoords, Shade[,,][] _grounds)
    {
        levelName = _levelName;
        brambleCoords = _brambleCoords;
        brambleDirection = _brambleDirection;
        sigilCoords = _sigilCoords;
        grounds = _grounds;
    }
}