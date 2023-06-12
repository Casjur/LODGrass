using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeA
{
    public virtual A GiveA()
    {
        return new A();
    }
}

public class TreeB : TreeA
{
    //B b;

    //public void SetB()
    //{
    //    this.b = GiveA();
    //}

    public override A GiveA()
    {
        return new B();
    }
}

public class A
{

}

public class B : A
{

}

public abstract class QuadTreeBase<TNode> : IQuadTree where TNode : IQuadTreeNode
{
    public TNode Root { get; protected set; }

    // How many layers deep the tree goes (including root)
    public int Depth { get; private set; }

    public QuadTreeBase(Vector3 position, float size)
    {
        GenerateRoot(position, size);
        this.Depth = 1;
    }

    public abstract void GenerateRoot(Vector3 position, float size);
}

public abstract class QuadTreeNodeBase<TContent, TNode> : IQuadTreeNode where TContent : class where TNode : QuadTreeNodeBase<TContent, TNode>
{
    public TNode Parent { get; protected set; }

    public TNode BottomRight { get; protected set; }
    public TNode BottomLeft { get; protected set; }
    public TNode TopRight { get; protected set; }
    public TNode TopLeft { get; protected set; }

    public LODTile Tile { get; set; }

    public TContent Content { get; protected set; }

    public QuadTreeNodeBase(LODTile.QuadNodePosition relativePosition, float size, TNode parent)
    {
        this.Parent = parent;
        this.Tile = new LODTile(relativePosition, size);
    }

    public QuadTreeNodeBase(Vector3 position, float size, TNode parent = null)
    {
        this.Parent = parent;
        this.Tile = new LODTile(position, size);
    }

    public abstract void GenerateBottomRight();
    public abstract void GenerateBottomLeft();
    public abstract void GenerateTopRight();
    public abstract void GenerateTopLeft();

    public void GenerateAllChildren()
    {
        GenerateBottomRight();
        GenerateBottomLeft();
        GenerateTopRight();
        GenerateTopLeft();
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

    public void GenerateAllChildren();
}
