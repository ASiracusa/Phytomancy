using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager current;

    private GameObject levelAnchor;
    private GameObject camAnchor;
    private Camera cam;

    void Start()
    {
        current = this;

        levelAnchor = GameObject.Find("LevelAnchor");
        camAnchor = GameObject.Find("LevelAnchor/CameraAnchor");
        cam = GameObject.Find("LevelAnchor/CameraAnchor/Camera").GetComponent<Camera>();

        StartCoroutine(CursorEvents());
        StartCoroutine(RotateBoard());
    }

    private IEnumerator CursorEvents()
    {
        while (true)
        {
            RaycastHit hitGround;
            Ray rayGround = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayGround, out hitGround, 100, LayerMask.GetMask("Ground")))
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    Click(Input.GetMouseButtonDown(0), hitGround);
                }
                else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    Release(Input.GetMouseButtonUp(0), hitGround);
                }
                else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    Hold(Input.GetMouseButton(0), hitGround);
                }
            }
            Hover(hitGround);

            RaycastHit hitBoth;
            Ray rayBoth = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayBoth, out hitBoth, 100, LayerMask.GetMask("Ground", "NotGround")))
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    ClickBoth(Input.GetMouseButtonDown(0), hitBoth);
                }
                else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    ReleaseBoth(Input.GetMouseButtonUp(0), hitBoth);
                }
                else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    HoldBoth(Input.GetMouseButton(0), hitBoth);
                }
                else
                {
                    HoverBoth(hitBoth);
                }
            }

            yield return null;
        }
    }

    private IEnumerator RotateBoard()
    {
        float pos = 0;
        float velocity = 0;
        float zoom = 14;

        while (true)
        {
            if (Input.GetKey(KeyCode.Q) && velocity < 1f)
            {
                velocity += Time.deltaTime / 5;
            }
            if (Input.GetKey(KeyCode.E) && velocity > -1f)
            {
                velocity -= Time.deltaTime / 5;
            }

            if (Input.GetMouseButton(2))
            {
                velocity = Input.GetAxis("Mouse X") / 15f;
            }

            pos += velocity;

            camAnchor.transform.localPosition = new Vector3(17 * -Mathf.Sin(pos), 20, 17 * -Mathf.Cos(pos));
            camAnchor.transform.eulerAngles = new Vector3(45, pos * 180 / Mathf.PI, 0);

            velocity *= 0.95f;
            
            zoom = Mathf.Clamp(zoom - Input.mouseScrollDelta.y, 2, 14);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom, 0.1f);

            yield return null;
        }
    }

    // Returns the coords of the tile next to a selected block depending on which facet was clicked.
    public static Vector3Int GetAdjacentCoords(RaycastHit hit, bool additive)
    {
        int sideModifier = additive ? 1 : 0;

        if (hit.transform.parent.GetComponent<ModelTileBridge>().Data is Monocoord)
        {
            float xDist = hit.point.x - hit.transform.parent.position.x;
            float yDist = hit.point.y - hit.transform.parent.position.y;
            float zDist = hit.point.z - hit.transform.parent.position.z;

            if (Mathf.Abs(xDist) > Mathf.Abs(yDist) && Mathf.Abs(xDist) > Mathf.Abs(zDist))
            {
                return new Vector3Int((int)(hit.transform.parent.position.x + (xDist > 0 ? 1 : -1) * sideModifier), (int)(hit.transform.parent.position.y), (int)(hit.transform.parent.position.z));
            }
            else if (Mathf.Abs(yDist) > Mathf.Abs(xDist) && Mathf.Abs(yDist) > Mathf.Abs(zDist))
            {
                return new Vector3Int((int)(hit.transform.parent.position.x), (int)(hit.transform.parent.position.y + (yDist > 0 ? 1 : -1) * sideModifier), (int)(hit.transform.parent.position.z));
            }
            else
            {
                return new Vector3Int((int)(hit.transform.parent.position.x), (int)(hit.transform.parent.position.y), (int)(hit.transform.parent.position.z + (zDist > 0 ? 1 : -1) * sideModifier));
            }
        }
        else
        {
            Dicoord dicoord = (Dicoord)hit.transform.parent.GetComponent<ModelTileBridge>().Data;
            float upDist = Mathf.Abs(hit.point.y - 0.5f - dicoord.GetPos2().y);
            float downDist = Mathf.Abs(hit.point.y + 0.5f - dicoord.GetPos1().y);
            float westDist = Mathf.Abs(hit.point.z - 0.5f - dicoord.GetPos2().z);
            float eastDist = Mathf.Abs(hit.point.z + 0.5f - dicoord.GetPos1().z);
            float northDist = Mathf.Abs(hit.point.x - 0.5f - dicoord.GetPos2().x);
            float southDist = Mathf.Abs(hit.point.x + 0.5f - dicoord.GetPos1().x);

            int xRounded = Mathf.RoundToInt(hit.point.x);
            int yRounded = Mathf.RoundToInt(hit.point.y);
            int zRounded = Mathf.RoundToInt(hit.point.z);

            if (upDist < downDist && upDist < westDist && upDist < eastDist && upDist < northDist && upDist < southDist)
            {
                return new Vector3Int(xRounded, dicoord.GetPos2().y + sideModifier, zRounded);
            }
            else if (downDist < upDist && downDist < westDist && downDist < eastDist && downDist < northDist && downDist < southDist)
            {
                return new Vector3Int(xRounded, dicoord.GetPos1().y - sideModifier, zRounded);
            }
            else if (westDist < upDist && westDist < downDist && westDist < eastDist && westDist < northDist && westDist < southDist)
            {
                return new Vector3Int(dicoord.GetPos2().y + sideModifier, yRounded, zRounded);
            }
            else if (eastDist < upDist && eastDist < downDist && eastDist < westDist && eastDist < northDist && eastDist < southDist)
            {
                return new Vector3Int(dicoord.GetPos1().y - sideModifier, yRounded, zRounded);
            }
            else if (northDist < upDist && northDist < downDist && northDist < westDist && northDist < eastDist && northDist < southDist)
            {
                return new Vector3Int(xRounded, yRounded, dicoord.GetPos2().z + sideModifier);
            }
            else
            {
                return new Vector3Int(xRounded, yRounded, dicoord.GetPos1().z - sideModifier);
            }
        }
    }

    public void CalibrateCamera (TileElement[,,] board)
    {
        levelAnchor.transform.localPosition = new Vector3(board.GetLength(0) / 2.0f, 0, board.GetLength(2) / 2.0f);
    }

    public Facet GetCameraOrientation ()
    {
        float y = camAnchor.transform.localEulerAngles.y;
        if (45 < y && y < 135) { return Facet.North; }
        if (135 < y && y < 225) { return Facet.East; }
        if (225 < y && y < 315) { return Facet.South; }
        return Facet.West;
    }

    public event Action<RaycastHit> onHover;
    public void Hover(RaycastHit hit)
    {
        onHover?.Invoke(hit);
    }

    public event Action<bool, RaycastHit> onClick;
    public void Click(bool left, RaycastHit hit)
    {
        onClick?.Invoke(left, hit);
    }

    public event Action<bool, RaycastHit> onHold;
    public void Hold(bool left, RaycastHit hit)
    {
        onHold?.Invoke(left, hit);
    }

    public event Action<bool, RaycastHit> onRelease;
    public void Release(bool left, RaycastHit hit)
    {
        onRelease?.Invoke(left, hit);
    }

    public event Action<RaycastHit> onHoverBoth;
    public void HoverBoth(RaycastHit hit)
    {
        onHoverBoth?.Invoke(hit);
    }

    public event Action<bool, RaycastHit> onClickBoth;
    public void ClickBoth(bool left, RaycastHit hit)
    {
        onClickBoth?.Invoke(left, hit);
    }

    public event Action<bool, RaycastHit> onHoldBoth;
    public void HoldBoth(bool left, RaycastHit hit)
    {
        onHoldBoth?.Invoke(left, hit);
    }

    public event Action<bool, RaycastHit> onReleaseBoth;
    public void ReleaseBoth(bool left, RaycastHit hit)
    {
        onReleaseBoth?.Invoke(left, hit);
    }
}