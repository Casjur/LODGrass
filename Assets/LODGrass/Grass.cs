using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
    int detailMapDensity = 4;
    [SerializeField]
    int detailMapPixelWidth = 512;
    [SerializeField]
    double maxStoredPixels = 357826560;
    [SerializeField]
    float grassDensity = 8;

    // Contents
    public GrassQuadTree GrassData { get; private set; }

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

        //CreateGrassQuadTree(fullFolderPath, terrain, detailMapDensity, detailMapPixelWidth, maxStoredPixels);
    }

    

    //protected void CreateGrassQuadTree(string folderPath, Terrain canvas, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels)
    //{
    //    Vector3 position = canvas.transform.position;

    //    float w_canvas = canvas.terrainData.size.x;
    //    float h_canvas = canvas.terrainData.size.z;

    //    float w_detailMap = Mathf.Sqrt(detailMapDensity);
    //    float w_totalPixels = w_canvas * w_detailMap;
    //    float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

    //    int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2) + 0.5);

    //    float w_smallTile = detailMapPixelWidth / w_detailMap;

    //    float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

    //    // Safeguard to avoid absurd memory/diskspace usage
    //    bool isTooLarge = GrassQuadTree.IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
    //    if (isTooLarge)
    //        throw new Exception("Resulting Tile Tree will be too large!");

    //    this.GrassData = new GrassQuadTree(folderPath, position, w_rootSize);
    //}

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Render()
    {

    }
}

public class GrassQuadTree : LoadableQuadTree<GrassTileData, GrassDataContainer, GrassQuadTreeNode>
{
    public List<GrassQuadTreeNode> NodesToRender { get; protected set; } = new List<GrassQuadTreeNode>();

    public GrassQuadTree(string folderPath, Vector3 position, Vector2 size, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels) 
        : base(folderPath)
    {
        float w_canvas = size.x;
        float h_canvas = size.y;

        float w_detailMap = Mathf.Sqrt(detailMapDensity);
        float w_totalPixels = w_canvas * w_detailMap;
        float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

        int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2) + 0.5);

        float w_smallTile = detailMapPixelWidth / w_detailMap;

        float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

        // Safeguard to avoid absurd memory/diskspace usage
        bool isTooLarge = GrassQuadTree.IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
        if (isTooLarge)
            throw new Exception("Resulting Tile Tree will be too large!");

        this.GenerateRoot(position, w_rootSize);
    }

    Vector2 GetSize()
    {
        return new Vector2(1.2f, 0.1f);
    }



    //public override void GenerateRoot(Vector3 position, float size)
    //{
    //    throw new InvalidOperationException("GrassQuadTree requires a fileName to create the root node.");
    //}

    public override void GenerateRoot(Vector3 position, float size, string fileName)
    {
        this.Root = new GrassQuadTreeNode(position, size);
        this.LoadedNodes.Add(this.Root);
    }

    protected override GrassQuadTreeNode CreateRootNode(Vector3 position, float size, string fileName)
    {
        GrassQuadTreeNode node = new GrassQuadTreeNode(position, size);
        return node;
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

        // Total number of pixels
        double noTotalPixels = noTotalTiles * w_pixels * h_pixels;

        Debug.Log("Total Pixels: " + noTotalPixels);

        return noTotalPixels > maxSize;
    }

    void ExpandTree(int layers)
    {
        for (int i = 0; i < layers; i++)
        {

        }
    }

    public virtual void UpdateLoaded(Vector3 cameraPosition)
    {
        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
        foreach(GrassQuadTreeNode loaded in this.LoadedNodes)
        {
            GrassQuadTreeNode n = loaded;
            float dist = Vector3.Distance(loaded.Tile.GetPosition(), cameraPosition);
            //int distUnits = dist / this
            //if(dist)
        }
    }

    public override void UpdateLoaded()
    {
        throw new InvalidOperationException("Requires Camera (position) to check for needed changes to loaded tiles");
    }


    public void DrawTiles()
    {
        foreach(GrassQuadTreeNode node in this.NodesToRender)
        {
            node.Tile.DrawTile(Color.red);
        }
    }
}

public class GrassQuadTreeNode : LoadableQuadTreeNode<GrassTileData, GrassDataContainer, GrassQuadTreeNode>
{
    public GrassQuadTreeNode(QuadNodePosition relativePosition, float size, GrassQuadTreeNode parent) 
        : base(relativePosition, size, parent)
    {

    }

    public GrassQuadTreeNode(Vector3 position, float size) : base(position, size)
    {
        
    }

    public override void GenerateBottomLeft()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.BottomLeft = new GrassQuadTreeNode(QuadNodePosition.BottomLeft, newSize, this);
    }

    public override void GenerateBottomRight()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.BottomLeft = new GrassQuadTreeNode(QuadNodePosition.BottomRight, newSize, this);
    }

    public override void GenerateTopLeft()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.BottomLeft = new GrassQuadTreeNode(QuadNodePosition.TopLeft, newSize, this);
    }

    public override void GenerateTopRight()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.BottomLeft = new GrassQuadTreeNode(QuadNodePosition.TopRight, newSize, this);
    }

    public override void UnloadData()
    {
        this.Content.UnloadData();
    }

    protected override GrassDataContainer CreateContent()
    {
        return new GrassDataContainer();
    }
}

public class GrassDataContainer : LoadableDataContainer<GrassTileData>
{
    public override IEnumerator LoadDataCoroutine(string fullFilePath)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Loads all data the Tile is supposed to store.
    /// !Note: Can only be called from a monoscript class!
    /// </summary>
    //public IEnumerator LoadDataCoroutine(string path)
    //{
    //    ResourceRequest request = Resources.LoadAsync<Texture2D>(path); // Assuming the texture is in the "Resources" folder

    //    yield return request;

    //    if (request.asset != null && request.asset is Texture2D)
    //    {
    //        Texture2D texture = (Texture2D)request.asset;

    //        // Create the struct with the loaded Texture2D
    //        this.Data = new GrassTileData
    //        {
    //            exampleTexture = texture
    //        };

    //        this.IsLoaded = true;
    //    }
    //}

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


    public override void SaveData(string fullFilePath)
    {
        throw new System.NotImplementedException();
    }
}

public struct GrassTileData
{
    public Texture2D exampleTexture;
}
