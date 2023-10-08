//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//public abstract class LoadableQuadTreeNode<TData, TContent, TNode> : QuadTreeNodeBase<TContent, TNode>
//    where TData : struct
//    where TContent : LoadableDataContainer<TData>
//    where TNode : LoadableQuadTreeNode<TData, TContent, TNode>
//{
//    public TContent DataContainer { get; protected set; }

//    public LoadableQuadTreeNode(QuadNodePosition relativePosition, float size, TNode parent) : base(relativePosition, size, parent)
//    {
//        this.Content = CreateContent();
//    }

//    public LoadableQuadTreeNode(Vector3 position, float size) : base(position, size)
//    {
//        this.Content = CreateContent();
//    }

//    protected abstract TContent CreateContent();

//    public virtual void UnloadData()
//    {
//        this.Content.UnloadData();
//    }
//}
