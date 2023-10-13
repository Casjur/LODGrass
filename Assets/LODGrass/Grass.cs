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
    //[field: SerializeField]
    //public const float SplitDistanceMultiplier = 2;

    // Contents
    public GrassQuadTree GrassData { get; private set; }
    private GrassRenderer GrassRenderer;

    //
    public readonly List<GrassTileData> GrassList = new List<GrassTileData>(); 

    // Start is called before the first frame update
    void Start()
    {
        this.GrassRenderer = new GrassRenderer();

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
        //this.GrassData.ExpandTree(this.GrassData.MaxDepth);
    }

    // Update is called once per frame
    void Update()
    {
        //this.GrassRenderer.ProcessAndRender(this, camera, this.GrassData);
        this.GrassData.UpdateLoaded(camera.transform.position);
    }

    private void DrawLoadedTiles(List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> loadedNodes)
    {
        foreach(QuadTreeNode<LoadableStructContainer<GrassTileData>> node in loadedNodes)
        {
            
        }
    }
}

public class GrassQuadTree : LoadableQuadTree<LoadableStructContainer<GrassTileData>, GrassTileData>
{
    public const float SplitDistanceMultiplier = 2;
    public int DataMapWidth { get; private set; }

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
        if (isTooLarge)
        {
            Debug.Log("Resulting tree will be too big!");
            throw new Exception("Resulting Tile Tree will be too large!");
        }

        this.MaxDepth = noLayers;
        this.DataMapWidth = detailMapPixelWidth;

        this.GenerateRoot(position, w_rootSize);
    }

    public override void GenerateRoot(Vector3 position, float size)
    {
        this.Root = new QuadTreeNode<LoadableStructContainer<GrassTileData>>(position, size);
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
    
    
    //public override async Task UpdateLoaded()
    //{
    //    throw new InvalidOperationException("Requires Camera (position) to check for needed changes to loaded tiles");
    //}

    /// <summary>
    /// Update the list of loaded nodes
    /// </summary>
    public async void UpdateLoaded(Vector3 cameraPosition)
    {
        this.nodesToLoad.Clear();

        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
        foreach (QuadTreeNode<LoadableStructContainer<GrassTileData>> node in this.LoadedNodes)
        {
            UpdateNodeToLoad(node, cameraPosition);
        }

        // Load all found nodes that need loading
        Task[] loadingTasks = new Task[this.nodesToLoad.Count];
        for (int i = 0; i < this.nodesToLoad.Count; i++)
        {
            loadingTasks[i] = this.nodesToLoad[i].Content.LoadData(this.FolderPath);
        }

        await Task.WhenAll(loadingTasks);
    }

    private bool UpdateNodeToLoad(QuadTreeNode<LoadableStructContainer<GrassTileData>> node, Vector3 cameraPosition)
    {
        if (node == null)
            return false;

        float distance = Vector3.Distance(node.Tile.GetCenterPosition(), cameraPosition);

        // Node might be too close
        if (distance < SplitDistanceMultiplier * node.Tile.GetSize())
        {
            // Node is too close -> Split node
            if (node.HasChildren)
            {
                UpdateNodeToLoad(node.BottomLeft, cameraPosition);
                UpdateNodeToLoad(node.BottomRight, cameraPosition);
                UpdateNodeToLoad(node.TopLeft, cameraPosition);
                UpdateNodeToLoad(node.TopRight, cameraPosition);

                return true;
            }

            this.nodesToLoad.Add(node);
            return true;
        }

        if (node.Parent == null)
        {
            this.nodesToLoad.Add(node);
            return true;
        }

        float distanceToParent = Vector3.Distance(node.Parent.Tile.GetCenterPosition(), cameraPosition);

        // Node is too far away -> render parent
        if (distanceToParent >= SplitDistanceMultiplier * node.Parent.Tile.GetSize())
        {
            if (this.nodesToLoad.Contains(node.Parent))
                return true;

            this.nodesToLoad.Add(node.Parent);
            return true;
        }

        // Node is fine as it is
        this.nodesToLoad.Add(node);
        return true;
    }

    // brushSize naar world size veranderen
    public async void PaintGrass(Vector3 brushWorldPosition, int brushSize, int grassTypeIndex )
    {
        // Dont do anything if brush is not on terrain
        if (!this.Root.Tile.IsPointInTile(brushWorldPosition))
            return;

        //List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> nodesToPaint = new List<QuadTreeNode<LoadableStructContainer<GrassTileData>>>();

        // Get the bottom node at position
        QuadTreeNode<LoadableStructContainer<GrassTileData>> bottomNode = this.GenerateToBottomTileAtPosition(brushWorldPosition);
        if (bottomNode == null)
            return;

        // Load nodes
        await LoadNodeAndUp(bottomNode);

        List<Task> savingTasks = new List<Task>();

        // Check whether or not the brush crosses into neighbouring tile
        Rect brushBounds = new Rect(brushWorldPosition.x, brushWorldPosition.y, brushSize, brushSize);
        
        if (bottomNode.Tile.IsRectOnlyInTile(brushBounds))
        { // Brush is only in this tile
            // If new node, create initial data for tile
            if (bottomNode.Content.Data == null)
                bottomNode.Content.Data = new GrassTileData(this.DataMapWidth, this.DataMapWidth);
            
            Texture2D tex = bottomNode.Content.Data.Value.exampleTexture;

            // Convert to UV space
            Vector2Int relativePosition = new Vector2Int(
                (int)(brushWorldPosition.x - bottomNode.Tile.Tile.x),
                (int)(brushWorldPosition.z - bottomNode.Tile.Tile.y)
                );
            Vector2Int uv = relativePosition / (int)bottomNode.Tile.GetSize();
            
            // Set pixels of "brush"
            for (int x = uv.x; x < tex.width && x < (uv.x + brushSize); x++)
            {
                for (int y = uv.y; y < tex.height && y < (uv.y + brushSize); y++)
                {
                    tex.SetPixel(x, y, Color.red);
                }
            }

            tex.Apply();
            savingTasks.Add(bottomNode.Content.SaveData(this.FolderPath));
        }
        else
        {
            // Brush crosses into a neighbouring tile
            return;
        }

        // Carry painted changes over to parents
        QuadTreeNode<LoadableStructContainer<GrassTileData>> node = bottomNode;
        Texture2D source = bottomNode.Content.Data.Value.exampleTexture;

        for(int mipLevel = 1; node.Parent != null; mipLevel++)
        {
            // Go up
            node = node.Parent;

            // If new node, create initial data for tile
            if (node.Content.Data == null)
                node.Content.Data = new GrassTileData(this.DataMapWidth, this.DataMapWidth);

            // Paint mipmap on corner
            Texture2D target = node.Content.Data.Value.exampleTexture;

            switch (node.RelativePosition)
            {
                case QuadNodePosition.TopRight:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(255, 255));
                    break;
                case QuadNodePosition.TopLeft:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 255));
                    break;
                case QuadNodePosition.BottomRight:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(255, 0));
                    break;
                case QuadNodePosition.BottomLeft:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 0));
                    break;
                default:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 0));
                    break;
            }

            // Save changes
            savingTasks.Add(node.Content.SaveData(this.FolderPath));
        }

        await Task.WhenAll(savingTasks);
    }

    private Texture2D PaintTextureOnTexture(ref Texture2D target, Texture2D source, int mipLevel, Vector2Int position)
    {
        Color[] targetPixels = target.GetPixels();
        Color[] sourcePixels = source.GetPixels();

        int multiplier = (int)Math.Pow(2, mipLevel);

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                int targetIndex = y * target.width + x;
                int sourceIndex = y * source.width * multiplier + x * multiplier; // Gaat sowieso buiten array range

                if (targetIndex < targetPixels.Length && sourceIndex < sourcePixels.Length)
                {
                    targetPixels[targetIndex] = sourcePixels[sourceIndex];
                }
                else
                {
                    Debug.Log("Source or target texture assumed to be 'too long'");
                }

                //target.SetPixel(
                //    x + position.x, 
                //    y + position.y, 
                //    target.GetPixel(x << mipLevel, y, mipLevel)
                //    );
            }
        }

        return target;
    }

    private void PaintRect()
    {

    }
}

public struct GrassTileData
{
    public Texture2D exampleTexture;

    public GrassTileData(int width, int height)
    {
        exampleTexture = new Texture2D(width, height);
    }
}

public interface IGrassData
{

}
