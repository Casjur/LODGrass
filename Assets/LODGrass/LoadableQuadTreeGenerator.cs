using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LoadableQuadTreeGenerator<T> where T : class
{
    //public static QuadTree<LODTile> GenerateLoadableQuadTree(string folderPath, Terrain canvas, float pixelsPerMeter, double maxTreeSize)
    //{
    //    int w_pixels = 1;
    //    int h_pixels = 1;
    //    float w_minTileSize = 1;
    //    float h_minTileSize = 1;

    //    return GenerateLoadableQuadTree(folderPath, canvas, w_minTileSize, h_minTileSize, w_pixels, h_pixels, maxTreeSize);
    //}

    //public static LoadableQuadTree<LODTile> GenerateLoadableQuadTree(string folderPath, Terrain canvas, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels, double maxStoredPixels)
    //{
    //    float w_canvasSize = canvas.terrainData.size.x;
    //    float h_canvasSize = canvas.terrainData.size.z;

    //    // Safeguard to avoid absurd memory/diskspace usage
    //    bool isTooLarge = IsTreeMemoryTooLarge(maxStoredPixels, w_canvasSize, h_canvasSize, w_minTileSize, h_minTileSize, w_pixels, h_pixels);
    //    if (isTooLarge)
    //        throw new Exception("Resulting Tile Tree will be too large!");

    //    LoadableQuadTree<LODTile> tree = new LoadableQuadTree<LODTile>(folderPath, position, size);

    //    return tree;
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="canvas"></param>
    /// <param name="detailMapDensity"> Pixels per meter^2 </param>
    /// <param name="detailMapPixelWidth"></param>
    /// <param name="maxStoredPixels"></param>
    /// <returns></returns>
    public static LoadableQuadTree<T> GenerateLoadableQuadTree(string folderPath, Terrain canvas, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels)
    {
        Vector3 position = canvas.transform.position;

        float w_canvas = canvas.terrainData.size.x;
        float h_canvas = canvas.terrainData.size.z;

        float w_detailMap = Mathf.Sqrt(detailMapDensity);
        float w_totalPixels = w_canvas * w_detailMap;
        float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

        int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2) + 0.5);

        float w_smallTile = detailMapPixelWidth / w_detailMap;

        float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

        // Safeguard to avoid absurd memory/diskspace usage
        bool isTooLarge = IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
        if (isTooLarge)
            throw new Exception("Resulting Tile Tree will be too large!");

        LoadableQuadTree<T> tree = new LoadableQuadTree<T>(folderPath, position, w_rootSize);

        return tree;
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
}



public class LoadableQuadTree<T> : QuadTree<T> where T : class
{
    public string FolderPath { get; private set; }

    public LoadableQuadTree(string folderPath, Vector3 position, float size) : base(position, size)
    {
        if (SetupFolder(folderPath))
            this.FolderPath = folderPath;
    }

    private bool SetupFolder(string folderPath)
    {
        DirectoryInfo folder = Directory.CreateDirectory(folderPath);
        if (folder == null)
            return false;

        return true;
    }

    public void Insert()
    {

    }
}

public abstract class LoadableQuadTreeNode<U> : QuadTreeNode<LoadableDataContainer<U>> where U : struct
{
    public  LoadableDataContainer<U> DataContainer { get; protected set; }

    public LoadableQuadTreeNode(Vector3 position, float size, string fileName, QuadTreeNode<LoadableDataContainer<U>> parent = null) : base(position, size, parent)
    {
        
    }

    

    public virtual void UnloadData()
    {
        this.DataContainer.UnloadData();
    }

}


public abstract class LoadableDataContainer<U> where U : struct
{
    public string FileName { get; private set; }
    public bool IsLoaded { get; private set; }
    public U? Data { get; private set; }

    public LoadableDataContainer(U? data = null)
    {
        this.Data = data;
    }

    public virtual void UnloadData()
    {
        this.Data = null;
        Resources.UnloadUnusedAssets();
    }

    public abstract void SaveData(string fullFilePath);

    public abstract IEnumerator LoadDataCoroutine(string fullFilePath);

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
}

