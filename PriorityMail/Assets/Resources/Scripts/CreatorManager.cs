using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class CreatorManager : MonoBehaviour
{
    public static CreatorManager current;

    private TileElement[,,] board;

    // EDITOR DATA
    private Vector3Int primarySelection;
    private TileElement tileModel;
    private Facet direction;
    private Shade paintColor;

    // BOARD DATA
    private Bramble bramble;
    private Sigil sigil;

    private Color32[] palette = new Color32[]
    {
        new Color32 (0x00, 0x00, 0x00, 0xFF),
        new Color32 (0x11, 0x00, 0x00, 0xFF),
        new Color32 (0x22, 0x00, 0x00, 0xFF),
        new Color32 (0x33, 0x00, 0x00, 0xFF),
        new Color32 (0x44, 0x00, 0x00, 0xFF),
        new Color32 (0x55, 0x00, 0x00, 0xFF),
        new Color32 (0x66, 0x00, 0x00, 0xFF),
        new Color32 (0x77, 0x00, 0x00, 0xFF),
        new Color32 (0x88, 0x00, 0x00, 0xFF),
        new Color32 (0x99, 0x00, 0x00, 0xFF),
        new Color32 (0xAA, 0x00, 0x00, 0xFF)
    };

    private delegate void MonocoordFunction(int x, int y, int z);



    void Start()
    {
        current = this;
        tileModel = Constants.TILE_MODELS[(int)TileElementNames.Ground];

        board = new TileElement[20, 10, 20];
        primarySelection = Vector3Int.zero;
        direction = Facet.North;

        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                Ground bottom = (Ground)tileModel.GenerateTileElement(
                    new Vector3Int(x, 0, z)
                );
                board[x, 0, z] = bottom;
                bottom.model = Instantiate(Resources.Load("Models/" + tileModel.TileName())) as GameObject;
                board[x, 0, z].BindDataToModel();
                bottom.MoveToPos();
            }
        }

        CameraManager.current.onClick += SetPrimarySelection;
        CameraManager.current.onRelease += ExecuteSelection;

        GenerateTileMenu();
        StartCoroutine(ChangeDirection());
    }



    private void RemoveTile (int x, int y, int z)
    {
        board[x, y, z].RemoveModel();
        board[x, y, z] = null;
    }



    private void PlaceStandardTile (int x, int y, int z)
    {
        if (board[x, y, z] != null)
        {
            board[x, y, z].RemoveModel();
        }
        board[x, y, z].GenerateTileElement();
    }



    private void SetPrimarySelection (bool left, RaycastHit hit)
    {
        if (left)
        {
            primarySelection = CameraManager.GetAdjacentCoords(hit);
        }
        else
        {
            primarySelection = new Vector3Int((int)(hit.transform.position.x), (int)(hit.transform.position.y), (int)(hit.transform.position.z));
        }
    }



    private void ExecuteSelection (bool left, RaycastHit hit)
    {
        Vector3Int secondarySelection;
        MonocoordFunction tsf;

        if (left)
        {
            secondarySelection = CameraManager.GetAdjacentCoords(hit);
            tsf = PlaceStandardTile;
        }
        else
        {
            secondarySelection = new Vector3Int((int)(hit.transform.position.x), (int)(hit.transform.position.y), (int)(hit.transform.position.z));
            print(hit.transform.position);
            tsf = RemoveTile;
        }

        NormalizeCoords(ref primarySelection, ref secondarySelection);

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
                            bramble = null;
                        }
                        if (board[x, y, z] != null && board[x, y, z] is Sigil)
                        {
                            sigil = null;
                        }
                        board[x, y, z]?.EditorDeleteTileElement(board);
                    }
                }
            }
        }
        else if (tileModel is Dicoord)
        {
            
        }
        else if (tileModel is Bramble)
        {
            if (primarySelection.Equals(secondarySelection))
            {
                if (bramble != null)
                {
                    board[bramble.GetPos().x, bramble.GetPos().y, bramble.GetPos().z].EditorDeleteTileElement(board);
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
                board[primarySelection.x, primarySelection.y, primarySelection.z].MoveToPos();
                bramble = (Bramble)board[primarySelection.x, primarySelection.y, primarySelection.z];
            }
        }
        else if (tileModel is Sigil)
        {
            if (primarySelection.Equals(secondarySelection))
            {
                if (sigil != null)
                {
                    board[sigil.GetPos().x, sigil.GetPos().y, sigil.GetPos().z].EditorDeleteTileElement(board);
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
                board[primarySelection.x, primarySelection.y, primarySelection.z].MoveToPos();
                sigil = (Sigil)board[primarySelection.x, primarySelection.y, primarySelection.z];
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
                            bramble = null;
                        }
                        if (board[x, y, z] != null && board[x, y, z] is Sigil)
                        {
                            sigil = null;
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
                        board[x, y, z].MoveToPos();
                    }
                }
            }
        }
    }



    public void GenerateTileMenu ()
    {
        GameObject tileMenu = GameObject.Find("EditorCanvas/LeftMenu/AddMenu/TEMenu");
        for (int i = 0; i < Constants.TILE_MODELS.Length; i++)
        {
            GameObject icon = Instantiate(Resources.Load<GameObject>("Prefabs/TEIcon"), tileMenu.transform) as GameObject;
            icon.transform.localPosition = new Vector3(0, 135 - 55 * i, 0);
            icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Enum.ToObject(typeof(TileElementNames), i).ToString();
            int _i = i;
            icon.GetComponent<Button>().onClick.AddListener(delegate { ChangeTileModel(_i); });
        }
    }



    public void ResizeBoard ()
    {
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
    }



    private void NormalizeCoords (ref Vector3Int vec1, ref Vector3Int vec2)
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



    private IEnumerator ChangeDirection ()
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



    public void ChangeMenu (int menu)
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
            CameraManager.current.onClickBoth += SetPrimarySelection;
            CameraManager.current.onReleaseBoth += ExecuteSelection;
        }
        else if (menu == 3)
        {
            CameraManager.current.onHoldBoth += ColorMesh;
        }
    }



    private void ColorMesh (bool left, RaycastHit hit)
    {
        if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>() != null)
        {
            hit.transform.gameObject.GetComponent<MeshRenderer>().material.color = palette[(int)paintColor];
            ((IColorable)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).SetShade(paintColor,
                hit.transform.gameObject.GetComponent<ColoredMeshBridge>().index);
        }
    }



    public void ChangeColor (int colorIndex)
    {
        paintColor = (Shade)colorIndex;
    }


    
    public void ChangeTileModel (int index)
    {
        print(index);
        tileModel = Constants.TILE_MODELS[index];
    }



    public void SaveLevel()
    {
        print(sigil.GetPos());

        Shade[,,][] grounds = new Shade[board.GetLength(0), board.GetLength(1), board.GetLength(2)][];
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int z = 0; z < board.GetLength(2); z++)
                {
                    if (board[x, y, z] is Ground)
                    {
                        grounds[x, y, z] = ((Ground)(board[x, y, z])).GetShades();
                    }
                }
            }
        }

        LevelData _ld = new LevelData(
            "heights",
            new int[]
            {
                bramble.GetPos().x,
                bramble.GetPos().y,
                bramble.GetPos().z
            },
            bramble.GetDirection(),
            new int[]
            {
                sigil.GetPos().x,
                sigil.GetPos().y,
                sigil.GetPos().z
            },
            grounds
        );
        print("_ld:");
        print(_ld.sigilCoords[0]);

        SerializationManager.SaveLevel("auburn", "heights", _ld);

        LevelData ld = (LevelData)SerializationManager.LoadLevel(Application.persistentDataPath + "/worlds/auburn/heights.lvl");
        print(ld.sigilCoords);
    }
}