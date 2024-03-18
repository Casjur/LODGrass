using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// Potential optimizations:
// 1. In renderer, load nodes async all at once, not one at a time

public class Grass : MonoBehaviour
{
    // Debug vars
    public static Material testMat;
    public GameObject testObject;
    public Transform brushTransform;

    // Controls
    public const bool _enableEditing = true;
    public bool doUpdateWithCamera = false;

    // Generation input variables
    [SerializeField] string folderPath;// = "/GrassData";
    [SerializeField] Terrain terrain; 
    [SerializeField] float detailMapDensity = 6.5f; // Ghost of Tsushima waarde (ongv: 200m tile oftewel 0.39 texel)
    [SerializeField] int detailMapPixelWidth = 512; // Ghost of Tsushima waarde
    [SerializeField] double maxStoredPixels = 357826560;
    [SerializeField] float grassDensity = 8;

    // Dependencies
    [SerializeField] private Camera camera;
    
    //[field: SerializeField]
    //public const float SplitDistanceMultiplier = 2;

    // Contents
    public LoadableGrassMQT GrassData { get; private set; }
    private GrassRenderer GrassRenderer;

    // Start is called before the first frame update
    void Start()
    {
        this.GrassRenderer = new GrassRenderer();

        string wrongFullFolderPath = Path.Combine(Application.dataPath, folderPath);
        string fullFolderPath = wrongFullFolderPath.Replace("\\", "/");

        Vector2 terrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);

        this.GrassData = new LoadableGrassMQT(
            fullFolderPath, 
            terrain.GetPosition(), 
            terrainSize, 
            detailMapDensity, 
            detailMapPixelWidth,
            testObject
            );

        // Test painting
        //for (int i = 0; i < 20; i++)
        //{
        //    Vector3 randomPos = new Vector3(UnityEngine.Random.Range(0, terrainSize.x), 0f, UnityEngine.Random.Range(0, terrainSize.y));
        //    this.GrassData.PaintGrass(randomPos, 20, 0);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //this.GrassRenderer.ProcessAndRender(this, camera, this.GrassData);

        // Test grass painting with an objects position
        if (_enableEditing)
            this.GrassData.PaintGrass(brushTransform.position, (int)brushTransform.localScale.x, 0);

        this.GrassData.doUpdateWithCamera = this.doUpdateWithCamera;
        this.GrassData.UpdateLoaded(camera.transform.position);
        DrawLoadedTiles(this.GrassData.GetLoadedNodes());
    }

    //
    private void DrawLoadedTiles(List<LoadableGrassMQTNode> loadedNodes)
    {
        foreach(LoadableGrassMQTNode node in loadedNodes)
        {
            node.Bounds.DrawTile(Color.red);
            Debug.Log("Drawing Bounds. Pos: " + node.Bounds.GetPosition() + "; Size: " + node.Bounds.GetSize());
            //GameObject.CreatePrimitive(PrimitiveType.Quad);
        }
    }
}

public class GrassTileData
{
    public Texture2D exampleTexture;

    public GrassTileData(int width, int height)
    {
        exampleTexture = new Texture2D(width, height);
    }

    public GrassTileData(Texture2D texture)
    {
        this.exampleTexture = texture;
    }
}
