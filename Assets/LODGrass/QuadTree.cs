using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T> where T : class
{
    public QuadTreeNode<T> Root { get; private set; }

    // How many layers deep the tree goes (including root)
    public int Depth { get; private set; }

    public QuadTree()
    {
        this.Root = new QuadTreeNode<T>();
        this.Depth = 1;
    }
}

public class QuadTreeNode<T> where T : class
{
    public enum QuadNodePosition { BottomRight = 0, BottomLeft = 1, TopRight = 2, TopLeft = 3 };

    public QuadTreeNode<T> Parent { get; private set; }

    public QuadTreeNode<T> BottomRight { get; private set; }
    public QuadTreeNode<T> BottomLeft { get; private set; }
    public QuadTreeNode<T> TopRight { get; private set; }
    public QuadTreeNode<T> TopLeft { get; private set; }

    public T Content { get; private set; }

    public QuadTreeNode(QuadTreeNode<T> parent = null)
    {
        this.Parent = parent;
    }

    public void GenerateBottomRight()
    {
        this.BottomRight = new QuadTreeNode<T>();
    }
    public void GenerateBottomLeft()
    {
        this.BottomLeft = new QuadTreeNode<T>();
    }
    public void GenerateTopRight()
    {
        this.TopRight = new QuadTreeNode<T>();
    }
    public void GenerateTopLeft()
    {
        this.TopLeft = new QuadTreeNode<T>();
    }
    public void GenerateAllChildren()
    {
        GenerateBottomRight();
        GenerateBottomLeft();
        GenerateTopRight();
        GenerateTopLeft();
    }

}
