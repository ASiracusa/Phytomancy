using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager current;

    private LevelData levelData;
    private string levelPath;

    private TileElement[,,] board;
    private Stack<Stack<BoardStateChange>> undoData;
    private Bramble bramble;
    private int[] availableVines;

    public LinkedList<TileAnimationMovement> movementAnims;
    public LinkedList<TileAnimationFall> fallAnims;

    private Coroutine brambleInput;
    private RaycastHit lastHit;
    private Transform tracedVine;

    public Color[] palette = new Color[]
    {
        Color.black,
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green,
        Color.magenta,
        Color.cyan,
        Color.white,
        Color.gray,
        Color.black,
        Color.magenta,
    };
    public Material[] materials;
    public Material darkener;

    private GameObject edges;
    public Material voidGradient;

    // Start is called before the first frame update
    void Start()
    {
        current = this;

        availableVines = new int[10] {
            5, 0, 0, 0, 7, 0, 0, 0, 0, 3
        };

        edges = GameObject.Find("Edges");
    }

    private void LoadLevel(string levelPath)
    {
        // Load LevelData and initialize the lists
        levelData = (LevelData)SerializationManager.LoadData(levelPath);
        TileElement tileModel = Constants.TILE_MODELS[(int)TileElementNames.Ground];
        undoData = new Stack<Stack<BoardStateChange>>();
        movementAnims = new LinkedList<TileAnimationMovement>();
        fallAnims = new LinkedList<TileAnimationFall>();

        availableVines = levelData.availableVines;

        GameObject voidPlane = Resources.Load<GameObject>("Models/VoidEdge");

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
                        ((Ground)board[x, y, z]).ColorFacets(materials);

                        if (y == 0)
                        {
                            GameObject northEdge = Instantiate(voidPlane, new Vector3(x, -3, z + 0.5f), Quaternion.identity, edges.transform);
                            northEdge.transform.eulerAngles = new Vector3(90, 0, 0);
                            northEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                                materials[(int)(levelData.grounds[x, y, z][2])],
                                voidGradient
                            };
                            GameObject eastEdge = Instantiate(voidPlane, new Vector3(x - 0.5f, -3, z), Quaternion.identity, edges.transform);
                            eastEdge.transform.eulerAngles = new Vector3(90, 0, 90);
                            eastEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                                materials[(int)(levelData.grounds[x, y, z][5])],
                                voidGradient
                            };
                            GameObject southEdge = Instantiate(voidPlane, new Vector3(x, -3, z - 0.5f), Quaternion.identity, edges.transform);
                            southEdge.transform.eulerAngles = new Vector3(90, 0, 180);
                            southEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                                materials[(int)(levelData.grounds[x, y, z][3])],
                                voidGradient
                            };
                            GameObject westEdge = Instantiate(voidPlane, new Vector3(x + 0.5f, -3, z), Quaternion.identity, edges.transform);
                            westEdge.transform.eulerAngles = new Vector3(90, 0, 270);
                            westEdge.GetComponent<MeshRenderer>().materials = new Material[] {
                                materials[(int)(levelData.grounds[x, y, z][4])],
                                voidGradient
                            };
                        }
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
    }

    private IEnumerator BrambleInput()
    {
        while (true)
        {
            if (movementAnims.Count == 0 && fallAnims.Count == 0)
            {
                Facet camDirection = CameraManager.current.GetCameraOrientation();

                if (bramble.model != null)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.North + (int)camDirection) % 4), null);
                        bramble.model.transform.localEulerAngles = new Vector3(0, 90 - 90 * (int)camDirection, 0);
                        ClearSpaciousTiles();
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.South + (int)camDirection) % 4), null);
                        bramble.model.transform.localEulerAngles = new Vector3(0, 270 - 90 * (int)camDirection, 0);
                        ClearSpaciousTiles();
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.West + (int)camDirection) % 4), null);
                        bramble.model.transform.localEulerAngles = new Vector3(0, 0 - 90 * (int)camDirection, 0);
                        ClearSpaciousTiles();
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.East + (int)camDirection) % 4), null);
                        bramble.model.transform.localEulerAngles = new Vector3(0, 180 - 90 * (int)camDirection, 0);
                        ClearSpaciousTiles();
                    }
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    UndoTurn();
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RemoveBoard();
                    LoadLevel(levelPath);

                    GameObject avBase = GameObject.Find("PlayerCanvas/AvailableVinesMenu/AVAnchor");
                    DeleteAVUI(avBase.transform.GetChild(0).gameObject);
                    avBase.transform.localPosition = new Vector3(-37.5f, 0, 0);

                    GenerateAvailableVinesUI();
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    LeaveLevel();
                }
                CameraManager.current.GetCameraOrientation();

            }
            yield return null;
        }
    }



    private void MainVineControlHelper (bool left, RaycastHit hit)
    {
        StartCoroutine(MainVineControl(left, hit));
    }

    private IEnumerator MainVineControl(bool left, RaycastHit hit)
    {
        if (left) {
            float t = Time.time;
            while (Input.GetMouseButton(0))
            {
                yield return null;
            }
            if (Time.time - t < 0.2f)
            {
                CreateVine(left, hit);
            }
        }
        else
        {
            CreateVine(left, hit);
        }
    }

    private void PreTraceVine(bool left, RaycastHit hit)
    {
        if (left)
        {
            lastHit = hit;
            tracedVine = hit.transform;
            CameraManager.current.onHover += TraceVine;
        }
    }

    private void TraceVine(RaycastHit hit)
    {
        float deltaX = Input.mousePosition.x - CameraManager.current.cam.WorldToScreenPoint(tracedVine.position).x;
        float deltaY = Input.mousePosition.y - CameraManager.current.cam.WorldToScreenPoint(tracedVine.position).y;
        float theta = Mathf.Atan(deltaY / deltaX) * Mathf.Rad2Deg + (deltaX < 0 ? 180 : (deltaY < 0 ? 360 : 0));
        Vector3Int givenDirection = Vector3Int.zero;

        if (Mathf.Abs(Mathf.DeltaAngle(theta, 90)) < 20)
        {
            Debug.Log("UP");
            givenDirection = new Vector3Int(0, 1, 0);
        }
        else if (Mathf.Abs(Mathf.DeltaAngle(theta, 270 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
        {
            Debug.Log("EAST");
            givenDirection = new Vector3Int(0, 0, -1);
        }
        else if (Mathf.Abs(Mathf.DeltaAngle(theta, 0 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
        {
            Debug.Log("NORTH");
            givenDirection = new Vector3Int(1, 0, 0);
        }
        else if (Mathf.Abs(Mathf.DeltaAngle(theta, 90 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
        {
            Debug.Log("WEST");
            givenDirection = new Vector3Int(0, 0, 1);
        }
        else if (Mathf.Abs(Mathf.DeltaAngle(theta, 180 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
        {
            Debug.Log("SOUTH");
            givenDirection = new Vector3Int(-1, 0, 0);
        }

        if (hit.distance == 0 || !((hit.transform == tracedVine) || (hit.transform == tracedVine.parent)))
        {
            Debug.Log("It be different!");
            Debug.Log(givenDirection);
            CreateVine(givenDirection);
        }
    }



    private void CreateVine(bool left, RaycastHit hit, Vector3Int? givenDirection = null)
    {
        if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>() != null && hit.transform.gameObject.layer == 8)
        {
            if (left)
            {
                // Get the color of the new Vine based on where the player clicked
                Shade vineColor;
                if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data is Ground)
                {
                    vineColor = ((Ground)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).GetShades()[hit.transform.gameObject.GetComponent<ColoredMeshBridge>().index];
                }
                else
                {
                    vineColor = ((Vine)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).GetColor();
                }
                int vinesOfColor = availableVines[(int)vineColor - 1];
                Vector3Int stemCoords = CameraManager.GetAdjacentCoords(hit, false);

                if (vinesOfColor > 0 && (!(board[stemCoords.x, stemCoords.y, stemCoords.z] is Vine) || ((Vine)board[stemCoords.x, stemCoords.y, stemCoords.z]).GetVine() == null))
                {
                    Vector3Int vineCoords = (givenDirection == null) ? CameraManager.GetAdjacentCoords(hit, true) : stemCoords + (Vector3Int)givenDirection;
                    if (vineCoords.x < 0 || vineCoords.y < 0 || vineCoords.z < 0 ||
                    vineCoords.x >= board.GetLength(0) || vineCoords.y >= board.GetLength(1) || vineCoords.z >= board.GetLength(2))
                    {
                        return;
                    }

                    TileElement tileAtPos = board[vineCoords.x, vineCoords.y, vineCoords.z];

                    Vector3Int direction = (givenDirection == null) ? vineCoords - ((Monocoord)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).GetPos() : (Vector3Int)givenDirection;

                    Vine vine = (Vine)Constants.TILE_MODELS[(int)TileElementNames.Vine].GenerateTileElement(new object[] {
                        vineCoords,
                        vineColor,
                        Constants.VectorToFacet(-direction)
                    });

                    undoData.Push(new Stack<BoardStateChange>());
                    if (tileAtPos != null && tileAtPos.Pushable && !tileAtPos.Weedblocked && !(tileAtPos is IMonoSpacious))
                    {
                        if (!board[vineCoords.x, vineCoords.y, vineCoords.z].InitiatePush(board, Constants.VectorToFacet(direction), vine))
                        {
                            undoData.Pop();
                            return;
                        }
                    }
                    else if (tileAtPos == null)
                    {
                        board[vineCoords.x, vineCoords.y, vineCoords.z] = vine;
                    }
                    else
                    {
                        return;
                    }

                    board[vineCoords.x, vineCoords.y, vineCoords.z].model = Instantiate(Resources.Load("Models/Vine")) as GameObject;
                    board[vineCoords.x, vineCoords.y, vineCoords.z].model.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = palette[(int)vine.GetColor()];
                    board[vineCoords.x, vineCoords.y, vineCoords.z].BindDataToModel();
                    board[vineCoords.x, vineCoords.y, vineCoords.z].AdjustRender();
                    board[vineCoords.x, vineCoords.y, vineCoords.z].WarpToPos();
                    tracedVine = board[vineCoords.x, vineCoords.y, vineCoords.z].model.transform;

                    AddUndoData(new BoardCreationState(vine));

                    if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data is Vine)
                    {
                        ((Vine)(board[stemCoords.x, stemCoords.y, stemCoords.z])).SetVine((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]);
                    }

                    AdjustAvailableVinesUI(vineColor, -1);
                }
            }
            else if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data is Vine)
            {
                Vector3Int vineCoords = CameraManager.GetAdjacentCoords(hit, false);
                Vector3Int stemCoords = ((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]).GetPos() + Constants.FacetToVector(((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]).GetOrigin());
                Shade vineColor = ((Vine)(hit.transform.gameObject.GetComponent<ColoredMeshBridge>().data)).GetColor();

                undoData.Push(new Stack<BoardStateChange>());
                AdjustAvailableVinesUI(vineColor, ((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]).RemoveVine(board));
                if (board[stemCoords.x, stemCoords.y, stemCoords.z] is Vine)
                {
                    ((Vine)board[stemCoords.x, stemCoords.y, stemCoords.z]).SetVine(null);
                }
                StartCoroutine(AnimateTileStateChange());
            }
        }
    }

    private void CreateVine (Vector3Int givenDirection)
    {
        // Get the color of the new Vine based on where the player clicked
        Shade vineColor;
        if (tracedVine.gameObject.GetComponent<ColoredMeshBridge>().data is Ground)
        {
            //vineColor = ((Ground)(tracedVine.gameObject.GetComponent<ColoredMeshBridge>().data)).GetShades()[tracedVine.gameObject.GetComponent<ColoredMeshBridge>().index];
            Debug.Log(Constants.VectorToFacet(givenDirection));
            vineColor = ((Ground)(tracedVine.transform.parent.GetComponent<ModelTileBridge>().Data)).GetShades()[(int)Constants.FacetToModel(Constants.VectorToFacet(givenDirection))];
        }
        else
        {
            vineColor = ((Vine)(tracedVine.gameObject.GetComponent<ColoredMeshBridge>().data)).GetColor();
        }
        int vinesOfColor = availableVines[(int)vineColor - 1];
        Vector3Int stemCoords = ((Monocoord)tracedVine.GetComponent<ColoredMeshBridge>().data).GetPos(); 

        if (vinesOfColor > 0 && (!(board[stemCoords.x, stemCoords.y, stemCoords.z] is Vine) || ((Vine)board[stemCoords.x, stemCoords.y, stemCoords.z]).GetVine() == null))
        {
            Vector3Int vineCoords =stemCoords + givenDirection;
            if (vineCoords.x < 0 || vineCoords.y < 0 || vineCoords.z < 0 ||
            vineCoords.x >= board.GetLength(0) || vineCoords.y >= board.GetLength(1) || vineCoords.z >= board.GetLength(2))
            {
                return;
            }

            TileElement tileAtPos = board[vineCoords.x, vineCoords.y, vineCoords.z];

            Vine vine = (Vine)Constants.TILE_MODELS[(int)TileElementNames.Vine].GenerateTileElement(new object[] {
                        vineCoords,
                        vineColor,
                        Constants.VectorToFacet(-givenDirection)
                    });

            undoData.Push(new Stack<BoardStateChange>());
            if (tileAtPos != null && tileAtPos.Pushable && !tileAtPos.Weedblocked && !(tileAtPos is IMonoSpacious))
            {
                if (!board[vineCoords.x, vineCoords.y, vineCoords.z].InitiatePush(board, Constants.VectorToFacet(givenDirection), vine))
                {
                    undoData.Pop();
                    return;
                }
            }
            else if (tileAtPos == null)
            {
                board[vineCoords.x, vineCoords.y, vineCoords.z] = vine;
            }
            else
            {
                return;
            }

            board[vineCoords.x, vineCoords.y, vineCoords.z].model = Instantiate(Resources.Load("Models/Vine")) as GameObject;
            board[vineCoords.x, vineCoords.y, vineCoords.z].model.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = palette[(int)vine.GetColor()];
            board[vineCoords.x, vineCoords.y, vineCoords.z].BindDataToModel();
            board[vineCoords.x, vineCoords.y, vineCoords.z].AdjustRender();
            board[vineCoords.x, vineCoords.y, vineCoords.z].WarpToPos();

            if (tracedVine.gameObject.GetComponent<ColoredMeshBridge>().data is Vine)
            {
                ((Vine)(board[stemCoords.x, stemCoords.y, stemCoords.z])).SetVine((Vine)board[vineCoords.x, vineCoords.y, vineCoords.z]);
            }

            tracedVine = board[vineCoords.x, vineCoords.y, vineCoords.z].model.transform.GetChild(1);
            Debug.Log("Getting Vine Pos - " + tracedVine.gameObject.GetComponent<ColoredMeshBridge>());
            Debug.Log("Getting Vine Pos - " + tracedVine.gameObject.GetComponent<ColoredMeshBridge>().data);

            AddUndoData(new BoardCreationState(vine));

            AdjustAvailableVinesUI(vineColor, -1);
        }
    }

    private void ClearSpaciousTiles()
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

    private void GenerateAvailableVinesUI()
    {
        GameObject avBase = GameObject.Find("PlayerCanvas/AvailableVinesMenu/AVAnchor");
        GameObject avAnchor = avBase;
        GameObject avIconResource = Resources.Load<GameObject>("Prefabs/AVIcon");

        for (int i = 0; i < availableVines.Length; i++)
        {
            GameObject avIcon = Instantiate<GameObject>(avIconResource);
            avIcon.transform.SetParent(avAnchor.transform);
            if (availableVines[i] > 0)
            {
                avIcon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = availableVines[i].ToString();
                avIcon.transform.GetComponent<Image>().color = palette[i + 1];
                avIcon.transform.localPosition = new Vector3(30, 0, 0);
                avBase.transform.localPosition += new Vector3(-15, 0, 0);
            }
            else
            {
                avIcon.transform.GetComponent<Image>().color = Color.clear;
                avIcon.transform.localPosition = new Vector3(0, 0, 0);
            }
            avAnchor = avIcon;
        }
    }

    public void AdjustAvailableVinesUI(Shade color, int amount)
    {
        GameObject avBase = GameObject.Find("PlayerCanvas/AvailableVinesMenu/AVAnchor");
        GameObject avIcon = avBase.transform.GetChild(0).gameObject;
        for (int i = 0; i < (int)color - 1; i++)
        {
            avIcon = avIcon.transform.GetChild(1).gameObject;
        }

        if (availableVines[(int)color - 1] == 0)
        {
            availableVines[(int)color - 1] += amount;
            avIcon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = availableVines[(int)color - 1].ToString();
            avIcon.transform.GetComponent<Image>().color = palette[(int)color];
            avIcon.transform.localPosition = new Vector3(30, 0, 0);
            avBase.transform.localPosition += new Vector3(-15, 0, 0);
        }
        else if (amount > 0)
        {
            availableVines[(int)color - 1] += amount;
            avIcon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = availableVines[(int)color - 1].ToString();
        }
        else
        {
            availableVines[(int)color - 1] += amount;
            avIcon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = availableVines[(int)color - 1].ToString();
            if (availableVines[(int)color - 1] == 0)
            {
                avIcon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
                avIcon.transform.GetComponent<Image>().color = Color.clear;
                avIcon.transform.localPosition = new Vector3(0, 0, 0);
                avBase.transform.localPosition += new Vector3(15, 0, 0);
            }
        }
        
    }

    public void UndoTurn ()
    {
        if (undoData.Count > 0)
        {
            Stack<BoardStateChange> undos = undoData.Pop();
            while (undos.Count > 0)
            {
                undos.Pop().Revert(board);
            }
        }
    }

    public void AddUndoData (BoardStateChange stateChange)
    {
        undoData.Peek().Push(stateChange);
    }

    public void RemoveBoard ()
    {
        for(int x = 0; x < board.GetLength(0); x++)
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

    public IEnumerator AnimateTileStateChange()
    {
        if (movementAnims.Count > 0)
        {
            float startTime = Time.time;
            while (Time.time - startTime < 0.1f)
            {
                foreach (TileAnimation tileAnim in movementAnims)
                {
                    tileAnim.Animate();
                }
                yield return null;
            }

            foreach (TileAnimation tileAnim in movementAnims)
            {
                tileAnim.Complete();
            }
            movementAnims.Clear();
        }

        if (fallAnims.Count > 0)
        {
            foreach (TileAnimationFall tileAnim in fallAnims)
            {
                tileAnim.SetStartPos();
            }
            
            float startTime = Time.time;

            Debug.Log(fallAnims.Count);
            while (Time.time - startTime < 0.2f)
            {
                Debug.Log("it iterated");
                foreach (TileAnimation tileAnim in fallAnims)
                {
                    tileAnim.Animate();
                }
                yield return null;
            }

            foreach (TileAnimation tileAnim in fallAnims)
            {
                tileAnim.Complete();
            }
            fallAnims.Clear();
        }

        yield return null;
    }

    public void OpenLevel(WorldData worldData, string _levelPath)
    {
        levelPath = _levelPath;
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
        materials = new Material[11];
        for (int i = 0; i < 11; i++)
        {
            materials[i] = new Material(Resources.Load<Material>("Materials/TwotoneMat"));
            materials[i].SetColor("_TopColor", palette[i]);
            materials[i].SetColor("_FrontColor", Color.Lerp(palette[i], palette[0], 0.45f));
            materials[i].SetColor("_SideColor", Color.Lerp(palette[i], palette[0], 0.6f));
        }
        darkener = new Material(Resources.Load<Material>("Materials/DarkenMat"));
        darkener.SetColor("_BlendColor", palette[0]);
        voidGradient = new Material(Resources.Load<Material>("Materials/VoidGradientMat"));
        voidGradient.SetColor("_GradientColor", palette[0]);

        LoadLevel(levelPath);

        //CameraManager.current.onClick += CreateVine;
        CameraManager.current.onClick += MainVineControlHelper;
        CameraManager.current.onClick += PreTraceVine;
        CameraManager.current.onReleaseAny += delegate { CameraManager.current.onHover -= TraceVine; };

        brambleInput = StartCoroutine(BrambleInput());
        GenerateAvailableVinesUI();

        GameObject.Find("LevelAnchor/CameraAnchor/Camera").GetComponent<Camera>().backgroundColor = palette[0];
        CameraManager.current.CalibrateCamera(board);
    }

    public void DeleteAVUI (GameObject avui)
    {
        if (avui.transform.childCount != 1)
        {
            DeleteAVUI(avui.transform.GetChild(1).gameObject);
        }
        Destroy(avui.transform.GetChild(0).gameObject);
        Destroy(avui);
    }

    public void LeaveLevel()
    {
        RemoveBoard();
        Destroy(bramble.model);

        //CameraManager.current.onClick -= CreateVine;
        CameraManager.current.onClick -= MainVineControlHelper;
        CameraManager.current.onClick -= PreTraceVine;
        CameraManager.current.onReleaseAny -= delegate { CameraManager.current.onHover -= TraceVine; };

        StopCoroutine(brambleInput);

        GameObject avBase = GameObject.Find("PlayerCanvas/AvailableVinesMenu/AVAnchor");
        DeleteAVUI(avBase.transform.GetChild(0).gameObject);
        
        //Destroy(avBase.transform.GetChild(0));

        avBase.transform.localPosition = new Vector3(-37.5f, 0, 0);

        for (int i = 0; i < edges.transform.childCount; i++)
        {
            Destroy(edges.transform.GetChild(i).gameObject);
        }

        PlayerMenuManager.current.ReturnToLevelSelector();
    }

    public void WinLevel ()
    {
        LeaveLevel();
    }
}