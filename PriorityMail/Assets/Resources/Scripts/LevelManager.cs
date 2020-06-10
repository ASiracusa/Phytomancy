using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager current;

    private LevelData levelData;

    private TileElement[,,] board;
    private Bramble bramble;
    private int[] availableVines;

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

    // Start is called before the first frame update
    void Start()
    {
        current = this;

        availableVines = new int[10] {
            5, 0, 0, 0, 0, 0, 0, 0, 0, 3
        };

        LoadLevel("auburn", "heights");
        CameraManager.current.onClick += CreateVine;

        StartCoroutine(BrambleInput());
    }

    private void LoadLevel (string worldName, string levelName)
    {
        levelData = (LevelData)SerializationManager.LoadLevel(Application.persistentDataPath + "/worlds/" + worldName + "/" + levelName + ".lvl");
        print(levelData.sigilCoords[0]);
        TileElement tileModel = Constants.TILE_MODELS[(int)TileElementNames.Ground];

        board = new TileElement[levelData.grounds.GetLength(0), levelData.grounds.GetLength(1), levelData.grounds.GetLength(2)];
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y ++)
            {
                for (int z = 0; z < board.GetLength(2); z++)
                {
                    if (levelData.grounds[x, y, z] != null)
                    {
                        board[x, y, z] = tileModel.LoadTileElement(new object[] {
                            new Vector3Int(x, y, z),
                            levelData.grounds[x, y, z]
                        });
                        board[x, y, z].model = Instantiate(Resources.Load("Models/Ground")) as GameObject;
                        board[x, y, z].BindDataToModel();
                        board[x, y, z].MoveToPos();
                        ((Ground)board[x, y, z]).ColorFacets(palette);
                    }
                }
            }
        }

        bramble = (Bramble)Constants.TILE_MODELS[(int)TileElementNames.Bramble].LoadTileElement(new object[]
        {
            new Vector3Int(levelData.brambleCoords[0], levelData.brambleCoords[1], levelData.brambleCoords[2]),
            levelData.brambleDirection
        });
        bramble.model = Instantiate(Resources.Load("Models/Bramble")) as GameObject;
        bramble.BindDataToModel();
        bramble.MoveToPos();
        board[bramble.GetPos().x, bramble.GetPos().y, bramble.GetPos().z] = bramble;

        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]] = (Sigil)Constants.TILE_MODELS[(int)TileElementNames.Sigil].LoadTileElement(new object[]
        {
            new Vector3Int(levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]),
        });
        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]].model = Instantiate(Resources.Load("Models/Sigil")) as GameObject;
        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]].BindDataToModel();
        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]].MoveToPos();
    }

    private IEnumerator BrambleInput ()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                bramble.Push(ref board, Facet.North, null);
                ClearSpaciousTiles();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                bramble.Push(ref board, Facet.South, null);
                ClearSpaciousTiles();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                bramble.Push(ref board, Facet.West, null);
                ClearSpaciousTiles();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                bramble.Push(ref board, Facet.East, null);
                ClearSpaciousTiles();
            }
            yield return null;
        }
    }

    private void CreateVine (bool left, RaycastHit hit)
    {
        if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>() != null && hit.transform.gameObject.layer == 8)
        {
            if (left)
            {
                Shade vineColor;
                if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data is Ground) {
                    vineColor = ((Ground)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).GetShades()[hit.transform.gameObject.GetComponent<ColoredMeshBridge>().index];
                }
                else
                {
                    vineColor = ((Vine)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).GetColor();
                }
                int vinesOfColor = availableVines[(int)vineColor - 1];
                Vector3Int stemCoords = new Vector3Int((int)(hit.transform.position.x), (int)(hit.transform.position.y), (int)(hit.transform.position.z));

                if (vinesOfColor > 0 && (!(board[stemCoords.x, stemCoords.y, stemCoords.z] is Vine) || ((Vine)board[stemCoords.x, stemCoords.y, stemCoords.z]).GetVine() == null))
                {
                    Vector3Int vineCoords = CameraManager.GetAdjacentCoords(hit);
                    TileElement tileAtPos = board[vineCoords.x, vineCoords.y, vineCoords.z];
                    Vector3Int direction = vineCoords - ((Monocoord)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).GetPos();
                    print(direction);
                    Vine vine = new Vine(new object[] {
                        vineCoords,
                        vineColor,
                        Constants.VectorToFacet(-direction)
                    });
                    if (tileAtPos != null && tileAtPos.Pushable && !tileAtPos.Weedblocked && !(tileAtPos is IMonoSpacious))
                    {
                        if (!board[vineCoords.x, vineCoords.y, vineCoords.z].Push(ref board, Constants.VectorToFacet(direction), vine))
                        {
                            return;
                        }
                    }
                    else
                    {
                        board[vineCoords.x, vineCoords.y, vineCoords.z] = vine;
                    }
                    board[vineCoords.x, vineCoords.y, vineCoords.z].model = Instantiate(Resources.Load("Models/Vine")) as GameObject;
                    board[vineCoords.x, vineCoords.y, vineCoords.z].model.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = palette[(int)vine.GetColor()];
                    board[vineCoords.x, vineCoords.y, vineCoords.z].BindDataToModel();
                    board[vineCoords.x, vineCoords.y, vineCoords.z].MoveToPos();
                    if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data is Vine)
                    {
                        ((Vine)(board[stemCoords.x, stemCoords.y, stemCoords.z])).SetVine((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]);
                    }
                }
            }
            else if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data is Vine)
            {
                Vector3Int vineCoords = new Vector3Int((int)(hit.transform.position.x), (int)(hit.transform.position.y), (int)(hit.transform.position.z));
                Vector3Int stemCoords = ((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]).GetPos() + Constants.FacetToVector(((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]).GetOrigin());

                ((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]).RemoveVine(board);
                if (board[stemCoords.x, stemCoords.y, stemCoords.z] is Vine)
                {
                    ((Vine)board[stemCoords.x, stemCoords.y, stemCoords.z]).SetVine(null);
                }
            }
        }
    }

    private void ClearSpaciousTiles ()
    {
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int z = 0; z < board.GetLength(2); z++)
                {
                    if (board[x, y, z] is IMonoSpacious)
                    {
                        print(x + " " + y + " " + z);
                        if (((IMonoSpacious)board[x, y, z]).Expecting)
                        {
                            ((IMonoSpacious)board[x, y, z]).TileLeaves();
                            ((IMonoSpacious)board[x, y, z]).Helper.Inhabitant = null;
                            ((IMonoSpacious)board[x, y, z]).Expecting = false;
                        }
                    }
                }
            }
        }
    }
}
