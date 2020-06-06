using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class CreatorManager : MonoBehaviour
{
    public static CreatorManager current;

    private GameObject[,,] board;

    // EDITOR DATA
    private Vector3Int primarySelection;
    private TileElement tileModel;

    // BOARD DATA
    private Vector3Int brambleCoords;

    private delegate void MonocoordFunction(int x, int y, int z);



    void Start()
    {
        current = this;

        board = new GameObject[20, 10, 20];
        primarySelection = Vector3Int.zero;

        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                GameObject bottom = Instantiate(Resources.Load("Prefabs/BoardElements/floor"), new Vector3(x - 9.5f, 0, z - 9.5f), Quaternion.identity) as GameObject;
                board[x, 0, z] = bottom;
            }
        }

        CameraManager.current.onClick += SetPrimarySelection;
        CameraManager.current.onRelease += ExecuteSelection;

        GenerateTileMenu();
    }



    // Returns the coords of the tile next to a selected block depending on which facet was clicked.
    private static Vector3Int GetAdjacentCoords (RaycastHit hit) 
    {
        float xDist = hit.point.x - hit.transform.position.x;
        float yDist = hit.point.y - hit.transform.position.y;
        float zDist = hit.point.z - hit.transform.position.z;

        if (Mathf.Abs(xDist) > Mathf.Abs(yDist) && Mathf.Abs(xDist) > Mathf.Abs(zDist))
        {
            return new Vector3Int((int)(hit.transform.position.x + 9.5f + (xDist > 0 ? 1 : -1)), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f));
        }
        else if (Mathf.Abs(yDist) > Mathf.Abs(xDist) && Mathf.Abs(yDist) > Mathf.Abs(zDist))
        {
            return new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y + (yDist > 0 ? 1 : -1)), (int)(hit.transform.position.z + 9.5f));
        }
        else
        {
            return new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f + (zDist > 0 ? 1 : -1)));
        }

    }



    private void RemoveTile (int x, int y, int z)
    {
        GameObject.Destroy(board[x, y, z]);
        board[x, y, z] = null;
    }



    private void PlaceStandardTile (int x, int y, int z)
    {
        GameObject tile = Instantiate(Resources.Load("Prefabs/BoardElements/floor"), new Vector3(x - 9.5f, y, z - 9.5f), Quaternion.identity) as GameObject;
        if (board[x, y, z] != null)
        {
            GameObject.Destroy(board[x, y, z]);
        }
        board[x, y, z] = tile;
    }



    private void SetPrimarySelection (bool left, RaycastHit hit)
    {
        if (left)
        {
            primarySelection = GetAdjacentCoords(hit);
        }
        else
        {
            primarySelection = new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f));
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
            secondarySelection = new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f));
            tsf = RemoveTile;
        }

        for (int x = (primarySelection.x > secondarySelection.x ? secondarySelection.x : primarySelection.x); x <= (primarySelection.x < secondarySelection.x ? secondarySelection.x : primarySelection.x); x++)
        {
            for (int y = (primarySelection.y > secondarySelection.y ? secondarySelection.y : primarySelection.y); y <= (primarySelection.y < secondarySelection.y ? secondarySelection.y : primarySelection.y); y++)
            {
                for (int z = (primarySelection.z > secondarySelection.z ? secondarySelection.z : primarySelection.z); z <= (primarySelection.z < secondarySelection.z ? secondarySelection.z : primarySelection.z); z++)
                {
                    tsf(x, y, z);
                }
            }
        }
    }



    private void GenerateTileMenu ()
    {
        GameObject tileMenu = GameObject.Find("EditorCanvas/LeftMenu/AddMenu/TEMenu");
        for (int i = 0; i < Constants.TILE_MODELS.Length; i++)
        {
            GameObject icon = Instantiate(Resources.Load<GameObject>("Prefabs/TEIcon"), tileMenu.transform) as GameObject;
            icon.transform.localPosition = new Vector3(0, 135 - 55 * i, 0);
            icon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Enum.ToObject(typeof(TileElementNames), i).ToString();
        }
    }
}