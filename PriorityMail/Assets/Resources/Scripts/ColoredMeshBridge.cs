using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColoredMeshBridge : MonoBehaviour
{
    public TileElement data;
    public int index;

    void Start()
    {
        data = transform.GetComponentInParent<ModelTileBridge>().Data;
    }
}
