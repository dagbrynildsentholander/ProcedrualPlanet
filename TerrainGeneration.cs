using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class TerrainGeneration : MonoBehaviour
{
    public Transform camera;
    private Vector3 tl = Vector3.zero;
    private Vector3 tr = Vector3.zero;
    private Vector3 bl = Vector3.zero;
    private Vector3 br = Vector3.zero;
    private bool flipNormals = false;

    private QuadTreeNode root;
    private float radius;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    private Vector3 cameraLastPos;
    private GameObject[] decoration;
    private int decorationPointer = 0;

    private volatile bool threadReady;
    ConcurrentQueue<TerrainThreadInfo<TerrainData>> terrainThreadInfoQueue = new ConcurrentQueue<TerrainThreadInfo<TerrainData>>();
    MeshDataCache<string, MeshData> storedMeshData = new MeshDataCache<string, MeshData>(2000);

    public Vector3 GetTL()
    {
        return tl;
    }
    public Vector3 GetTR()
    {
        return tr;
    }
    public Vector3 GetBL()
    {
        return bl;
    }
    public Vector3 GetBR()
    {
        return br;
    }

    public void RequestTerrainData(Action<TerrainData> callback)
    {
        threadReady = false;
        ThreadStart threadStart = delegate { TerrainDataThread(callback); };
        Thread thread = new Thread(threadStart);
        thread.IsBackground = true;
        thread.Start();
    }

    public void TerrainDataThread(Action<TerrainData> callback)
    {
        int size = root.GetSize(root);
        Vector3[] verts_holder = new Vector3[(QuadBuilder.res + 1) * (QuadBuilder.res + 1) * size];
        Vector3[] normals_holder = new Vector3[(QuadBuilder.res + 1) * (QuadBuilder.res + 1) * size];
        Vector2[] uvs_holder = new Vector2[(QuadBuilder.res + 1) * (QuadBuilder.res + 1) * size];
        Color[] colors_holder = new Color[(QuadBuilder.res + 1) * (QuadBuilder.res + 1) * size];
        int[] tris_holder = new int[(QuadBuilder.res) * (QuadBuilder.res) * 6 * size];

        TerrainData result = new TerrainData(verts_holder, normals_holder, uvs_holder, tris_holder, colors_holder, storedMeshData);

        result = GenerateMeshData(root.GetLeafNodes(root), result);
        lock (terrainThreadInfoQueue)
        {
            terrainThreadInfoQueue.Enqueue(new TerrainThreadInfo<TerrainData>(callback, result));
        }
    }

    void OnTerrainDataRecieved(TerrainData data)
    {
        Destroy(mesh);
        mesh = new Mesh();
        mesh.SetVertices(data.GetVertices());
        mesh.SetTriangles(data.GetTriangles(), 0);
        mesh.SetNormals(data.GetNormals());
        mesh.SetUVs(0, data.GetUVs());
        mesh.SetColors(data.GetColors());
        storedMeshData = data.GetCache();
        meshFilter.sharedMesh = mesh;
        threadReady = true;
        
    }

    public void Create(Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, bool flipped, Material terrainMaterial, float radius)
    {
        this.tl = tl;
        this.tr = tr;
        this.bl = bl;
        this.br = br;
        this.radius = radius;
        flipNormals = flipped;

        camera = Camera.main.gameObject.transform;

        root = new QuadTreeNode(null, "", 0, tl, tr, bl, br, 0, radius);
        root.Recalculate(camera.position);

        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;

        if (!QuadBuilder.Generated)
        {
            QuadBuilder.GenerateQuadTemplate();
        }

        GenerateMesh();
    }

    public void Create(Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, bool flipped, Material terrainMaterial, float radius, GameObject[] decorations, Transform cam)
    {
        this.tl = tl;
        this.tr = tr;
        this.bl = bl;
        this.br = br;
        this.radius = radius;
        flipNormals = flipped;

        camera = cam;

        root = new QuadTreeNode(null, "", 0, tl, tr, bl, br, 0, radius);
        root.Recalculate(camera.position);

        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;

        if (!QuadBuilder.Generated)
        {
            QuadBuilder.GenerateQuadTemplate();
        }

        // instantiating is slow, create trees before
        /*
        if (decorations != null)
        {
            decoration = new GameObject[1000];

            for(int i = 0; i < 1000; i++)
            {
                decoration[i] = Instantiate(decorations[Random.Range(0, decorations.Length)], transform);
            }
        }
        */
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        RequestTerrainData(OnTerrainDataRecieved);
    }

    private Vector3 GetDisp(Vector4 heightAndNormalData, Vector3 position)
    {
        return (heightAndNormalData.x + radius) * position.normalized;
    }

    private Vector3 GetNormal(Vector4 heightAndNormalData, Vector3 position)
    {
        Vector3 gradient = new Vector3(heightAndNormalData.y, heightAndNormalData.z, heightAndNormalData.w);

        Vector3 surfGradient = gradient - position * Vector3.Dot(gradient, position);

        return (position - gradient*radius*.6f).normalized;
    }

    private Color GetVertexColor(Vector3 position)
    {
        float temp = 1 - Mathf.Pow(position.normalized.y, 4);
        temp += NoiseGeneration.NoiseSample(position / 1000 + new Vector3(radius, radius, radius)) * .25f;
        temp += NoiseGeneration.NoiseSample(position / 500 + new Vector3(radius, radius, radius)) * .125f;
        float lowest = 200;
        float highest = 1000;
        float heightFallof = Mathf.Clamp01((Vector3.Distance(position, Vector3.zero) - radius - lowest) / (highest));
        temp -= heightFallof * heightFallof * heightFallof * 2;
        temp = Mathf.Clamp01(temp);

        float humid = (NoiseGeneration.NoiseSample(position / 5000 - new Vector3(radius, radius, radius)) + 1) / 2;
        humid += (NoiseGeneration.NoiseSample(position / 2500 - new Vector3(radius, radius, radius)) + 1) / 4;
        humid = Mathf.Clamp01(humid);
        return new Color(temp, humid, 0, 0);
    }



    private Vector4 GetHeightNormal(Vector3 position)
    {
        float scale = 1500f;
        float freq = 0.0002f;
        Vector4 noise = new Vector4();
        for (int i = 0; i < 8; i++)
        {
            Vector4 n = NoiseGeneration.Noised(position * freq);

            noise.x += (.5f - Mathf.Abs(n.x)) * scale;
            noise.y += n.y * freq * scale * -Mathf.Sign(n.x);
            noise.z += n.z * freq * scale * -Mathf.Sign(n.x);
            noise.w += n.w * freq * scale * -Mathf.Sign(n.x);

            position += new Vector3(599, 0, -1314);
            //position += new Vector3(noise.x, noise.x, noise.x); // Domain warp
            freq *= 2f;
            scale *= 0.5f;
        }
        noise *= (NoiseGeneration.Noised(position * 0.0001f).x+1)/2;
        return noise;
    }

    private TerrainData GenerateMeshData(QuadTreeNode[] Leafnodes, TerrainData data)
    {
        int[] t_tris = QuadBuilder.quadTemplateTriangles[15];

        for (int quad = 0; quad < Leafnodes.Length; quad++)
        {
            QuadTreeNode node = Leafnodes[quad];

            int sideSize = (QuadBuilder.res + 1);
            int vertCount = sideSize * sideSize;
            int offset = quad * vertCount;

            if (data.GetCache().ContainsKey(node.hashvalue))
            {
                MeshData cached;
                if (data.GetCache().TryGetValue(node.hashvalue, out cached))
                {
                    for (int i = 0; i < vertCount; i++)
                    {
                        data.GetVertices()[i + offset] = cached.GetVerts()[i];
                        data.GetNormals()[i + offset] = cached.GetNormals()[i];
                        data.GetColors()[i + offset] = cached.GetColors()[i];
                        data.GetUVs()[i + offset] = cached.GetUVs()[i];
                    }
                }
            }
            else
            {
                // Values to save to cache after mesh is generated. This improves performance the next time we generate
                // data on this node
                Vector3[] c_verts = new Vector3[vertCount];
                Vector3[] c_norms = new Vector3[vertCount];
                Vector2[] c_uvs = new Vector2[vertCount];
                Color[] c_colors = new Color[vertCount];

                for (int x = 0, i = 0; x < sideSize; x++)
                {
                    for (int z = 0; z < sideSize; z++, i++)
                    {
                        // Seams are solved by a skirt on every tile. Ugly, but works
                        Vector3 cornerTL = node.cornerTL;
                        Vector3 cornerTR = node.cornerTR;
                        Vector3 cornerBL = node.cornerBL;
                        Vector3 cornerBR = node.cornerBR;
                        Vector3 interpolated = Vector3.Lerp(Vector3.Lerp(cornerTL, cornerTR, (float)(x - 1) / (float)(QuadBuilder.res - 2)), Vector3.Lerp(cornerBL, cornerBR, (float)(x - 1) / (float)(QuadBuilder.res - 2)), (float)(z - 1) / (float)(QuadBuilder.res - 2));
                        interpolated = interpolated.normalized * radius;
                        Vector4 heightNormalData = GetHeightNormal(interpolated);
                        Vector3 normal = GetNormal(heightNormalData, interpolated);
                        Vector3 position = GetDisp(heightNormalData, interpolated);
                        Vector2 uv = new Vector2(z / (float)QuadBuilder.res, x / (float)QuadBuilder.res);
                        Color color = GetVertexColor(position);

                        if (z == 0 || z == sideSize - 1 || x == 0 || x == sideSize - 1)
                            position = position.normalized * radius * .95f;

                        int index = offset + i;
                        data.GetVertices()[index] = position;
                        data.GetNormals()[index] = normal;
                        data.GetUVs()[index] = uv;
                        data.GetColors()[index] = color;
                        c_verts[i] = position;
                        c_norms[i] = normal;
                        c_uvs[i] = uv;
                        c_colors[i] = color;
                    }
                }

                // Save newly generated MeshData to cache
                data.GetCache().TryAdd(node.hashvalue, new MeshData(c_verts, c_norms, c_uvs, c_colors));
            }

            // Triangulation
            int trisCount = QuadBuilder.res * QuadBuilder.res * 6;
            for (int i = 0; i < trisCount; i++)
            {
                if (!flipNormals)
                    data.GetTriangles()[i + quad * trisCount] = t_tris[t_tris.Length - 1 - i] + offset;
                else
                    data.GetTriangles()[i + quad * trisCount] = t_tris[i] + offset;
            }
        }

        return data;
    }

    // Update is called once per frame
    void Update()
    {
        if (terrainThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < terrainThreadInfoQueue.Count; i++)
            {
                TerrainThreadInfo<TerrainData> terrainInfo;
                if (terrainThreadInfoQueue.TryDequeue(out terrainInfo))
                {
                    terrainInfo.callback(terrainInfo.parameter);
                }
            }
        }
        
        if(threadReady && Vector3.Distance(cameraLastPos, camera.transform.position) > 50f)
        {
            cameraLastPos = camera.transform.position;
            root.Recalculate(camera.position);
            GenerateMesh();
        }
    }

    struct TerrainThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public TerrainThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

