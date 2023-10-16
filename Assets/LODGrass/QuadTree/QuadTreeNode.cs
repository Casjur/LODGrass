using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode<TContent> //: IQuadTreeNode
{
    public QuadTreeNode<TContent> Parent { get; protected set; }

    public QuadTreeNode<TContent> BottomRight { get; protected set; }
    public QuadTreeNode<TContent> BottomLeft { get; protected set; }
    public QuadTreeNode<TContent> TopRight { get; protected set; }
    public QuadTreeNode<TContent> TopLeft { get; protected set; }

    public Rect3D Tile { get; set; }

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

        this.Tile = new Rect3D(parent.Tile.GetPosition(), relativePosition, size);

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
        this.Tile = new Rect3D(position, size);

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

public class GrassMQT : LoadableMQT<GrassTileData>
{
    public GrassMQTNode GetBottomNodeAtPosition(Vector3 position)
    {
        // Dont do anything if position is not on terrain
        if (!this.Root.Tile.IsPointInTile(position))
            return null;

        GrassMQTNode node = this.Root;

        // Find bottom node on position
        while (node != null)
        {
            if (!node.HasChildren)
                return node;

            QuadNodePosition relativePosition = node.Tile.GetRelativePositionInTile(position);
            switch (relativePosition)
            {
                case QuadNodePosition.NE:
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
}

public class GrassMQTNode : LoadableMQTNode<GrassTileData>
{
    public GrassMQTNode(Rect3D bounds) : base(bounds)
    {
    }

    public GrassMQTNode(LoadableMQTNode<GrassTileData> parent, Rect3D bounds, QuadNodePosition relativePosition) : base(parent, bounds, relativePosition)
    {
    }
}

public class LoadableMQT<TContent>
    : MinimalQuadTreeAbstract<LoadableMQTNode<TContent>, TContent>
{

}

public class LoadableMQTNode<TContent> 
    : MinimalQuadTreeNodeAbstract<TContent, LoadableMQTNode<TContent>>
{
    public UInt32 Index { get; private set; } // Describes Layer and RelativePosition, so maybe redundant
    public int Layer { get; protected set; }
    public QuadNodePosition RelativePosition { get; protected set; }
    public string FileName { get; private set; }

    public LoadableMQTNode(Rect3D bounds) 
        : base(bounds)
    {
        this.Index = 0;
    }

    public LoadableMQTNode(LoadableMQTNode<TContent> parent, Rect3D bounds, QuadNodePosition relativePosition) 
        : base(parent, bounds)
    {
        GenerateIndex(parent.Index, this.GetDepth(), relativePosition);
    }

    protected void GenerateIndex(UInt32 parentIndex, int layer, QuadNodePosition relativePosition)
    {
        this.Index = ((uint)relativePosition << (layer * 2)) | parentIndex;
    }

    public virtual string GetFileName()
    {
        return ConvertIndexToString(this.Index);
    }

    public static string ConvertIndexToString(UInt32 index)
    {
        byte[] bytes = BitConverter.GetBytes(index);
        return Convert.ToBase64String(bytes);
    }

    

    //public void ExpandNode(int maxDepth, int layers)
    //{
    //    if (this.Layer >= maxDepth || layers < 1)
    //        return;

    //    layers--;

    //    this.GenerateBottomLeft();
    //    this.BottomLeft.ExpandNode(maxDepth, layers);
    //    this.GenerateBottomRight();
    //    this.BottomRight.ExpandNode(maxDepth, layers);
    //    this.GenerateTopLeft();
    //    this.TopLeft.ExpandNode(maxDepth, layers);
    //    this.GenerateTopRight();
    //    this.TopRight.ExpandNode(maxDepth, layers);
    //}

    public void DrawTiles(int depth)
    {
        if (this.Layer > depth)
            return;

        if (this.NE != null)
            this.NE.DrawTiles(depth);
        if (this.NW != null)
            this.NW.DrawTiles(depth);
        if (this.SE != null)
            this.SE.DrawTiles(depth);
        if (this.SW != null)
            this.SW.DrawTiles(depth);

        this.Bounds.DrawTile(Color.blue);
    }

    public override void GenerateNW()
    {
        Rect3D childBounds = new Rect3D(this.Bounds.GetPosition(), QuadNodePosition.NW, this.Bounds.GetSize());
        this.NW = new LoadableMQTNode<TContent>(this, childBounds, QuadNodePosition.NW);

        this.HasChildren = true;
    }

    public override void GenerateNE()
    {
        Rect3D childBounds = new Rect3D(this.Bounds.GetPosition(), QuadNodePosition.NE, this.Bounds.GetSize());
        this.NE = new LoadableMQTNode<TContent>(this, childBounds, QuadNodePosition.NE);

        this.HasChildren = true;
    }

    public override void GenerateSE()
    {
        Rect3D childBounds = new Rect3D(this.Bounds.GetPosition(), QuadNodePosition.SE, this.Bounds.GetSize());
        this.SE = new LoadableMQTNode<TContent>(this, childBounds, QuadNodePosition.SE);

        this.HasChildren = true;
    }

    public override void GenerateSW()
    {
        Rect3D childBounds = new Rect3D(this.Bounds.GetPosition(), QuadNodePosition.SW, this.Bounds.GetSize());
        this.SW = new LoadableMQTNode<TContent>(this, childBounds, QuadNodePosition.SW);

        this.HasChildren = true;
    }
}


public abstract class MinimalQuadTreeAbstract<TNode, TContent>
    where TNode : MinimalQuadTreeNodeAbstract<TContent, TNode>
{
    public TNode Root { get; private set; }

    public MinimalQuadTreeAbstract()
    {

    }

    public MinimalQuadTreeAbstract(Rect3D bounds)
    {
        this.Root = GenerateRoot(bounds);
    }

    public abstract TNode GenerateRoot(Rect3D bounds);
}

public abstract class MinimalQuadTreeNodeAbstract<TContent, TNode>
    where TNode : MinimalQuadTreeNodeAbstract<TContent, TNode>
{
    public TContent Content { get; private set; }

    public TNode Parent { get; private set; }

    public TNode NE { get; protected set; } 
    public TNode NW { get; protected set; } // (x:  1, z:  1)
    public TNode SE { get; protected set; } // (x: -1, z: -1)
    public TNode SW { get; protected set; }

    public Rect3D Bounds { get; private set; } // Replace with a generic/abstract/interface for 

    public bool HasChildren { get; protected set; }

    public MinimalQuadTreeNodeAbstract(Rect3D bounds)
    {
        this.Bounds = bounds;
    }

    public MinimalQuadTreeNodeAbstract(TNode parent, Rect3D bounds)
    {
        this.Parent = parent;
        this.Bounds = bounds;
    }

    public abstract void GenerateNE();
    public abstract void GenerateNW();
    public abstract void GenerateSE();
    public abstract void GenerateSW();
    
    public int GetDepth()
    {
        if (this.Parent == null)
            return 0;
        
        return 1 + this.Parent.GetDepth();
    }
}

