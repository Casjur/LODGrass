using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<TContent> //: QuadTreeBase<QuadTreeNode<TContent>> 
    where TContent : class
{
    public QuadTreeNode<TContent> Root { get; protected set; }

    // How many layers deep the tree goes (including root)
    public int Depth { get; protected set; } // !!! NOT IMPLEMENTED !!!
    public int MaxDepth { get; protected set; }

    public QuadTree()
    {
        this.Depth = 0;
    }

    public QuadTree(Vector3 position, float size)
    {
        GenerateRoot(position, size);
        this.Depth = 1;
    }

    public virtual void GenerateRoot(Vector3 position, float size)
    {
        this.Root = new QuadTreeNode<TContent>(position, size);
    }

    public virtual void ExpandTree(int maxDepth, int layers)
    {
        if (layers < 1)
            return;

        this.Root.ExpandNode(maxDepth, layers);
    }

    public QuadTreeNode<TContent> GetBottomNodeAtPosition(Vector3 position)
    {
        // Dont do anything if position is not on terrain
        if (!this.Root.Tile.IsPointInTile(position))
            return null;

        QuadTreeNode<TContent> node = this.Root;

        // Find bottom node on position
        while (node != null)
        {
            if (!node.HasChildren)
                return node;

            QuadNodePosition relativePosition = node.Tile.GetRelativePositionInTile(position);
            switch (relativePosition)
            {
                case QuadNodePosition.BottomLeft:
                    if (node.BottomLeft == null)
                        return node;
                    node = node.BottomLeft;
                    break;
                case QuadNodePosition.BottomRight:
                    if (node.BottomRight == null)
                        return node;
                    node = node.BottomRight;
                    break;
                case QuadNodePosition.TopLeft:
                    if (node.TopLeft == null)
                        return node;
                    node = node.TopLeft;
                    break;
                case QuadNodePosition.TopRight:
                    if (node.TopRight == null)
                        return node;
                    node = node.TopRight;
                    break;
            }
        }

        return node;
    }

    /// <summary>
    /// Generates nodes untill the max depth is reached, following a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The bottom node that was generated</returns>
    public QuadTreeNode<TContent> GenerateToBottomTileAtPosition(Vector3 position)
    {
        if (this.Root == null)
            return null;

        QuadTreeNode<TContent> bottomNode = this.Root;

        while (bottomNode.Layer < this.MaxDepth)
        {
            QuadNodePosition relativePosition = bottomNode.Tile.GetRelativePositionInTile(position);
            switch (relativePosition)
            {
                case QuadNodePosition.BottomLeft:
                    bottomNode.GenerateBottomLeft();
                    bottomNode = bottomNode.BottomLeft;
                    break;
                case QuadNodePosition.BottomRight:
                    bottomNode.GenerateBottomRight();
                    bottomNode = bottomNode.BottomRight;
                    break;
                case QuadNodePosition.TopLeft:
                    bottomNode.GenerateTopLeft();
                    bottomNode = bottomNode.TopLeft;
                    break;
                case QuadNodePosition.TopRight:
                    bottomNode.GenerateTopRight();
                    bottomNode = bottomNode.TopRight;
                    break;
            }
        }

        return bottomNode;
    }

    public void DrawAllTiles()
    {
        this.Root.DrawTiles(this.MaxDepth);
    }
}


