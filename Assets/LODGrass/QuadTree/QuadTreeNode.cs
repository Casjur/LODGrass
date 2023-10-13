﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode<TContent> //: IQuadTreeNode
    where TContent : class
{
    public QuadTreeNode<TContent> Parent { get; protected set; }

    public QuadTreeNode<TContent> BottomRight { get; protected set; }
    public QuadTreeNode<TContent> BottomLeft { get; protected set; }
    public QuadTreeNode<TContent> TopRight { get; protected set; }
    public QuadTreeNode<TContent> TopLeft { get; protected set; }

    public LODTile Tile { get; set; }

    public TContent Content { get; protected set; }

    public UInt32 Index { get; protected set; } // Describes Layer and RelativePosition, so maybe redundant
    public int Layer { get; protected set; }
    public QuadNodePosition RelativePosition { get; protected set; }
    public bool HasChildren { get; protected set; }

    public QuadTreeNode(QuadNodePosition relativePosition, float size, QuadTreeNode<TContent> parent)
    {
        this.Parent = parent;
        this.Parent.HasChildren = true;
        this.Layer = parent.Layer + 1; // Probably bad practice to extract variable from a variable that was passed for a different reason
        //UpdateTreeDepth(this.Layer);
        this.RelativePosition = relativePosition;

        this.Tile = new LODTile(parent.Tile.GetPosition(), relativePosition, size);

        GenerateIndex(parent.Index, this.Layer, relativePosition);
    }

    /// <summary>
    /// Root node constructor
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    public QuadTreeNode(Vector3 position, float size)
    {
        this.Parent = null;
        this.Tile = new LODTile(position, size);

        this.Layer = 0;
        this.Index = 0;
    }

    protected void GenerateIndex(UInt32 parentIndex, int layer, QuadNodePosition relativePosition)
    {
        this.Index = ((uint)relativePosition << (layer * 2)) | parentIndex;
    }

    public static string ConvertIndexToString(UInt32 index)
    {
        byte[] bytes = BitConverter.GetBytes(index);
        return Convert.ToBase64String(bytes);
    }

    public void GenerateBottomRight()
    {
        float size = this.Tile.GetSize() / 2;
        this.BottomRight = new QuadTreeNode<TContent>(QuadNodePosition.BottomRight, size, this);
    }
    public void GenerateBottomLeft()
    {
        float size = this.Tile.GetSize() / 2;
        this.BottomLeft = new QuadTreeNode<TContent>(QuadNodePosition.BottomLeft, size, this);
    }
    public void GenerateTopRight()
    {
        float size = this.Tile.GetSize() / 2;
        this.TopRight = new QuadTreeNode<TContent>(QuadNodePosition.TopRight, size, this);
    }
    public void GenerateTopLeft()
    {
        float size = this.Tile.GetSize() / 2;
        this.TopLeft = new QuadTreeNode<TContent>(QuadNodePosition.TopLeft, size, this);
    }

    public void GenerateAllChildren()
    {
        GenerateBottomRight();
        GenerateBottomLeft();
        GenerateTopRight();
        GenerateTopLeft();
    }

    public void ExpandNode(int maxDepth, int layers)
    {
        if (this.Layer >= maxDepth || layers < 1)
            return;

        layers--;

        this.GenerateBottomLeft();
        this.BottomLeft.ExpandNode(maxDepth, layers);
        this.GenerateBottomRight();
        this.BottomRight.ExpandNode(maxDepth, layers);
        this.GenerateTopLeft();
        this.TopLeft.ExpandNode(maxDepth, layers);
        this.GenerateTopRight();
        this.TopRight.ExpandNode(maxDepth, layers);
    }

    public void DrawTiles(int depth)
    {
        if (this.Layer > depth)
            return;

        if (this.BottomLeft != null)
            this.BottomLeft.DrawTiles(depth);
        if (this.BottomRight != null)
            this.BottomRight.DrawTiles(depth);
        if (this.TopLeft != null)
            this.TopLeft.DrawTiles(depth);
        if (this.TopRight != null)
            this.TopRight.DrawTiles(depth);

        this.Tile.DrawTile(Color.blue);
    }
}
