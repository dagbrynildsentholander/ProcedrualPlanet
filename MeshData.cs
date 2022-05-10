using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshData
{
    private readonly Vector3[] vertices;
    private readonly Vector3[] normals;
    private readonly Vector2[] uv;
    private readonly Color[] colors;

    public Vector3[] GetVerts()
    {
        return vertices;
    }

    public Vector3[] GetNormals()
    {
        return normals;
    }
    public Vector2[] GetUVs()
    {
        return uv;
    }

    public Color[] GetColors()
    {
        return colors;
    }

    public MeshData(Vector3[] verts, Vector3[] norm, Vector2[] uvs, Color[] cols)
    {
        vertices = verts;
        normals = norm;
        colors = cols;
        uv = uvs;
    }

    public MeshData(int size)
    {
        vertices = new Vector3[size];
        normals = new Vector3[size];
        colors = new Color[size];
        uv = new Vector2[size];
    }
}
