using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODTile
{
    // Indicies of possible quadrant positions
    public enum QuadNodePosition { TopLeft = 0, TopRight = 1, BottomLeft = 2, BottomRight = 3 };

    // Position relative to the TopLeft origin point of the parent tile
    public static readonly Vector3[] RelativePositions = {
        Vector3.zero,
        Right,
        Bottom,
        Bottom + Right
    };

    public static readonly Vector3 Top = new Vector3(0, 0, 1);
    public static readonly Vector3 Bottom = new Vector3(0, 0, -1);
    public static readonly Vector3 Left = new Vector3(-1, 0, 0);
    public static readonly Vector3 Right = new Vector3(1, 0, 0);

    // Position and scale (always convert Tile's y to world z)
    public Rect Tile { get; private set; }

    public LODTile(QuadNodePosition relativePosition, float size) 
        : this(LODTile.RelativePositions[(int)relativePosition] * size, size) 
    { }

    public LODTile(Vector3 position, float size)
    {
        this.Tile = new Rect(
            new Vector2(position.x, position.z),
            new Vector2(size, size)
            );
    }

    public Vector3 GetPosition()
    {
        return new Vector3(this.Tile.x, 0, this.Tile.y);
    }

    public float GetSize()
    {
        return this.Tile.size.x;
    }

    public bool IsPointInTile(Vector3 v)
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

//public class GrassLODTile : LODTile
//{
    
//    public GrassTileData? Data { get; private set; } = null;

//    // Example of a texture name suffix
//    public static string exampleNaming = "_h";

//    public GrassLODTile(Vector3 position, float size, string fileName) : base(position, size)
//    {
//        this.FileName = fileName;
//    }

    

    
//}

public struct GrassTileData
{
    public Texture2D exampleTexture;
}
