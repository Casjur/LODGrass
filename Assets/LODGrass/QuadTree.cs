using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<TContent> : QuadTreeBase<QuadTreeNode<TContent>> 
    where TContent : class
{
    public QuadTree()
    {
        this.Depth = 0;
    }

    public QuadTree(Vector3 position, float size)
    {
        GenerateRoot(position, size);
        this.Depth = 1;
    }

    public override void GenerateRoot(Vector3 position, float size)
    {
        this.Root = new QuadTreeNode<TContent>(position, size);
    }
}

public class QuadTreeNode<T> : IQuadTreeNode
    where T : class
{
    public QuadTreeNode<T> Parent { get; protected set; }

    public QuadTreeNode<T> BottomRight { get; protected set; }
    public QuadTreeNode<T> BottomLeft { get; protected set; }
    public QuadTreeNode<T> TopRight { get; protected set; }
    public QuadTreeNode<T> TopLeft { get; protected set; }

    public LODTile Tile { get; set; }

    public T Content { get; protected set; }

    public QuadTreeNode(LODTile.QuadNodePosition relativePosition, float size, QuadTreeNode<T> parent)
    {
        this.Parent = parent;
        this.Tile = new LODTile(relativePosition, size);
    }

    public QuadTreeNode(Vector3 position, float size, QuadTreeNode<T> parent = null)
    {
        this.Parent = parent;
        this.Tile = new LODTile(position, size);
    }

    

    public void GenerateBottomRight()
    {
        float size = this.Tile.GetSize() / 2;
        this.BottomRight = new QuadTreeNode<T>(LODTile.QuadNodePosition.BottomRight, size, this);
    }
    public void GenerateBottomLeft()
    {
        float size = this.Tile.GetSize() / 2;
        this.BottomLeft = new QuadTreeNode<T>(LODTile.QuadNodePosition.BottomLeft, size, this);
    }
    public void GenerateTopRight()
    {
        float size = this.Tile.GetSize() / 2;
        this.TopRight = new QuadTreeNode<T>(LODTile.QuadNodePosition.TopRight, size, this);
    }
    public void GenerateTopLeft()
    {
        float size = this.Tile.GetSize() / 2;       
        this.TopLeft = new QuadTreeNode<T>(LODTile.QuadNodePosition.TopLeft, size, this);
    }

    public void GenerateAllChildren()
    {
        GenerateBottomRight();
        GenerateBottomLeft();
        GenerateTopRight();
        GenerateTopLeft();
    }
}
