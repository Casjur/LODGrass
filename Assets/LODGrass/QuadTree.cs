using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T> where T : class
{
    public QuadTreeNode<T> Root { private set; get; }

    public QuadTree()
    {
        this.Root = new QuadTreeNode<T>();
    }
}

public class QuadTreeNode<T> where T : class
{
    public QuadTreeNode<T> Parent { private set; get; }

    public QuadTreeNode<T> BottomRight { private set; get; }
    public QuadTreeNode<T> BottomLeft { private set; get; }
    public QuadTreeNode<T> TopRight { private set; get; }
    public QuadTreeNode<T> TopLeft { private set; get; }

    public void GenerateAllChildren()
    {

    }
}

public class TileData
{
    public string FileName { private set; get; }


}

