using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGeneration : MonoBehaviour
{
    public Material terrainMat;
    public Material waterMat;
    public float planetRadius = 10000;
    public GameObject[] decorations;
    private TerrainGeneration top;
    private TerrainGeneration right;
    private TerrainGeneration left;
    private TerrainGeneration forward;
    private TerrainGeneration back;
    private TerrainGeneration bottom;
    private Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.transform;

        NoiseGeneration.GenerateNoiseLookup();
        QuadBuilder.GenerateQuadTemplate();

        GameObject top_obj = new GameObject("top");
        top_obj.AddComponent<TerrainGeneration>();
        top = top_obj.GetComponent<TerrainGeneration>();
        top.Create(new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1), false, terrainMat, planetRadius, decorations, camera);
        
        GameObject bottom_obj = new GameObject("bottom");
        bottom_obj.AddComponent<TerrainGeneration>();
        bottom = bottom_obj.GetComponent<TerrainGeneration>();
        bottom.Create(new Vector3(-1, -1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1), true, terrainMat, planetRadius, decorations, camera);

        GameObject right_obj = new GameObject("right");
        right_obj.AddComponent<TerrainGeneration>();
        right = right_obj.GetComponent<TerrainGeneration>();
        right.Create(new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, -1), new Vector3(1, -1, 1), false, terrainMat, planetRadius, decorations, camera);

        GameObject left_obj = new GameObject("left");
        left_obj.AddComponent<TerrainGeneration>();
        left = left_obj.GetComponent<TerrainGeneration>();
        left.Create(new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), true, terrainMat, planetRadius, decorations, camera);

        GameObject forward_obj = new GameObject("forward");
        forward_obj.AddComponent<TerrainGeneration>();
        forward = forward_obj.GetComponent<TerrainGeneration>();
        forward.Create(new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1), true, terrainMat, planetRadius, decorations, camera);

        GameObject back_obj = new GameObject("back");
        back_obj.AddComponent<TerrainGeneration>();
        back = back_obj.GetComponent<TerrainGeneration>();
        back.Create(new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1), false, terrainMat, planetRadius, decorations, camera);
    }

    private bool IsVisible(TerrainGeneration tg)
    {
        Vector3 center = (tg.GetBL() + tg.GetTR()) * .5f;

        if (Vector3.Dot(center.normalized, camera.position.normalized) < 0)
            return false;
        return true;
    }
    // Update is called once per frame
    void Update()
    {      
        back.enabled = IsVisible(back);
        forward.enabled = IsVisible(forward);
        top.enabled = IsVisible(top);
        bottom.enabled = IsVisible(bottom);
        right.enabled = IsVisible(right);
        left.enabled = IsVisible(left);
    }
}
