using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuadTreeBase<TNode> : IQuadTree 
    where TNode : IQuadTreeNode
{
    public TNode Root { get; protected set; }

    // How many layers deep the tree goes (including root)
    public int Depth { get; protected set; } // !!! NOT IMPLEMENTED !!!
    public int MaxDepth {get; protected set; }

    public QuadTreeBase()
    {
        this.Depth = 0;
    }

    public QuadTreeBase(Vector3 position, float size)
    {
        GenerateRoot(position, size);
        this.Depth = 1;
    }

    public abstract void GenerateRoot(Vector3 position, float size);
}

// Indicies of possible quadrant positions 
//(probably better if this is inside the class, 
// but than it can not be properly accessed by another class, 
// because it is generic) 
public enum QuadNodePosition { SW = 0, SE = 1, NW = 2, NE = 3 };

public abstract class QuadTreeNodeBase<TContent, TNode> : IQuadTreeNode 
    where TContent : class 
    where TNode : QuadTreeNodeBase<TContent, TNode>
{
    public TNode Parent { get; protected set; }
    
    public TNode BottomRight { get; protected set; }
    public TNode BottomLeft { get; protected set; }
    public TNode TopRight { get; protected set; }
    public TNode TopLeft { get; protected set; }

    public LODTile Tile { get; set; }

    public TContent Content { get; protected set; }

    public UInt32 Index { get; protected set; } // Describes Layer and RelativePosition, so maybe redundant
    public int Layer { get; protected set; }
    public QuadNodePosition RelativePosition { get; protected set; }
    public bool HasChildren { get; protected set; }


    public QuadTreeNodeBase(QuadNodePosition relativePosition, float size, TNode parent)
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
    public QuadTreeNodeBase(Vector3 position, float size) //, TNode parent = null)
    {
        this.Parent = null;
        this.Tile = new LODTile(position, size);

        this.Layer = 0;
        this.Index = 0;
    }

    // private void UpdateTreeDepth(int layer)
    // {
    //     if(this.Parent == null)

    //     else
    //         this.Parent.UpdateTreeDepth(layer);
    // }

    public abstract void GenerateBottomRight();
    public abstract void GenerateBottomLeft();
    public abstract void GenerateTopRight();
    public abstract void GenerateTopLeft();

    //public void GenerateAllChildren()
    //{
    //    GenerateBottomRight();
    //    GenerateBottomLeft();
    //    GenerateTopRight();
    //    GenerateTopLeft();
    //}

    protected void GenerateIndex(UInt32 parentIndex, int layer, QuadNodePosition relativePosition)
    {
        this.Index = ((uint)relativePosition << (layer * 2)) | parentIndex; 
    }

    public static string ConvertIndexToString(UInt32 index)
    {
        byte[] bytes = BitConverter.GetBytes(index);
        return Convert.ToBase64String(bytes);
    }
}

public interface IQuadTree
{
    public void GenerateRoot(Vector3 position, float size);
}

public interface IQuadTreeNode
{
    public void GenerateBottomRight();
    public void GenerateBottomLeft();
    public void GenerateTopRight();
    public void GenerateTopLeft();

    //public void GenerateAllChildren();
}
