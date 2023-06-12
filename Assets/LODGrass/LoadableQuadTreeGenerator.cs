//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//public static class LoadableQuadTreeGenerator<U, T> where U : struct where T : LoadableQuadTreeNode<U, T>
//{
    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="folderPath"></param>
    ///// <param name="canvas"></param>
    ///// <param name="detailMapDensity"> Pixels per meter^2 </param>
    ///// <param name="detailMapPixelWidth"></param>
    ///// <param name="maxStoredPixels"></param>
    ///// <returns></returns>
    //public static LoadableQuadTree<U, T> GenerateLoadableQuadTree(string folderPath, Terrain canvas, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels)
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
    //    bool isTooLarge = IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
    //    if (isTooLarge)
    //        throw new Exception("Resulting Tile Tree will be too large!");

    //    LoadableQuadTree<U, T> tree = new LoadableQuadTree<U, T>(folderPath, position, w_rootSize);

    //    return tree;
    //}

//    /// <summary>
//    /// This method assumes you fill the entire canvas with the smallest types of LODTiles.
//    /// </summary>
//    /// <param name="maxSize"> Maximum acceptable number of pixels stored in this tree </param>
//    /// <param name="w_canvasSize"> Terrain width </param>
//    /// <returns> Returns true if the maximum size of a LODTile tree is too large for memory</returns>
//    public static bool IsTreeMemoryTooLarge(double maxSize, float w_canvasSize, float h_canvasSize, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels)
//    {
//        // Calculate number of tiles in each direction
//        int w_noTiles = (int)Math.Ceiling((double)(w_canvasSize / w_minTileSize));
//        int h_noTiles = (int)Math.Ceiling((double)(h_canvasSize / h_minTileSize));

//        // Calculate number of smallest tiles
//        int noSmallTiles = w_noTiles * h_noTiles;

//        // Calculate total number of tiles
//        int noTotalTiles = noSmallTiles;
//        while (noSmallTiles > 4)
//        {
//            noSmallTiles = (int)Math.Ceiling((double)(noSmallTiles / 4));
//            noTotalTiles += noSmallTiles;
//        }

//        // Add 1 for the root
//        noTotalTiles++;

//        // Total number of pixels
//        double noTotalPixels = noTotalTiles * w_pixels * h_pixels;

//        Debug.Log("Total Pixels: " + noTotalPixels);

//        return noTotalPixels > maxSize;
//    }
//}
