using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Potential optimizations:
// 1. In renderer, load nodes async all at once, not one at a time

public class Grass : MonoBehaviour
{
    // 
    public const bool enableEditing = false;

    // Generation input variables
    [SerializeField]
    string folderPath = "Assets/GrassData";
    [SerializeField]
    Terrain terrain;
    [SerializeField]
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

    // Contents
    public GrassQuadTree GrassData { get; private set; }
    private GrassRenderer GrassRenderer = new GrassRenderer();

    // Start is called before the first frame update
    void Start()
    {
        string fullFolderPath = Application.dataPath + folderPath;
        Vector2 terrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);

        this.GrassData = new GrassQuadTree(
            fullFolderPath, 
            terrain.GetPosition(), 
            terrainSize, 
            detailMapDensity, 
            detailMapPixelWidth, 
            maxStoredPixels
            );

        // Test expansion
        this.GrassData.ExpandTree(this.GrassData.MaxDepth);
    }

    // Update is called once per frame
    void Update()
    {
        this.GrassRenderer.ProcessAndRender(this, camera, this.GrassData);
    }
}

public class GrassQuadTree : LoadableQuadTree<GrassDataContainer, GrassTileData>
{
    

    public GrassQuadTree(string folderPath, Vector3 position, Vector2 size, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels) 
        : base(folderPath)
    {
        float w_canvas = size.x;
        float h_canvas = size.y;

        float w_detailMap = Mathf.Sqrt(detailMapDensity);
        float w_totalPixels = w_canvas * w_detailMap;
        float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

        int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2));// + 0.5);

        float w_smallTile = detailMapPixelWidth / w_detailMap;

        float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

        // Safeguard to avoid absurd memory/diskspace usage
        bool isTooLarge = GrassQuadTree.IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
        //if (isTooLarge)
        //    throw new Exception("Resulting Tile Tree will be too large!");

        this.MaxDepth = noLayers;
        this.GenerateRoot(position, w_rootSize);
    }

    public override void GenerateRoot(Vector3 position, float size)
    {
        this.Root = new QuadTreeNode<GrassDataContainer>(position, size);
        this.Depth = 1;
        //this.LoadedNodes.Add(this.Root);
        //this.NodesToRender.Add(this.Root);
    }

    /// <summary>
    /// This method assumes you fill the entire canvas with the smallest types of LODTiles.
    /// </summary>
    /// <param name="maxSize"> Maximum acceptable number of pixels stored in this tree </param>
    /// <param name="w_canvasSize"> Terrain width </param>
    /// <returns> Returns true if the maximum size of a LODTile tree is too large for memory</returns>
    public static bool IsTreeMemoryTooLarge(double maxSize, float w_canvasSize, float h_canvasSize, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels)
    {
        // Calculate number of tiles in each direction
        int w_noTiles = (int)Math.Ceiling((double)(w_canvasSize / w_minTileSize));
        int h_noTiles = (int)Math.Ceiling((double)(h_canvasSize / h_minTileSize));

        // Calculate number of smallest tiles
        int noSmallTiles = w_noTiles * h_noTiles;

        // Calculate total number of tiles
        int noTotalTiles = noSmallTiles;
        while (noSmallTiles > 4)
        {
            noSmallTiles = (int)Math.Ceiling((double)(noSmallTiles / 4));
            noTotalTiles += noSmallTiles;
        }

        // Add 1 for the root
        noTotalTiles++;
        Debug.Log(noTotalTiles);


        // Total number of pixels
        double noTotalPixels = noTotalTiles * w_pixels * h_pixels;

        Debug.Log("Total Pixels: " + noTotalPixels);

        return noTotalPixels > maxSize;
    }
    
    
    public override void UpdateLoaded()
    {
        throw new InvalidOperationException("Requires Camera (position) to check for needed changes to loaded tiles");
    }

    //public void DrawTilesToRender()
    //{
    //    foreach(QuadTreeNode<GrassDataContainer> node in this.NodesToRender)
    //    {
    //        node.Tile.DrawTile(Color.red);
    //    }
    //}
}

public class GrassDataContainer : LoadableDataContainer<GrassTileData>
{
    public GrassDataContainer(string fileName) : base(fileName)
    {
    }

    public GrassDataContainer(string fileName, GrassTileData data) : base(fileName, data)
    {
    }

    //public override IEnumerator LoadDataCoroutine(string folderPath)
    //{
    //    if (this.IsLoaded)
    //        return;



    //    throw new System.NotImplementedException();
    //}

    /// <summary>
    /// Loads all data the Tile is supposed to store.
    /// !Note: Can only be called from a monoscript class!
    /// </summary>
    public override IEnumerator LoadDataCoroutine(string path)
    {
        if(this.IsLoaded)
            yield return null;

        ResourceRequest request = Resources.LoadAsync<Texture2D>(path); // Assuming the texture is in the "Resources" folder

        yield return request;

        if (request.asset != null && request.asset is Texture2D)
        {
            Texture2D texture = (Texture2D)request.asset;

            // Create the struct with the loaded Texture2D
            this.Data = new GrassTileData
            {
                exampleTexture = texture
            };

            this.IsLoaded = true;
        }
    }

    //public IEnumerator LoadTextureFromDisk(string filePath, Action<Texture2D> onComplete)
    //{
    //    var uri = Path.Combine(Application.streamingAssetsPath, filePath);
    //    using (var request = UnityWebRequestTexture.GetTexture(uri))
    //    {
    //        yield return request.SendWebRequest();
    //        if (request.result == UnityWebRequest.Result.Success)
    //        {
    //            this.Data
    //            texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    //            onComplete?.Invoke(texture);
    //        }
    //        else
    //        {
    //            Debug.LogError($"Failed to load texture from disk: {request.error}");
    //            onComplete?.Invoke(null);
    //        }
    //    }
    //}


    public override void SaveData(string folderPath)
    {
        throw new System.NotImplementedException();
    }
}

public struct GrassTileData
{
    public Texture2D exampleTexture;
}
