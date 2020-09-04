using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Linq;

public class CreatorManager : MonoBehaviour
{
    public static CreatorManager current;

    // EDITOR DATA
    private Vector3Int primarySelection;
    private TileElement tileModel;
    private Facet direction;
    private Shade paintColor;



    void Start()
    {
        current = this;

        primarySelection = Vector3Int.zero;
        direction = Facet.North;
        //GenerateNewLevel();

        GenerateTileMenu();
        StartCoroutine(ChangeDirection());

        GameObject.Find("EditorCanvas").SetActive(false);
    }



    private void RemoveTile(int x, int y, int z)
    {
        WorldManager.current.board[x, y, z].RemoveModel();
        WorldManager.current.board[x, y, z] = null;
    }



    private void PlaceStandardTile(int x, int y, int z)
    {
        if (WorldManager.current.board[x, y, z] != null)
        {
            WorldManager.current.board[x, y, z].RemoveModel();
        }
        WorldManager.current.board[x, y, z].GenerateTileElement();
    }



    private void SetPrimarySelection(bool left, RaycastHit hit)
    {
        primarySelection = CameraManager.GetAdjacentCoords(hit, left);
    }



    private void ExecuteSelection(bool left, RaycastHit hit)
    {
        TileElement[,,] board = WorldManager.current.board;
        Vector3Int secondarySelection = CameraManager.GetAdjacentCoords(hit, left);

        NormalizeCoords(ref primarySelection, ref secondarySelection);

        if (primarySelection.x < 0 || primarySelection.y < 0 || primarySelection.z < 0 ||
            secondarySelection.x >= board.GetLength(0) || secondarySelection.y >= board.GetLength(1) || secondarySelection.z >= board.GetLength(2))
        {
            return;
        }

        object[] constructorVals = new object[]
        {
            primarySelection,
            secondarySelection,
            direction
        };

        if (!left)
        {
            for (int x = primarySelection.x; x <= secondarySelection.x; x++)
            {
                for (int y = primarySelection.y; y <= secondarySelection.y; y++)
                {
                    for (int z = primarySelection.z; z <= secondarySelection.z; z++)
                    {
                        if (board[x, y, z] != null && board[x, y, z] is Bramble)
                        {
                            WorldManager.current.bramble = null;
                        }
                        if (board[x, y, z] != null && board[x, y, z] is Sigil)
                        {
                            WorldManager.current.sigil = null;
                        }
                        board[x, y, z]?.EditorDeleteTileElement(board);
                    }
                }
            }
        }
        else if (tileModel is Dicoord)
        {
            EditorTEIndices[] etei = tileModel.GetEditorTEIndices();
            object[] data = new object[etei.Length];
            for (int d = 0; d < data.Length; d++)
            {
                data[d] = constructorVals[(int)etei[d]];
            }

            Dicoord dicoord = (Dicoord)tileModel.GenerateTileElement(data);
            dicoord.model = Instantiate(Resources.Load("Models/" + tileModel.TileName())) as GameObject;
            dicoord.BindDataToModel();
            dicoord.WarpToPos();
            dicoord.AdjustRender();
            if (dicoord is IColorable)
            {
                ((IColorable)dicoord).ColorFacets(WorldManager.current.materials);
            }

            for (int x = primarySelection.x; x <= secondarySelection.x; x++)
            {
                for (int y = primarySelection.y; y <= secondarySelection.y; y++)
                {
                    for (int z = primarySelection.z; z <= secondarySelection.z; z++)
                    {
                        if (board[x, y, z] != null && board[x, y, z] is Bramble)
                        {
                            WorldManager.current.bramble = null;
                        }
                        if (board[x, y, z] != null && board[x, y, z] is Sigil)
                        {
                            WorldManager.current.sigil = null;
                        }
                        board[x, y, z]?.EditorDeleteTileElement(board);
                        board[x, y, z] = dicoord;
                    }
                }
            }
        }
        else if (tileModel is Bramble)
        {
            if (primarySelection.Equals(secondarySelection))
            {
                if (WorldManager.current.bramble != null)
                {
                    board[WorldManager.current.bramble.GetPos().x, WorldManager.current.bramble.GetPos().y, WorldManager.current.bramble.GetPos().z].EditorDeleteTileElement(board);
                }

                EditorTEIndices[] etei = tileModel.GetEditorTEIndices();
                object[] data = new object[etei.Length];
                for (int d = 0; d < data.Length; d++)
                {
                    data[d] = constructorVals[(int)etei[d]];
                }
                board[primarySelection.x, primarySelection.y, primarySelection.z] = tileModel.GenerateTileElement(data);
                board[primarySelection.x, primarySelection.y, primarySelection.z].model = Instantiate(Resources.Load("Models/Bramble")) as GameObject;
                board[primarySelection.x, primarySelection.y, primarySelection.z].BindDataToModel();
                board[primarySelection.x, primarySelection.y, primarySelection.z].WarpToPos();
                WorldManager.current.bramble = (Bramble)board[primarySelection.x, primarySelection.y, primarySelection.z];
            }
        }
        else if (tileModel is Sigil)
        {
            if (primarySelection.Equals(secondarySelection))
            {
                if (WorldManager.current.sigil != null)
                {
                    board[WorldManager.current.sigil.GetPos().x, WorldManager.current.sigil.GetPos().y, WorldManager.current.sigil.GetPos().z].EditorDeleteTileElement(board);
                }

                EditorTEIndices[] etei = tileModel.GetEditorTEIndices();
                object[] data = new object[etei.Length];
                for (int d = 0; d < data.Length; d++)
                {
                    data[d] = constructorVals[(int)etei[d]];
                }
                board[primarySelection.x, primarySelection.y, primarySelection.z] = tileModel.GenerateTileElement(data);
                board[primarySelection.x, primarySelection.y, primarySelection.z].model = Instantiate(Resources.Load("Models/Sigil")) as GameObject;
                board[primarySelection.x, primarySelection.y, primarySelection.z].BindDataToModel();
                board[primarySelection.x, primarySelection.y, primarySelection.z].WarpToPos();
                WorldManager.current.sigil = (Sigil)board[primarySelection.x, primarySelection.y, primarySelection.z];
            }
        }
        else
        {
            EditorTEIndices[] etei = tileModel.GetEditorTEIndices();
            object[] data = new object[etei.Length];

            for (int x = primarySelection.x; x <= secondarySelection.x; x++)
            {
                for (int y = primarySelection.y; y <= secondarySelection.y; y++)
                {
                    for (int z = primarySelection.z; z <= secondarySelection.z; z++)
                    {
                        if (board[x, y, z] != null && board[x, y, z] is Bramble)
                        {
                            WorldManager.current.bramble = null;
                        }
                        if (board[x, y, z] != null && board[x, y, z] is Sigil)
                        {
                            WorldManager.current.sigil = null;
                        }
                        if (board[x, y, z] != null)
                        {
                            board[x, y, z].EditorDeleteTileElement(board);
                        }
                        constructorVals[0] = new Vector3Int(x, y, z);
                        for (int d = 0; d < data.Length; d++)
                        {
                            data[d] = constructorVals[(int)etei[d]];
                        }
                        board[x, y, z] = tileModel.GenerateTileElement(data);
                        board[x, y, z].model = Instantiate(Resources.Load("Models/" + tileModel.TileName())) as GameObject;
                        board[x, y, z].BindDataToModel();
                        board[x, y, z].WarpToPos();
                        if (board[x, y, z] is IColorable)
                        {
                            ((IColorable)board[x, y, z]).ColorFacets(WorldManager.current.materials);
                        }
                    }
                }
            }
        }
    }



    public void GenerateTileMenu()
    {
        GameObject tileMenu = GameObject.Find("EditorCanvas/LeftMenu/EditMenu/TEMenu");
        for (int i = 1; i < Constants.TILE_MODELS.Length; i++)
        {
            GameObject icon = Instantiate(Resources.Load<GameObject>("Prefabs/TEIcon"), tileMenu.transform) as GameObject;
            icon.transform.localPosition = new Vector3(0, 135 - 55 * i, 0);
            icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Enum.ToObject(typeof(TileElementNames), i).ToString();
            int _i = i;
            icon.GetComponent<Button>().onClick.AddListener(delegate { ChangeTileModel(_i); });
        }
    }



    public void GeneratePaletteMenu()
    {
        GameObject colorMenu = GameObject.Find("EditorCanvas/LeftMenu/ColorMenu");
        for (int i = 0; i < 10; i++)
        {
            GameObject button = Instantiate(Resources.Load<GameObject>("Prefabs/PaletteButton"), colorMenu.transform) as GameObject;
            button.transform.localPosition = new Vector3(-120 + (60 * (i % 5)), 130 - (80 * (i / 5)), 1);
            int _i = i;
            button.GetComponent<Button>().onClick.AddListener(delegate { ChangeColor(_i + 1); });
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = WorldManager.current.levelData.availableVines.Length == 0 ? "0" : WorldManager.current.levelData.availableVines[i].ToString();
            button.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { EditAvailableVines(_i, true); });
            button.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { EditAvailableVines(_i, false); });
            button.GetComponent<Image>().color = WorldManager.current.palette[i + 1];
        }
    }



    public void ResizeBoard()
    {
        TileElement[,,] board = WorldManager.current.board;

        int _x = (int)GameObject.Find("EditorCanvas/LeftMenu/InfoMenu/XSlider").GetComponent<Slider>().value;
        int _y = (int)GameObject.Find("EditorCanvas/LeftMenu/InfoMenu/YSlider").GetComponent<Slider>().value;
        int _z = (int)GameObject.Find("EditorCanvas/LeftMenu/InfoMenu/ZSlider").GetComponent<Slider>().value;

        TileElement[,,] _board = new TileElement[_x, _y, _z];
        for (int x = 0; x < Math.Min(_x, board.GetLength(0)); x++)
        {
            for (int y = 0; y < Math.Min(_y, board.GetLength(1)); y++)
            {
                for (int z = 0; z < Math.Min(_z, board.GetLength(2)); z++)
                {
                    _board[x, y, z] = board[x, y, z];
                }
            }
        }

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int z = 0; z < board.GetLength(2); z++)
                {
                    if (board[x, y, z] != null && (x >= _x || y >= _y || z >= _z))
                    {
                        board[x, y, z].EditorDeleteTileElement(_board);
                    }
                }
            }
        }

        board = _board;
        CameraManager.current.CalibrateCamera(board);
    }



    private void NormalizeCoords(ref Vector3Int vec1, ref Vector3Int vec2)
    {
        int x1 = Math.Min(vec1.x, vec2.x);
        int x2 = Math.Max(vec1.x, vec2.x);
        int y1 = Math.Min(vec1.y, vec2.y);
        int y2 = Math.Max(vec1.y, vec2.y);
        int z1 = Math.Min(vec1.z, vec2.z);
        int z2 = Math.Max(vec1.z, vec2.z);

        vec1.x = x1;
        vec1.y = y1;
        vec1.z = z1;
        vec2.x = x2;
        vec2.y = y2;
        vec2.z = z2;
    }



    private IEnumerator ChangeDirection()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                direction = Facet.North;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                direction = Facet.South;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                direction = Facet.West;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                direction = Facet.East;
            }
            yield return null;
        }
    }



    public void ChangeMenu(int menu)
    {
        CameraManager.current.onClickBoth -= SetPrimarySelection;
        CameraManager.current.onReleaseBoth -= ExecuteSelection;
        CameraManager.current.onHoldBoth -= ColorMesh;

        if (menu == 0)
        {

        }
        else if (menu == 1)
        {
            CameraManager.current.onClickBoth += SetPrimarySelection;
            CameraManager.current.onReleaseBoth += ExecuteSelection;
        }
        else if (menu == 2)
        {
            CameraManager.current.onHoldBoth += ColorMesh;
        }
    }



    private void ColorMesh(bool left, RaycastHit hit)
    {
        if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>() != null)
        {
            hit.transform.gameObject.GetComponent<MeshRenderer>().material = WorldManager.current.materials[(int)paintColor];
            ((IColorable)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).SetShade(paintColor,
                hit.transform.gameObject.GetComponent<ColoredMeshBridge>().index);
        }
    }



    public void ChangeColor(int colorIndex)
    {
        paintColor = (Shade)colorIndex;
    }



    public void ChangeTileModel(int index)
    {
        print(index);
        tileModel = Constants.TILE_MODELS[index];
    }



    public void SaveLevel()
    {
        // Don't save if the vital components are missing or if the level is unnamed
        if (WorldManager.current.bramble == null || WorldManager.current.sigil == null)
        {
            return;
        }

        if (GameObject.Find("EditorCanvas/LeftMenu/InfoMenu/LevelName").GetComponent<TMP_InputField>().text.Length == 0)
        {
            return;
        }

        // Initialize arrays and lists
        TileElement[,,] board = WorldManager.current.board;
        Shade[,,][] grounds = new Shade[board.GetLength(0), board.GetLength(1), board.GetLength(2)][];

        LinkedList<TileElementNames> dataTypes = new LinkedList<TileElementNames>();
        LinkedList<int> dataInts = new LinkedList<int>();
        LinkedList<Shade> dataShades = new LinkedList<Shade>();

        // Compile the non-essential elements and grounds in their respective lists/arrays
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int z = 0; z < board.GetLength(2); z++)
                {
                    if (board[x, y, z] != null && !(board[x, y, z] is Ground) && !(board[x, y, z] is Bramble) && !(board[x, y, z] is Sigil) && !board[x, y, z].Checked)
                    {
                        board[x, y, z].Checked = true;
                        dataTypes.AddLast(board[x, y, z].TileID());
                        board[x, y, z].CompileTileElement(ref dataInts, ref dataShades);
                        print(dataInts.Last.Value.ToString());
                    }
                    if (board[x, y, z] is Ground)
                    {
                        grounds[x, y, z] = ((Ground)(board[x, y, z])).GetShades();
                    }
                }
            }
        }

        // Create the LevelData
        LevelData _ld = new LevelData(
            GameObject.Find("EditorCanvas/LeftMenu/InfoMenu/LevelName").GetComponent<TMP_InputField>().text,
            new int[]
            {
                WorldManager.current.bramble.GetPos().x,
                WorldManager.current.bramble.GetPos().y,
                WorldManager.current.bramble.GetPos().z
            },
            WorldManager.current.bramble.GetDirection(),
            new int[]
            {
                WorldManager.current.sigil.GetPos().x,
                WorldManager.current.sigil.GetPos().y,
                WorldManager.current.sigil.GetPos().z
            },
            grounds,
            WorldManager.current.availableVines,
            dataTypes.ToArray(),
            dataInts.ToArray(),
            dataShades.ToArray()
        );

        // Create the file for the level
        SerializationManager.SaveLevel(EditorMenuManager.current.GetWorldName(), _ld.levelName, _ld);

        LevelData ld = (LevelData)SerializationManager.LoadData(Application.persistentDataPath + "/worlds/" + EditorMenuManager.current.GetWorldName() + "/" + _ld.levelName + ".lvl");
    }



    public void GenerateNewLevel()
    {
        tileModel = Constants.TILE_MODELS[(int)TileElementNames.Ground];

        WorldManager.current.board = new TileElement[20, 10, 20];
        TileElement[,,] board = WorldManager.current.board;

        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                Ground bottom = (Ground)tileModel.GenerateTileElement(
                    new Vector3Int(x, 0, z)
                );
                board[x, 0, z] = bottom;
                bottom.model = Instantiate(Resources.Load("Models/" + tileModel.TileName())) as GameObject;
                bottom.ColorFacets(WorldManager.current.materials);
                board[x, 0, z].BindDataToModel();
                bottom.WarpToPos();
            }
        }

        GameObject.Find("EditorCanvas/LeftMenu/InfoMenu/LevelName").GetComponent<TMP_InputField>().text = "";
        WorldManager.current.availableVines = new int[10];
        
        CameraManager.current.CalibrateCamera(board);
    }

    public void OpenLevel()
    {
        CameraManager.current.onClickBoth += SetPrimarySelection;
        CameraManager.current.onReleaseBoth += ExecuteSelection;
        
        GeneratePaletteMenu();
        GameObject.Find("LevelAnchor/CameraAnchor/Camera").GetComponent<Camera>().backgroundColor = WorldManager.current.palette[0];

        StartCoroutine(ShowAndHideEditorMenu());
    }

    public void LeaveLevel()
    {
        CameraManager.current.onClickBoth -= SetPrimarySelection;
        CameraManager.current.onReleaseBoth -= ExecuteSelection;

        WorldManager.current.availableVines = null;
        WorldManager.current.RemoveBoard();
        foreach (Transform t in GameObject.Find("EditorCanvas/LeftMenu/ColorMenu").transform)
        {
            Destroy(t.gameObject);
        }

        StopCoroutine(ShowAndHideEditorMenu());
    }

    public IEnumerator ShowAndHideEditorMenu()
    {
        bool usingMenu = true;
        Vector3 targetPos = new Vector3(-200, 0, 0);
        while (true)
        {
            if (!usingMenu && Input.mousePosition.x / Screen.width < 0.05f)
            {
                usingMenu = true;
                targetPos = new Vector3(-200, 0, 0);

            }
            if (usingMenu && Input.mousePosition.x / Screen.width > 0.5f)
            {
                usingMenu = false;
                targetPos = new Vector3(-550, 0, 0);
            }
            if (GameObject.Find("EditorCanvas/LeftMenu") != null)
            {
                GameObject.Find("EditorCanvas/LeftMenu").transform.localPosition = Vector3.Lerp(GameObject.Find("EditorCanvas/LeftMenu").transform.localPosition, targetPos, 0.2f);
            }
            yield return null;
        }
    }

    private void EditAvailableVines(int shade, bool increase)
    {
        if (increase)
        {
            WorldManager.current.availableVines[shade] = WorldManager.current.availableVines[shade] + 1;

        }
        else if (WorldManager.current.availableVines[shade] > 0)
        {
            WorldManager.current.availableVines[shade] = WorldManager.current.availableVines[shade] - 1;
        }
        GameObject.Find("EditorCanvas/LeftMenu/ColorMenu").transform.GetChild(shade).GetChild(0).GetComponent<TextMeshProUGUI>().text = WorldManager.current.availableVines[shade].ToString();
    }

    public void LoadLevel(string levelPath)
    {
        WorldManager.current.LoadLevel(levelPath, false);
    }
}