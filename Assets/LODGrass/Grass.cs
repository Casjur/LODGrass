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
    private GrassRenderer GrassRenderer;

    //
    public readonly List<GrassTileData> GrassList = new List<GrassTileData>(); 

    // Start is called before the first frame update
    void Start()
    {
        this.GrassRenderer = new GrassRenderer(this);

        string fullFolderPath = Application.dataPath + folderPath;
        Vector2 terrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);

        this.GrassData = new GrassQuadTree(
            this,
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
        //this.GrassRenderer.ProcessAndRender(this, camera, this.GrassData);
    }
}

public class GrassQuadTree : LoadableQuadTree<LoadableStructContainer<GrassTileData>, GrassTileData>
{
    public GrassQuadTree(MonoBehaviour monoBehaviour, string folderPath, Vector3 position, Vector2 size, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels) 
        : base(monoBehaviour, folderPath)
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
    
    
    public override void UpdateLoaded()
    {
        throw new InvalidOperationException("Requires Camera (position) to check for needed changes to loaded tiles");
    }

    // brushSize naar world size veranderen
    public async void PaintGrass(Vector3 brushWorldPosition, int brushSize, int grassTypeIndex )
    {
        // Dont do anything if brush is not on terrain
        if (!this.Root.Tile.IsPointInTile(brushWorldPosition))
            return;

        List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> nodesToPaint = new List<QuadTreeNode<LoadableStructContainer<GrassTileData>>>();

        // Get the bottom node at position
        QuadTreeNode<LoadableStructContainer<GrassTileData>> bottomNode = this.GenerateToBottomTileAtPosition(brushWorldPosition);
        if (bottomNode == null)
            return;

        // Load nodes
        await LoadNodeAndUp(bottomNode);

        // Check whether or not the brush crosses into neighbouring tile
        Rect brushBounds = new Rect(brushWorldPosition.x, brushWorldPosition.y, brushSize, brushSize);
        
        if (bottomNode.Tile.IsRectOnlyInTile(brushBounds))
        { // Brush is only in this tile

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
                    tex.SetPixel(x, y, new Color(1, 1, 1, 0));
                }
            }

            tex.Apply();
            bottomNode.Content.SaveData(this.FolderPath);
        }
        else
        {
            // Brush crosses into a neighbouring tile
        }
       

        // Find the relative position

        // Paint on position

        // Find relative position on child

        // Paint on position

        // Up2 
    }

    private void PaintRect()
    {

    }
}

public struct GrassTileData
{
    public Texture2D exampleTexture;
}

public interface IGrassData
{

}
