using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Indicies of possible quadrant positions 
//(probably better if this is inside the class, 
// but than it can not be properly accessed by another class, 
// because it is generic) 
public enum QuadNodePosition { SW = 3, SE = 2, NW = 1, NE = 0 }; // utterly ridiculous order to make indexing possible

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
    /// NOTE: Will not replace existing nodes.
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
                    if (bottomNode.NE == null)
                        bottomNode.GenerateNE();
                    bottomNode = bottomNode.NE;
                    break;
                case QuadNodePosition.NW:
                    if (bottomNode.NW == null)
                        bottomNode.GenerateNW();
                    bottomNode = bottomNode.NW;
                    break;
                case QuadNodePosition.SE:
                    if (bottomNode.SE == null)
                        bottomNode.GenerateSE();
                    bottomNode = bottomNode.SE;
                    break;
                case QuadNodePosition.SW:
                    if(bottomNode.SW == null)
                        bottomNode.GenerateSW();
                    bottomNode = bottomNode.SW;
                    break;
            }
        }

        return bottomNode;
    }
}

public abstract class MinimalQuadTreeNodeAbstract<TContent, TNode>
    where TNode : MinimalQuadTreeNodeAbstract<TContent, TNode>
{
    public TContent Content { get; set; }

    public TNode Parent { get; private set; }

    public TNode NE { get; protected set; } // (x:  1, z:  1) 
    public TNode NW { get; protected set; } 
    public TNode SE { get; protected set; } 
    public TNode SW { get; protected set; } // (x: -1, z: -1)

    public Rect3D Bounds { get; private set; } // Replace with a generic/abstract/interface for 

    public bool HasChildren { get; protected set; }

    //This should not be here
    public static readonly Vector3 North = new Vector3(0, 0, 1);
    public static readonly Vector3 South = new Vector3(0, 0, -1);
    public static readonly Vector3 East = new Vector3(1, 0, 0);
    public static readonly Vector3 West = new Vector3(-1, 0, 0);

    // Position relative to the TopLeft origin point of the parent tile
    public static readonly Vector3[] RelativePositions = {
        North + East, // NE (0)
        North,        // NW (1)
        East,         // SE (2)
        Vector3.zero  // SW (3)
    };

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

    // Jesus Christ, I have to fix the variable names here...
    private void GenerateBoundsFromParent(Rect3D parentBounds, QuadNodePosition relativePosition)
    {
        float size = parentBounds.GetSize() / 2f;
        Vector3 realRelativePosition = RelativePositions[(int)relativePosition] * size;
        Vector3 position = parentBounds.GetPosition() + realRelativePosition;
        
        this.Bounds = new Rect3D(position, size);
    }

    public int GetDepth()
    {
        if (this.Parent == null)
            return 0;

        return 1 + this.Parent.GetDepth();
    }
}



