using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager current;

    private GameObject camAnchor;
    private Camera cam;

    void Start()
    {
        current = this;

        camAnchor = GameObject.Find("CameraAnchor");
        cam = GameObject.Find("CameraAnchor/Camera").GetComponent<Camera>();

        StartCoroutine(CursorEvents());
        StartCoroutine(RotateBoard());
    }

    private IEnumerator CursorEvents()
    {
        while (true)
        {
            RaycastHit hitGround;
            Ray rayGround = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayGround, out hitGround, 100, ~8))
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
                else
                {
                    Hover(hitGround);
                }
            }

            RaycastHit hitBoth;
            Ray rayBoth = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayBoth, out hitBoth, 100, ~(8|9)))
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

            camAnchor.transform.position = new Vector3(17 * -Mathf.Sin(pos), 20, 17 * -Mathf.Cos(pos));
            camAnchor.transform.eulerAngles = new Vector3(45, pos * 180 / Mathf.PI, 0);

            velocity *= 0.95f;

            yield return null;
        }
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