[System.Serializable]
public class DataSaver
{
    private static DataSaver _current;
    public static DataSaver current
    {
        get
        {
            if (_current == null)
            {
                _current = new DataSaver();
            }
            return _current;
        }
    }

    public LevelData levelData;
}