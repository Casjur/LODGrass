using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T> where T : class
{
    public QuadTreeNode<T> Root { get; private set; }

    public QuadTree()
    {
        this.Root = new QuadTreeNode<T>();
    }
}

public class QuadTreeNode<T> where T : class
{
    public QuadTreeNode<T> Parent { get; private set; }

    public QuadTreeNode<T> BottomRight { get; private set; }
    public QuadTreeNode<T> BottomLeft { get; private set; }
    public QuadTreeNode<T> TopRight { get; private set; }
    public QuadTreeNode<T> TopLeft { get; private set; }

    public T Content { get; private set; }

    public void GenerateAllChildren()
    {
        
    }
}



