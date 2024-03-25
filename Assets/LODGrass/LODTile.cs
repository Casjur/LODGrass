using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Possible optimizations and current problems
// 1. Might be better to just use Bounds struct, since its
//    practically the same, and also used for Graphics.DrawMeshInstancedIndirect

public class Rect3D 
{
    // Position and scale (always convert Tile's y to world z)
    public Rect Tile { get; private set; }
    
    //public Rect3D(Vector3 parentPosition, QuadNodePosition positionIndex, float size) // Naming of positionIndex is vague
    //{ 
    //    Vector3 relativePosition = RelativePositions[(int)positionIndex] * size;
    //    Vector3 position = parentPosition + relativePosition;
    //    this.Tile = new Rect(position.x, position.z, size, size);
    //}

    public Rect3D(Vector3 position, float size)
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

    public Vector3 GetCenterPosition()
    {
        return new Vector3(this.Tile.center.x, 0, this.Tile.center.y);
    }

    public float DistanceTo(Vector3 position)
    {
        return Vector3.Distance(this.GetCenterPosition(), position) - (this.GetSize() / 2);
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

    public bool IsRectOnlyInTile(Rect rect)
    {
        Vector2 bottomLeftCorner = new Vector2(rect.x, rect.y);
        if (!this.Tile.Contains(bottomLeftCorner))
            return false;

        Vector2 topRightCorner = new Vector2(rect.xMax, rect.yMax);
        if (!this.Tile.Contains(topRightCorner))
            return false;

        return true;
    }

    public QuadNodePosition GetRelativePositionInTile(Vector3 position)
    {
        float halfLength = Tile.width / 2;

        if (position.z < Tile.y + halfLength)
        {
            if (position.x < Tile.x + halfLength)
                return QuadNodePosition.SW;
            else
                return QuadNodePosition.SE;
        }
        else
        {
            if (position.x < Tile.x + halfLength)
                return QuadNodePosition.NW;
            else
                return QuadNodePosition.NE;
        }
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

public interface IQuadrantBounds
{

}



