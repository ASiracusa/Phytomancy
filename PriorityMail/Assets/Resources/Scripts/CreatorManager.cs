using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreatorManager : MonoBehaviour
{
    private GameObject cam;
    private GameObject[,,] board;

    void Start()
    {
        cam = GameObject.Find("Camera");
        board = new GameObject[20, 10, 20];

        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                GameObject bottom = Instantiate(Resources.Load("Prefabs/BoardElements/floor"), new Vector3(x - 9.5f, 0, z - 9.5f), Quaternion.identity) as GameObject;
                board[x, 0, z] = bottom;
            }
        }

        StartCoroutine(RotateBoard());
        StartCoroutine(EditBoard());
    }

    private IEnumerator RotateBoard ()
    {
        float pos = 0;

        while (true)
        {
            if (Input.GetKey(KeyCode.A))
            {
                pos += Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                pos -= Time.deltaTime;
            }

            cam.transform.position = new Vector3(17 * -Mathf.Sin(pos), 20, 17 * -Mathf.Cos(pos));
            cam.transform.eulerAngles = new Vector3(45, pos * 180 / Mathf.PI, 0);

            yield return null;
        }
    }

    private IEnumerator EditBoard ()
    {
        Vector3Int selectedCorner = Vector3Int.zero;

        while (true)
        {
            // Runs whenever the editor clicks on a tile
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100, ~0))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        selectedCorner = GetAdjacentCoords(hit);
                    }
                    else
                    {
                        selectedCorner = new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f));
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                RaycastHit hit;
                Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100, ~0))
                {
                    Vector3Int selected;

                    if (Input.GetMouseButtonUp(0))
                    {
                        selected = GetAdjacentCoords(hit);

                        for (int x = (selectedCorner.x > selected.x ? selected.x : selectedCorner.x); x <= (selectedCorner.x < selected.x ? selected.x : selectedCorner.x); x++)
                        {
                            for (int y = (selectedCorner.y > selected.y ? selected.y : selectedCorner.y); y <= (selectedCorner.y < selected.y ? selected.y : selectedCorner.y); y++)
                            {
                                for (int z = (selectedCorner.z > selected.z ? selected.z : selectedCorner.z); z <= (selectedCorner.z < selected.z ? selected.z : selectedCorner.z); z++)
                                {
                                    GameObject tile = Instantiate(Resources.Load("Prefabs/BoardElements/floor"), new Vector3(x - 9.5f, y, z - 9.5f), Quaternion.identity) as GameObject;
                                    if (board[x, y, z] != null)
                                    {
                                        GameObject.Destroy(board[x, y, z]);
                                    }
                                    board[x, y, z] = tile;
                                }
                            }
                        }
                    }
                    else
                    {
                        selected = new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f));

                        for (int x = (selectedCorner.x > selected.x ? selected.x : selectedCorner.x); x <= (selectedCorner.x < selected.x ? selected.x : selectedCorner.x); x++)
                        {
                            for (int y = (selectedCorner.y > selected.y ? selected.y : selectedCorner.y); y <= (selectedCorner.y < selected.y ? selected.y : selectedCorner.y); y++)
                            {
                                for (int z = (selectedCorner.z > selected.z ? selected.z : selectedCorner.z); z <= (selectedCorner.z < selected.z ? selected.z : selectedCorner.z); z++)
                                {
                                    GameObject.Destroy(board[x, y, z]);
                                    board[x, y, z] = null;
                                }
                            }
                        }
                    }
                }
            }

            yield return null;
        }
    }

    private Vector3Int GetAdjacentCoords (RaycastHit hit) 
    {
        float xDist = hit.point.x - hit.transform.position.x;
        float yDist = hit.point.y - hit.transform.position.y;
        float zDist = hit.point.z - hit.transform.position.z;

        if (Mathf.Abs(xDist) > Mathf.Abs(yDist) && Mathf.Abs(xDist) > Mathf.Abs(zDist))
        {
            return new Vector3Int((int)(hit.transform.position.x + 9.5f + (xDist > 0 ? 1 : -1)), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f));
        } else if (Mathf.Abs(yDist) > Mathf.Abs(xDist) && Mathf.Abs(yDist) > Mathf.Abs(zDist))
        {
            return new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y + (yDist > 0 ? 1 : -1)), (int)(hit.transform.position.z + 9.5f));
        } else
        {
            return new Vector3Int((int)(hit.transform.position.x + 9.5f), (int)(hit.transform.position.y), (int)(hit.transform.position.z + 9.5f + (zDist > 0 ? 1 : -1)));
        }

    }
}
