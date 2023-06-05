using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T> where T : class
{
    public QuadTreeNode<T> Root { get; protected set; }

    // How many layers deep the tree goes (including root)
    public int Depth { get; private set; }

    public QuadTree(Vector3 position, float size)
    {
        GenerateRoot(position, size);
        this.Depth = 1;
    }

    protected virtual void GenerateRoot(Vector3 position, float size)
    {
        this.Root = new QuadTreeNode<T>(position, size);
    }
}

public class QuadTreeNode<T> where T : class
{
    public QuadTreeNode<T> Parent { get; private set; }

    public QuadTreeNode<T> BottomRight { get; private set; }
    public QuadTreeNode<T> BottomLeft { get; private set; }
    public QuadTreeNode<T> TopRight { get; private set; }
    public QuadTreeNode<T> TopLeft { get; private set; }

    public LODTile Tile { get; set; }

    public T Content { get; private set; }

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
