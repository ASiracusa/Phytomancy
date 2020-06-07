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

    // BOARD DATA
    private Vector3Int brambleCoords;

    private delegate void MonocoordFunction(int x, int y, int z);



    void Start()
    {
        current = this;
        tileModel = Constants.TILE_MODELS[(int)TileElementNames.Ground];

        board = new Ground[20, 10, 20];
        primarySelection = Vector3Int.zero;
        direction = Facet.North;

        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                Ground bottom = (Ground)tileModel.GenerateTileElement(
                    new Vector3Int(x, 0, z)
                );
                bottom.model = Instantiate(Resources.Load(tileModel.TileName())) as GameObject;
                bottom.MoveToPos();
                //Instantiate(Resources.Load("Prefabs/BoardElements/floor"), new Vector3(x - 9.5f, 0, z - 9.5f), Quaternion.identity) as GameObject;
                board[x, 0, z] = bottom;
            }
        }

        CameraManager.current.onClick += SetPrimarySelection;
        CameraManager.current.onRelease += ExecuteSelection;

        GenerateTileMenu();
        StartCoroutine(ChangeDirection());
    }



    // Returns the coords of the tile next to a selected block depending on which facet was clicked.
    private static Vector3Int GetAdjacentCoords (RaycastHit hit) 
    {
        float xDist = hit.point.x - hit.transform.position.x;
        float yDist = hit.point.y - hit.transform.position.y;
        float zDist = hit.point.z - hit.transform.position.z;

        if (Mathf.Abs(xDist) > Mathf.Abs(yDist) && Mathf.Abs(xDist) > Mathf.Abs(zDist))
        {
            return new Vector3Int((int)(hit.transform.position.x + (xDist > 0 ? 1 : -1)), (int)(hit.transform.position.y), (int)(hit.transform.position.z));
        }
        else if (Mathf.Abs(yDist) > Mathf.Abs(xDist) && Mathf.Abs(yDist) > Mathf.Abs(zDist))
        {
            return new Vector3Int((int)(hit.transform.position.x), (int)(hit.transform.position.y + (yDist > 0 ? 1 : -1)), (int)(hit.transform.position.z));
        }
        else
        {
            return new Vector3Int((int)(hit.transform.position.x), (int)(hit.transform.position.y), (int)(hit.transform.position.z + (zDist > 0 ? 1 : -1)));
        }

    }



    private static Vector3Int GetAdjacentTileElement (RaycastHit hit)
    {
        TileElement data = hit.collider.gameObject.GetComponent<ModelTileBridge>().Data;
        if (data is Monocoord)
        {
            return ((Monocoord)data).GetPos();
        }
        else if (data is Dicoord)
        {

        }
        return new Vector3Int(-1, -1, -1);
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
            primarySelection = GetAdjacentCoords(hit);
            print(primarySelection);
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
            secondarySelection = GetAdjacentCoords(hit);
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
                        board[x, y, z]?.DeleteTileElement(board);
                    }
                }
            }
        }
        else if (tileModel is Dicoord)
        {
            
        }
        else if (tileModel is Bramble)
        {

        }
        else if (tileModel is Sigil)
        {

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
                        if (board[x, y, z] != null)
                        {
                            board[x, y, z].DeleteTileElement(board);
                        }
                        constructorVals[0] = new Vector3Int(x, y, z);
                        for (int d = 0; d < data.Length; d++)
                        {
                            data[d] = constructorVals[(int)etei[d]];
                        }
                        board[x, y, z] = tileModel.GenerateTileElement(data);
                        board[x, y, z].model = Instantiate(Resources.Load(tileModel.TileName())) as GameObject;
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
                        board[x, y, z].DeleteTileElement(_board);
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
}