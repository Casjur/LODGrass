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
    public static Material testMat;

    // 
    public const bool enableEditing = false;

    // Generation input variables
    [SerializeField] string folderPath = "/GrassData";
    [SerializeField] Terrain terrain; [SerializeField]
    float detailMapDensity = 6.5f; // Ghost of Tsushima waarde (ongv: 200m tile oftewel 0.39 texel)
    [SerializeField]
    int detailMapPixelWidth = 512; // Ghost of Tsushima waarde
    [SerializeField]
    double maxStoredPixels = 357826560;
    [SerializeField]
    float grassDensity = 8;

    //
    [SerializeField]
    private Camera camera;
    //[field: SerializeField]
    //public const float SplitDistanceMultiplier = 2;

    // Contents
    public LoadableGrassMQT GrassData { get; private set; }
    private GrassRenderer GrassRenderer;

    //
    public readonly List<GrassTileData> LoadedGrass = new List<GrassTileData>(); 

    // Start is called before the first frame update
    void Start()
    {
        this.GrassRenderer = new GrassRenderer();

        string fullFolderPath = Path.Combine(Application.dataPath, folderPath);
        Vector2 terrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);

        this.GrassData = new LoadableGrassMQT(
            fullFolderPath, 
            terrain.GetPosition(), 
            terrainSize, 
            detailMapDensity, 
            detailMapPixelWidth
            );

        this.Test();

        // Test expansion
        //this.GrassData.ExpandTree(this.GrassData.MaxDepth);
        //for (int i = 0; i < 1; i++)
        //{
        //    Vector3 randomPos = new Vector3(UnityEngine.Random.Range(0, terrainSize.x), 0f, UnityEngine.Random.Range(0, terrainSize.y));
        //    this.GrassData.PaintGrass(randomPos, 10, 1);
        //}
    }

    private async void Test()
    {
        await this.GrassData.Root.LoadContent(this.GrassData.FolderPath);
    }

    // Update is called once per frame
    void Update()
    {
        //this.GrassRenderer.ProcessAndRender(this, camera, this.GrassData);
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
