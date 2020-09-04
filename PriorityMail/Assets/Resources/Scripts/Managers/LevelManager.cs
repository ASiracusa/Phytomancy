using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager current;
    
    private string levelPath;
    
    private Stack<Stack<BoardStateChange>> undoData;

    public LinkedList<TileAnimationMovement> movementAnims;
    public LinkedList<TileAnimationFall> fallAnims;

    private Coroutine brambleInput;
    private RaycastHit lastHit;
    private Transform tracedVine;

    private GameObject edges;
    public Material voidGradient;

    // Start is called before the first frame update
    void Start()
    {
        current = this;

        /*
        availableVines = new int[10] {
            5, 0, 0, 0, 7, 0, 0, 0, 0, 3
        };
        */

        edges = GameObject.Find("Edges");
    }

    private void LoadLevel(string levelPath)
    {
        undoData = new Stack<Stack<BoardStateChange>>();
        movementAnims = new LinkedList<TileAnimationMovement>();
        fallAnims = new LinkedList<TileAnimationFall>();

        WorldManager.current.LoadLevel(levelPath, true);
    }

    private IEnumerator BrambleInput()
    {
        while (true)
        {
            if (movementAnims.Count == 0 && fallAnims.Count == 0)
            {
                Facet camDirection = CameraManager.current.GetCameraOrientation();

                Bramble bramble = WorldManager.current.bramble;
                TileElement[,,] board = WorldManager.current.board;

                if (bramble != null && bramble.model != null)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.North + (int)camDirection) % 4), null);
                        if (bramble.model != null)
                        {
                            bramble.model.transform.localEulerAngles = new Vector3(0, 90 - 90 * (int)camDirection, 0);
                        }
                        ClearSpaciousTiles();
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.South + (int)camDirection) % 4), null);
                        if (bramble.model != null)
                        {
                            bramble.model.transform.localEulerAngles = new Vector3(0, 270 - 90 * (int)camDirection, 0);
                        }
                        ClearSpaciousTiles();
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.West + (int)camDirection) % 4), null);
                        if (bramble.model != null)
                        {
                            bramble.model.transform.localEulerAngles = new Vector3(0, 0 - 90 * (int)camDirection, 0);
                        }
                        ClearSpaciousTiles();
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        undoData.Push(new Stack<BoardStateChange>());
                        bramble.InitiatePush(board, (Facet)(((int)Facet.East + (int)camDirection) % 4), null);
                        if (bramble.model != null)
                        {
                            bramble.model.transform.localEulerAngles = new Vector3(0, 180 - 90 * (int)camDirection, 0);
                        }
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
        if (hit.distance == 0 || !((hit.transform == tracedVine) || (hit.transform == tracedVine.parent)))
        {
            float deltaX = Input.mousePosition.x - CameraManager.current.cam.WorldToScreenPoint(tracedVine.position).x;
            float deltaY = Input.mousePosition.y - CameraManager.current.cam.WorldToScreenPoint(tracedVine.position).y;
            float theta = Mathf.Atan(deltaY / deltaX) * Mathf.Rad2Deg + (deltaX < 0 ? 180 : (deltaY < 0 ? 360 : 0));
            Vector3Int givenDirection = Vector3Int.zero;

            if (Mathf.Abs(Mathf.DeltaAngle(theta, 90)) < 20)
            {
                givenDirection = new Vector3Int(0, 1, 0);
            }
            else if (Mathf.Abs(Mathf.DeltaAngle(theta, 270 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
            {
                givenDirection = new Vector3Int(0, 0, -1);
            }
            else if (Mathf.Abs(Mathf.DeltaAngle(theta, 0 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
            {
                givenDirection = new Vector3Int(1, 0, 0);
            }
            else if (Mathf.Abs(Mathf.DeltaAngle(theta, 90 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
            {
                givenDirection = new Vector3Int(0, 0, 1);
            }
            else if (Mathf.Abs(Mathf.DeltaAngle(theta, 180 + CameraManager.current.cam.transform.parent.localEulerAngles.y)) < 45)
            {
                givenDirection = new Vector3Int(-1, 0, 0);
            }

            CreateVine(givenDirection);
        }
    }



    private void CreateVine(bool left, RaycastHit hit, Vector3Int? givenDirection = null)
    {
        if (hit.transform.gameObject.GetComponent<ColoredMeshBridge>() != null && hit.transform.gameObject.layer == 8)
        {
            TileElement[,,] board = WorldManager.current.board;

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
                int vinesOfColor = WorldManager.current.availableVines[(int)vineColor - 1];
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
                    board[vineCoords.x, vineCoords.y, vineCoords.z].model.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = WorldManager.current.palette[(int)vine.GetColor()];
                    board[vineCoords.x, vineCoords.y, vineCoords.z].BindDataToModel();
                    board[vineCoords.x, vineCoords.y, vineCoords.z].AdjustRender();
                    board[vineCoords.x, vineCoords.y, vineCoords.z].WarpToPos();

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
        TileElement[,,] board = WorldManager.current.board;

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
        int vinesOfColor = WorldManager.current.availableVines[(int)vineColor - 1];
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
            board[vineCoords.x, vineCoords.y, vineCoords.z].model.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = WorldManager.current.palette[(int)vine.GetColor()];
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
        TileElement[,,] board = WorldManager.current.board;

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
        GameObject avIconResource = Resources.Load<GameObject>("Prefabs/AVIcon2");

        for (int i = 0; i < WorldManager.current.availableVines.Length; i++)
        {
            GameObject avIcon = Instantiate<GameObject>(avIconResource, Vector3.zero, Quaternion.identity);
            avIcon.transform.SetParent(avBase.transform);
            avIcon.transform.GetChild(0).GetComponent<Image>().color = WorldManager.current.palette[i + 1];
        }

        int total = 0;
        for (int i = 0; i < 10; i++)
        {
            if (WorldManager.current.availableVines[i] > 0)
            {
                total++;
            }
        }

        int pos = 0;
        for (int i = 0; i < 10; i++)
        {
            GameObject avIcon = avBase.transform.GetChild(i).gameObject;
            if (WorldManager.current.availableVines[i] > 0)
            {
                avIcon.transform.localPosition = new Vector3((total - 1) * -20 + pos * 40, 50, 0);
                avIcon.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                pos++;
            }
            else
            {
                avIcon.transform.localPosition = new Vector3((total - 1) * -20 + pos * 40 - 20, 50, 0);
                avIcon.transform.localScale = new Vector3(0, 0, 1);
            }
        }

        StartCoroutine(ControlAVUI());
    }

    public void AdjustAvailableVinesUI(Shade color, int amount)
    {
        WorldManager.current.availableVines[(int)color - 1] += amount;
    }

    private IEnumerator ControlAVUI ()
    {
        GameObject avBase = GameObject.Find("PlayerCanvas/AvailableVinesMenu/AVAnchor");
        while (true)
        {
            int total = 0;
            for (int i = 0; i < 10; i++)
            {
                if (WorldManager.current.availableVines[i] > 0)
                {
                    total++;
                }
            }

            int pos = 0;
            for (int i = 0; i < avBase.transform.childCount; i++)
            {
                GameObject avIcon = avBase.transform.GetChild(i).gameObject;
                if (WorldManager.current.availableVines[i] > 0)
                {
                    avIcon.transform.localPosition = Vector3.Lerp(avIcon.transform.localPosition, new Vector3((total - 1) * -20 + pos * 40, 0, 0), 0.25f);
                    avIcon.transform.localScale = Vector3.Lerp(avIcon.transform.localScale, new Vector3(1.25f, 1.25f, 1f), 0.25f);
                    avIcon.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "" + WorldManager.current.availableVines[i];
                    pos++;
                }
                else
                {
                    avIcon.transform.localPosition = Vector3.Lerp(avIcon.transform.localPosition, new Vector3((total - 1) * -20 + pos * 40 - 20, 0, 0), 0.25f);
                    avIcon.transform.localScale = avIcon.transform.localScale.x < 0.001f ? new Vector3(0, 0, 1) : Vector3.Lerp(avIcon.transform.localScale, new Vector3(0, 0, 1), 0.25f);
                    avIcon.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "";
                }
            }

            yield return null;
        }
    }

    public void UndoTurn ()
    {
        if (undoData.Count > 0)
        {
            Stack<BoardStateChange> undos = undoData.Pop();
            while (undos.Count > 0)
            {
                undos.Pop().Revert(WorldManager.current.board);
            }
        }
    }

    public void AddUndoData (BoardStateChange stateChange)
    {
        undoData.Peek().Push(stateChange);
    }

    public void RemoveBoard ()
    {
        WorldManager.current.RemoveBoard();
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

        WorldManager.current.GenerateMaterials();

        voidGradient = new Material(Resources.Load<Material>("Materials/VoidGradientMat"));
        voidGradient.SetColor("_GradientColor", WorldManager.current.palette[0]);

        LoadLevel(levelPath);
        
        CameraManager.current.onClick += MainVineControlHelper;
        CameraManager.current.onClick += PreTraceVine;
        CameraManager.current.onReleaseAny += delegate { CameraManager.current.onHover -= TraceVine; };

        brambleInput = StartCoroutine(BrambleInput());
        GenerateAvailableVinesUI();

        GameObject.Find("LevelAnchor/CameraAnchor/Camera").GetComponent<Camera>().backgroundColor = WorldManager.current.palette[0];
        CameraManager.current.CalibrateCamera(WorldManager.current.board);
    }

    public void DeleteAVUI (GameObject avui)
    {
        StopCoroutine("ControlAVUI");

        for (int i = 0; i < avui.transform.childCount; i++)
        {
            Destroy(avui.transform.GetChild(i).gameObject);
        }
    }

    public void LeaveLevel()
    {
        RemoveBoard();
        Destroy(WorldManager.current.bramble.model);
        
        CameraManager.current.onClick -= MainVineControlHelper;
        CameraManager.current.onClick -= PreTraceVine;
        CameraManager.current.onReleaseAny -= delegate { CameraManager.current.onHover -= TraceVine; };

        StopCoroutine(brambleInput);

        GameObject avBase = GameObject.Find("PlayerCanvas/AvailableVinesMenu/AVAnchor");
        DeleteAVUI(avBase);

        avBase.transform.localPosition = new Vector3(-15f, 0, 0);

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