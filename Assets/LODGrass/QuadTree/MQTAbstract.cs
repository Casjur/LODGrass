using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Indicies of possible quadrant positions 
//(probably better if this is inside the class, 
// but than it can not be properly accessed by another class, 
// because it is generic) 
public enum QuadNodePosition { SW = 0, SE = 1, NW = 2, NE = 3 };

/// <summary>
/// Minimal Quad Tree (MQT). Sorry for the, possibly already taken, accronym, 
/// but Im not gonna write it out every time...
/// </summary>
/// <typeparam name="TContent"></typeparam>
/// <typeparam name="TNode"></typeparam>
public abstract class MinimalQuadTreeAbstract<TContent, TNode>
    where TNode : MinimalQuadTreeNodeAbstract<TContent, TNode>
{
    public TNode Root { get; protected set; }

    public int MaxDepth { get; protected set; }

    // Misschien huidige depth toevoegen

    public MinimalQuadTreeAbstract()
    { }

    public MinimalQuadTreeAbstract(Rect3D bounds)
    {
        this.Root = GenerateRoot(bounds);
    }

    /// <summary>
    /// Creates and returns a node that represents the root node.
    /// </summary>
    /// <param name="bounds"> The bounds of the to-be-created root node. </param>
    /// <returns></returns>
    public abstract TNode GenerateRoot(Rect3D bounds);

    /// <summary>
    /// Generates nodes untill the max depth is reached, following a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The bottom node that was generated</returns>
    public TNode GenerateToBottomTileAtPosition(Vector3 position)
    {
        if (this.Root == null)
            return null;

        TNode bottomNode = this.Root;

        while (bottomNode.GetDepth() < this.MaxDepth)
        {
            QuadNodePosition relativePosition = bottomNode.Bounds.GetRelativePositionInTile(position);
            switch (relativePosition)
            {
                case QuadNodePosition.NE:
                    bottomNode.GenerateNE();
                    bottomNode = bottomNode.NE;
                    break;
                case QuadNodePosition.NW:
                    bottomNode.GenerateNW();
                    bottomNode = bottomNode.NW;
                    break;
                case QuadNodePosition.SE:
                    bottomNode.GenerateSE();
                    bottomNode = bottomNode.SE;
                    break;
                case QuadNodePosition.SW:
                    bottomNode.GenerateSW();
                    bottomNode = bottomNode.SW;
                    break;
            }
        }

        return bottomNode;
    }
}

public class Test2
{
    Test<GrassTileData> bla = new Test<GrassTileData>();
}

public class Test<T>
    where T : class
{
}

public abstract class MinimalQuadTreeNodeAbstract<TContent, TNode>
    where TNode : MinimalQuadTreeNodeAbstract<TContent, TNode>
{
    public TContent Content { get; set; }

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

    public MinimalQuadTreeNodeAbstract(TNode parent, QuadNodePosition relativePosition)
    {
        this.Parent = parent;
        GenerateBoundsFromParent(parent.Bounds, relativePosition);

        this.Parent.HasChildren = true; // Dit is een kut idee, maar dan vergeet je het niet...
    }

    public abstract void GenerateNE();
    public abstract void GenerateNW();
    public abstract void GenerateSE();
    public abstract void GenerateSW();

    private void GenerateBoundsFromParent(Rect3D parentBounds, QuadNodePosition relativePosition)
    {
        this.Bounds = new Rect3D(parentBounds.GetPosition(), relativePosition, parentBounds.GetSize());
    }

    public int GetDepth()
    {
        if (this.Parent == null)
            return 0;

        return 1 + this.Parent.GetDepth();
    }
}



