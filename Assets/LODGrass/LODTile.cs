using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODTile
{
    // Position and scale (always convert Tile's y to world z)
    public Rect Tile { get; private set; }

    public LODTile(Vector3 position, float size)
    {
        this.Tile = new Rect(
            new Vector2(position.x, position.z),
            new Vector2(size, size)
            );
    }

    public bool isPointInTile(Vector3 v)
    {
        Vector2 v2D = new Vector2(v.x, v.z);
        return this.Tile.Contains(v2D);
    }

    public void DrawTile(Color color)
    {
        Debug.DrawLine(
            new Vector3(this.Tile.xMin, 0, this.Tile.yMin),
            new Vector3(this.Tile.xMax, 0, this.Tile.yMin),
            color
            );

        Debug.DrawLine(
            new Vector3(this.Tile.xMin, 0, this.Tile.yMin),
            new Vector3(this.Tile.xMin, 0, this.Tile.yMax),
            color
            );

        Debug.DrawLine(
            new Vector3(this.Tile.xMax, 0, this.Tile.yMin),
            new Vector3(this.Tile.xMax, 0, this.Tile.yMax),
            color
            );

        Debug.DrawLine(
            new Vector3(this.Tile.xMin, 0, this.Tile.yMax),
            new Vector3(this.Tile.xMax, 0, this.Tile.yMax),
            color
            );
    }
}

public class GrassLODTile : LODTile
{
    public string FileName { get; private set; }
    public bool IsLoaded { get; private set; }
    public GrassTileData? Data { get; private set; } = null;

    // Example of a texture name suffix
    public static string exampleNaming = "_h";

    public GrassLODTile(Vector3 position, float size, string fileName) : base(position, size)
    {
        this.FileName = fileName;
    }

    public void SaveData()
    {

    }

    /// <summary>
    /// Loads all data the Tile is supposed to store.
    /// !Note: Can only be called from a monoscript class!
    /// </summary>
    public IEnumerator LoadDataCoroutine(string path)
    {
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

    public void UnloadData()
    {
        //Texture2D bla = new Texture2D(1, 1);
        //MonoBehaviour.Destroy(bla);
        
        this.Data = null;
        Resources.UnloadUnusedAssets();
    }
}

public struct GrassTileData
{
    public Texture2D exampleTexture;
}
