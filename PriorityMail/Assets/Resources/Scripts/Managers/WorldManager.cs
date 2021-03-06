﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager current;

    // BOARD DATA
    public WorldData worldData;
    public LevelData levelData;
    public TileElement[,,] board;
    public Bramble bramble;
    public Sigil sigil;
    public int[] availableVines;

    // RENDERING AND MATERIALS
    public Color[] palette;
    public Material[] unlitBases;
    public Material[] litBases;
    public Material[] unlitDarks;
    public Material[] litDarks;
    public Material[] unlitLights;
    public Material[] litLights;
    public Material darkener;
    public Material voidGradient;

    private void Start()
    {
        current = this;
    }

    public void LoadLevel(string levelPath, bool playing)
    {
        // Load LevelData and initialize the lists
        levelData = (LevelData)SerializationManager.LoadData(levelPath);
        TileElement tileModel = Constants.TILE_MODELS[(int)TileElementNames.Ground];

        availableVines = levelData.availableVines;
        
        // Create the Grounds
        board = new TileElement[levelData.grounds.GetLength(0), levelData.grounds.GetLength(1), levelData.grounds.GetLength(2)];
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
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
                        board[x, y, z].WarpToPos();
                        ((Ground)board[x, y, z]).ColorFacets(litBases);
                    }
                }
            }
        }

        // Create Bramble and save his position
        bramble = (Bramble)Constants.TILE_MODELS[(int)TileElementNames.Bramble].LoadTileElement(new object[]
        {
            new Vector3Int(levelData.brambleCoords[0], levelData.brambleCoords[1], levelData.brambleCoords[2]),
            levelData.brambleDirection
        });
        bramble.model = Instantiate(Resources.Load("Models/Bramble")) as GameObject;
        bramble.BindDataToModel();
        bramble.WarpToPos();
        board[bramble.GetPos().x, bramble.GetPos().y, bramble.GetPos().z] = bramble;

        // Create the Sigil
        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]] = (Sigil)Constants.TILE_MODELS[(int)TileElementNames.Sigil].LoadTileElement(new object[]
        {
            new Vector3Int(levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]),
        });
        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]].model = Instantiate(Resources.Load("Models/Sigil")) as GameObject;
        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]].BindDataToModel();
        board[levelData.sigilCoords[0], levelData.sigilCoords[1], levelData.sigilCoords[2]].WarpToPos();

        // Convert the data arrays to Queues
        Queue<int> intQueue = new Queue<int>();
        for (int i = 0; i < levelData.dataInts.Length; i++)
        {
            intQueue.Enqueue(levelData.dataInts[i]);
        }
        Queue<Shade> shadeQueue = new Queue<Shade>();
        for (int i = 0; i < levelData.dataShades.Length; i++)
        {
            shadeQueue.Enqueue(levelData.dataShades[i]);
        }

        // Decompile all of the non-essential elements
        for (int i = 0; i < levelData.tileTypes.Length; i++)
        {
            TileElement tileBase = Constants.TILE_MODELS[(int)levelData.tileTypes[i]];
            TileElement decompiledTile = tileBase.DecompileTileElement(ref intQueue, ref shadeQueue);
            decompiledTile.model = Instantiate(Resources.Load("Models/" + tileBase.TileName())) as GameObject;
            decompiledTile.BindDataToModel();
            decompiledTile.WarpToPos();
            decompiledTile.AdjustRender();
            if (tileBase is Monocoord)
            {
                Monocoord monoTile = (Monocoord)decompiledTile;
                board[monoTile.GetPos().x, monoTile.GetPos().y, monoTile.GetPos().z] = decompiledTile;
            }
            else
            {
                Dicoord diTile = (Dicoord)decompiledTile;
                for (int x = diTile.GetPos1().x; x <= diTile.GetPos2().x; x++)
                {
                    for (int y = diTile.GetPos1().y; y <= diTile.GetPos2().y; y++)
                    {
                        for (int z = diTile.GetPos1().z; z <= diTile.GetPos2().z; z++)
                        {
                            board[x, y, z] = decompiledTile;
                        }
                    }
                }
            }
        }

        CameraManager.current.CalibrateCamera(board);
    }

    public void RemoveBoard()
    {
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int z = 0; z < board.GetLength(2); z++)
                {
                    if (board[x, y, z] != null)
                    {
                        board[x, y, z].EditorDeleteTileElement(board);
                    }
                }
            }
        }
    }

    public void GenerateMaterials ()
    {
        palette = new Color[]
        {
            new Color(worldData.reds[0], worldData.greens[0], worldData.blues[0]),
            new Color(worldData.reds[1], worldData.greens[1], worldData.blues[1]),
            new Color(worldData.reds[2], worldData.greens[2], worldData.blues[2]),
            new Color(worldData.reds[3], worldData.greens[3], worldData.blues[3]),
            new Color(worldData.reds[4], worldData.greens[4], worldData.blues[4]),
            new Color(worldData.reds[5], worldData.greens[5], worldData.blues[5]),
            new Color(worldData.reds[6], worldData.greens[6], worldData.blues[6]),
            new Color(worldData.reds[7], worldData.greens[7], worldData.blues[7]),
            new Color(worldData.reds[8], worldData.greens[8], worldData.blues[8]),
            new Color(worldData.reds[9], worldData.greens[9], worldData.blues[9]),
            new Color(worldData.reds[10], worldData.greens[10], worldData.blues[10])
        };

        unlitBases = new Material[11];
        litBases = new Material[11];
        unlitDarks = new Material[11];
        litDarks = new Material[11];
        unlitLights = new Material[11];
        litLights = new Material[11];
        for (int i = 0; i < 11; i++)
        {
            unlitBases[i] = new Material(Resources.Load<Material>("Materials/BaseMat"));
            unlitBases[i].SetColor("_BaseColor", palette[i]);
            litBases[i] = new Material(Resources.Load<Material>("Materials/TwotoneMat"));
            litBases[i].SetColor("_TopColor", palette[i]);
            litBases[i].SetColor("_FrontColor", Color.Lerp(palette[i], palette[0], 0.2f));
            litBases[i].SetColor("_SideColor", Color.Lerp(palette[i], palette[0], 0.4f));
            unlitDarks[i] = new Material(Resources.Load<Material>("Materials/BaseMat"));
            unlitDarks[i].SetColor("_BaseColor", Color.Lerp(palette[i], palette[0], 0.5f));
            litDarks[i] = new Material(Resources.Load<Material>("Materials/TwotoneMat"));
            litDarks[i].SetColor("_TopColor", Color.Lerp(palette[i], palette[0], 0.3f));
            litDarks[i].SetColor("_FrontColor", Color.Lerp(palette[i], palette[0], 0.5f));
            litDarks[i].SetColor("_SideColor", Color.Lerp(palette[i], palette[0], 0.7f));
            unlitLights[i] = new Material(Resources.Load<Material>("Materials/BaseMat"));
            unlitLights[i].SetColor("_BaseColor", Color.Lerp(palette[i], Color.white, 0.2f));
            litLights[i] = new Material(Resources.Load<Material>("Materials/TwotoneMat"));
            litLights[i].SetColor("_TopColor", Color.Lerp(palette[i], Color.white, 0.4f));
            litLights[i].SetColor("_FrontColor", Color.Lerp(palette[i], Color.white, 0.2f));
            litLights[i].SetColor("_SideColor", palette[i]);
        }

        darkener = new Material(Resources.Load<Material>("Materials/DarkenMat"));
        darkener.SetColor("_BlendColor", palette[0]);

        voidGradient = new Material(Resources.Load<Material>("Materials/VoidGradientMat"));
        voidGradient.SetColor("_GradientColor", palette[0]);
    }

    public void GenerateVoidGradient()
    {
        GameObject voidPlane = Resources.Load<GameObject>("Models/VoidEdge");
        GameObject edges = GameObject.Find("Edges");

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int z = 0; z < board.GetLength(2); z++)
            {
                if (levelData.grounds[x, 0, z] != null)
                {
                    GameObject northEdge = Instantiate(voidPlane, new Vector3(x, -5, z + 0.5f), Quaternion.identity, edges.transform);
                    northEdge.transform.eulerAngles = new Vector3(90, 0, 0);
                    northEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                        litBases[(int)(levelData.grounds[x, 0, z][2])],
                        voidGradient
                    };
                    GameObject eastEdge = Instantiate(voidPlane, new Vector3(x - 0.5f, -5, z), Quaternion.identity, edges.transform);
                    eastEdge.transform.eulerAngles = new Vector3(90, 0, 90);
                    eastEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                        litBases[(int)(levelData.grounds[x, 0, z][5])],
                        voidGradient
                    };
                    GameObject southEdge = Instantiate(voidPlane, new Vector3(x, -5, z - 0.5f), Quaternion.identity, edges.transform);
                    southEdge.transform.eulerAngles = new Vector3(90, 0, 180);
                    southEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                        litBases[(int)(levelData.grounds[x, 0, z][3])],
                        voidGradient
                    };
                    GameObject westEdge = Instantiate(voidPlane, new Vector3(x + 0.5f, -5, z), Quaternion.identity, edges.transform);
                    westEdge.transform.eulerAngles = new Vector3(90, 0, 270);
                    westEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                        litBases[(int)(levelData.grounds[x, 0, z][4])],
                        voidGradient
                    };
                }
            }
        }
    }

    public void DestroyVoidGradient ()
    {
        GameObject edges = GameObject.Find("Edges");
        for (int i = 0; i < edges.transform.childCount; i++)
        {
            Destroy(edges.transform.GetChild(i).gameObject);
        }
    }
}