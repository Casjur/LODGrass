using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LoadableQuadTreeGenerator<T> where T : class
{
    public static QuadTree<LODTile> GenerateLoadableQuadTree(string folderPath, Terrain canvas, float pixelsPerMeter, double maxTreeSize)
    {
        int w_pixels = 1;
        int h_pixels = 1;
        float w_minTileSize = 1;
        float h_minTileSize = 1;

        return GenerateLoadableQuadTree(folderPath, canvas, w_minTileSize, h_minTileSize, w_pixels, h_pixels, maxTreeSize);
    }

    public static LoadableQuadTree<LODTile> GenerateLoadableQuadTree(string folderPath, Terrain canvas, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels, double maxStoredPixels)
    {
        float w_canvasSize = canvas.terrainData.size.x;
        float h_canvasSize = canvas.terrainData.size.z;

        // Safeguard to avoid absurd memory/diskspace usage
        bool isTooLarge = IsTreeMemoryTooLarge(maxStoredPixels, w_canvasSize, h_canvasSize, w_minTileSize, h_minTileSize, w_pixels, h_pixels);
        if (isTooLarge)
            throw new Exception("Resulting Tile Tree will be too large!");

        LoadableQuadTree<LODTile> tree = new LoadableQuadTree<LODTile>(folderPath);

        return tree;
    }

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
        float w_canvas = canvas.terrainData.size.x;
        float h_canvas = canvas.terrainData.size.z;

        float w_detailMap = Mathf.Sqrt(detailMapDensity);
        float w_totalPixels = w_canvas * w_detailMap;
        float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

        int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2) + 0.5);

        float w_smallTile = detailMapPixelWidth / w_detailMap;

        // Safeguard to avoid absurd memory/diskspace usage
        bool isTooLarge = IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
        if (isTooLarge)
            throw new Exception("Resulting Tile Tree will be too large!");

        LoadableQuadTree<T> tree = new LoadableQuadTree<T>(folderPath);

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

        return noTotalPixels > maxSize;
    }
}



public class LoadableQuadTree<T> : QuadTree<T> where T : class
{
    public string FolderPath { get; private set; }

    public LoadableQuadTree(string folderPath) : base()
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
}
