using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LODQuadTreeGenerator
{
    public static QuadTree<LODTile> GenerateLODQuadTree(string folderPath, Terrain canvas, float pixelsPerMeter, double maxTreeSize)
    {
        int w_pixels = 1;
        int h_pixels = 1;
        float w_minTileSize = 1;
        float h_minTileSize = 1;

        return GenerateLODQuadTree(folderPath, canvas, w_minTileSize, h_minTileSize, w_pixels, h_pixels, maxTreeSize);
    }

    public static LODQuadTree<LODTile> GenerateLODQuadTree(string folderPath, Terrain canvas, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels, double maxStoredPixels)
    {
        float w_canvasSize = canvas.terrainData.size.x;
        float h_canvasSize = canvas.terrainData.size.z;

        // Safeguard to avoid absurd memory/diskspace usage
        bool isTooLarge = IsTreeMemoryTooLarge(maxStoredPixels, w_canvasSize, h_canvasSize, w_minTileSize, h_minTileSize, w_pixels, h_pixels);
        if (isTooLarge)
            throw new Exception("Resulting Tile Tree will be too large!");

        LODQuadTree<LODTile> tree = new LODQuadTree<LODTile>(folderPath);

        return tree;
    }

    /// <summary>
    /// This method assumes you fill the entire canvas with the smallest types of LODTiles.
    /// </summary>
    /// <param name="maxSize"> Maximum acceptable number of pixels stored in this tree </param>
    /// <param name="w_canvasSize"></param>
    /// <param name="h_canvasSize"></param>
    /// <param name="w_minTileSize"></param>
    /// <param name="h_minTileSize"></param>
    /// <param name="w_pixels"></param>
    /// <param name="h_pixels"></param>
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
        while(noSmallTiles > 4)
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



public class LODQuadTree<T> : QuadTree<T> where T : class
{
    public string FolderPath { get; private set; }

    public LODQuadTree(string folderPath) : base()
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
