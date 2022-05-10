using UnityEngine;
public struct TerrainData
{
    private readonly Vector3[] verts;
    private readonly Vector3[] normals;
    private readonly Vector2[] uv;
    private readonly int[] tris;
    private readonly Color[] colors;
    private readonly MeshDataCache<string, MeshData> cachedMeshData;

    public Vector3[] GetVertices()
    {
        return verts;
    }
    public Vector3[] GetNormals()
    {
        return normals;
    }
    public Vector2[] GetUVs()
    {
        return uv;
    }
    public int[] GetTriangles()
    {
        return tris;
    }
    public Color[] GetColors()
    {
        return colors;
    }
    public MeshDataCache<string, MeshData> GetCache()
    {
        return cachedMeshData;
    }

    public TerrainData(Vector3[] verts, Vector3[] normals, Vector2[] uv, int[] tris, Color[] colors, MeshDataCache<string, MeshData> cachedMeshData)
    {
        this.verts = verts;
        this.normals = normals;
        this.uv = uv;
        this.colors = colors;
        this.tris = tris;
        this.cachedMeshData = cachedMeshData;
    }

    public TerrainData(TerrainData data)
    {
        this.verts = data.verts;
        this.normals = data.normals;
        this.uv = data.uv;
        this.colors = data.colors;
        this.tris = data.tris;
        this.cachedMeshData = data.cachedMeshData;
    }
}
