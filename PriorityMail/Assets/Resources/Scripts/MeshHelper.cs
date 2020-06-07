using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshHelper
{

    public static Mesh SolidPlane (Facet facet, float distance, float x, float y)
    {
        Mesh mesh = new Mesh();
        int direction = (facet == Facet.Down || facet == Facet.East || facet == Facet.South) ? -1 : 1;

        Vector3[] vertices;
        if (facet == Facet.Up || facet == Facet.Down)
        {
            vertices = new Vector3[4]
            {
                new Vector3(-x, direction * distance, -y),
                new Vector3(-x, direction * distance, y),
                new Vector3(x, direction * distance, y),
                new Vector3(x, direction * distance, -y)
            };
        }
        else if (facet == Facet.West || facet == Facet.East)
        {
            vertices = new Vector3[4]
            {
                new Vector3(direction * distance, x, y),
                new Vector3(direction * distance, -x, y),
                new Vector3(direction * distance, -x, -y),
                new Vector3(direction * distance, x, -y)
            };
        }
        else
        {
            vertices = new Vector3[4]
            {
                new Vector3(x, y, direction * distance),
                new Vector3(-x, y, direction * distance),
                new Vector3(-x, -y, direction * distance),
                new Vector3(x, -y, direction * distance)
            };
        }
        mesh.vertices = vertices;

        int[] tris;
        if (direction == 1)
        {
            tris = new int[] {
                0, 1, 2,
                2, 3, 0
            };
        }
        else
        {
            tris = new int[] {
                2, 1, 0,
                0, 3, 2
            };
        }
        mesh.triangles = tris;

        Vector3[] normals;
        if (facet == Facet.Up || facet == Facet.Down)
        {
            normals = new Vector3[4]
            {
                direction * Vector3.up,
                direction * Vector3.up,
                direction * Vector3.up,
                direction * Vector3.up
            };
        }
        else if (facet == Facet.West || facet == Facet.East)
        {
            normals = new Vector3[4]
            {
                direction * Vector3.left,
                direction * Vector3.left,
                direction * Vector3.left,
                direction * Vector3.left
            };
        }
        else
        {
            normals = new Vector3[4]
            {
                direction * Vector3.back,
                direction * Vector3.back,
                direction * Vector3.back,
                direction * Vector3.back
            };
        }
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        mesh.name = "GroundFacet";
        
        return mesh;
    }

}
