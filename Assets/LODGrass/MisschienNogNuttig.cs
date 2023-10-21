//public class GrassDataContainer : LoadableStructContainer<GrassTileData>
//{
//    public GrassDataContainer(string fileName) : base(fileName)
//    {
//    }

//    public GrassDataContainer(string fileName, GrassTileData data) : base(fileName, data)
//    {
//    }

//    //public override IEnumerator LoadDataCoroutine(string folderPath)
//    //{
//    //    if (this.IsLoaded)
//    //        return;



//    //    throw new System.NotImplementedException();
//    //}

//    /// <summary>
//    /// Loads all data the Tile is supposed to store.
//    /// !Note: Can only be called from a monoscript class!
//    /// </summary>
//    public override IEnumerator LoadDataCoroutine(string path)
//    {
//        if(this.IsLoaded)
//            yield return null;

//        ResourceRequest request = Resources.LoadAsync<Texture2D>(path); // Assuming the texture is in the "Resources" folder

//        yield return request;

//        if (request.asset != null && request.asset is Texture2D)
//        {
//            Texture2D texture = (Texture2D)request.asset;

//            // Create the struct with the loaded Texture2D
//            this.Data = new GrassTileData
//            {
//                exampleTexture = texture
//            };

//            this.IsLoaded = true;
//        }
//    }

//    //public IEnumerator LoadTextureFromDisk(string filePath, Action<Texture2D> onComplete)
//    //{
//    //    var uri = Path.Combine(Application.streamingAssetsPath, filePath);
//    //    using (var request = UnityWebRequestTexture.GetTexture(uri))
//    //    {
//    //        yield return request.SendWebRequest();
//    //        if (request.result == UnityWebRequest.Result.Success)
//    //        {
//    //            this.Data
//    //            texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
//    //            onComplete?.Invoke(texture);
//    //        }
//    //        else
//    //        {
//    //            Debug.LogError($"Failed to load texture from disk: {request.error}");
//    //            onComplete?.Invoke(null);
//    //        }
//    //    }
//    //}


//    public override void SaveData(string folderPath)
//    {
//        throw new System.NotImplementedException();
//    }
//}


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







// ___QuadTreeBase.cs___


//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public abstract class QuadTreeBase<TNode> : IQuadTree
//    where TNode : IQuadTreeNode
//{
//    public TNode Root { get; protected set; }

//    // How many layers deep the tree goes (including root)
//    public int Depth { get; protected set; } // !!! NOT IMPLEMENTED !!!
//    public int MaxDepth { get; protected set; }

//    public QuadTreeBase()
//    {
//        this.Depth = 0;
//    }

//    public QuadTreeBase(Vector3 position, float size)
//    {
//        GenerateRoot(position, size);
//        this.Depth = 1;
//    }

//    public abstract void GenerateRoot(Vector3 position, float size);
//}



//public abstract class QuadTreeNodeBase<TContent, TNode> : IQuadTreeNode
//    where TContent : class
//    where TNode : QuadTreeNodeBase<TContent, TNode>
//{
//    public TNode Parent { get; protected set; }

//    public TNode BottomRight { get; protected set; }
//    public TNode BottomLeft { get; protected set; }
//    public TNode TopRight { get; protected set; }
//    public TNode TopLeft { get; protected set; }

//    public Rect3 Tile { get; set; }

//    public TContent Content { get; protected set; }

//    public UInt32 Index { get; protected set; } // Describes Layer and RelativePosition, so maybe redundant
//    public int Layer { get; protected set; }
//    public QuadNodePosition RelativePosition { get; protected set; }
//    public bool HasChildren { get; protected set; }


//    public QuadTreeNodeBase(QuadNodePosition relativePosition, float size, TNode parent)
//    {
//        this.Parent = parent;
//        this.Parent.HasChildren = true;
//        this.Layer = parent.Layer + 1; // Probably bad practice to extract variable from a variable that was passed for a different reason
//        //UpdateTreeDepth(this.Layer);
//        this.RelativePosition = relativePosition;

//        this.Tile = new LODTile(parent.Tile.GetPosition(), relativePosition, size);

//        GenerateIndex(parent.Index, this.Layer, relativePosition);
//    }

//    /// <summary>
//    /// Root node constructor
//    /// </summary>
//    /// <param name="position"></param>
//    /// <param name="size"></param>
//    public QuadTreeNodeBase(Vector3 position, float size) //, TNode parent = null)
//    {
//        this.Parent = null;
//        this.Tile = new LODTile(position, size);

//        this.Layer = 0;
//        this.Index = 0;
//    }

//    // private void UpdateTreeDepth(int layer)
//    // {
//    //     if(this.Parent == null)

//    //     else
//    //         this.Parent.UpdateTreeDepth(layer);
//    // }

//    public abstract void GenerateBottomRight();
//    public abstract void GenerateBottomLeft();
//    public abstract void GenerateTopRight();
//    public abstract void GenerateTopLeft();

//    //public void GenerateAllChildren()
//    //{
//    //    GenerateBottomRight();
//    //    GenerateBottomLeft();
//    //    GenerateTopRight();
//    //    GenerateTopLeft();
//    //}

//    protected void GenerateIndex(UInt32 parentIndex, int layer, QuadNodePosition relativePosition)
//    {
//        this.Index = ((uint)relativePosition << (layer * 2)) | parentIndex;
//    }

//    public static string ConvertIndexToString(UInt32 index)
//    {
//        byte[] bytes = BitConverter.GetBytes(index);
//        return Convert.ToBase64String(bytes);
//    }
//}

//public interface IQuadTree
//{
//    public void GenerateRoot(Vector3 position, float size);
//}

//public interface IQuadTreeNode
//{
//    public void GenerateBottomRight();
//    public void GenerateBottomLeft();
//    public void GenerateTopRight();
//    public void GenerateTopLeft();

//    //public void GenerateAllChildren();
//}



// ___QuadTree.cs___

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class QuadTree<TContent> //: QuadTreeBase<QuadTreeNode<TContent>> 
//{
//    public QuadTreeNode<TContent> Root { get; protected set; }

//    // How many layers deep the tree goes (including root)
//    public int Depth { get; protected set; } // !!! NOT IMPLEMENTED !!!
//    public int MaxDepth { get; protected set; }

//    public QuadTree()
//    {
//        this.Depth = 0;
//    }

//    public QuadTree(Vector3 position, float size)
//    {
//        GenerateRoot(position, size);
//        this.Depth = 1;
//    }

//    public virtual void GenerateRoot(Vector3 position, float size)
//    {
//        this.Root = new QuadTreeNode<TContent>(position, size);
//    }

//    public virtual void ExpandTree(int maxDepth, int layers)
//    {
//        if (layers < 1)
//            return;

//        this.Root.ExpandNode(maxDepth, layers);
//    }

//    public QuadTreeNode<TContent> GetBottomNodeAtPosition(Vector3 position)
//    {
//        // Dont do anything if position is not on terrain
//        if (!this.Root.Tile.IsPointInTile(position))
//            return null;

//        QuadTreeNode<TContent> node = this.Root;

//        // Find bottom node on position
//        while (node != null)
//        {
//            if (!node.HasChildren)
//                return node;

//            QuadNodePosition relativePosition = node.Tile.GetRelativePositionInTile(position);
//            switch (relativePosition)
//            {
//                case QuadNodePosition.BottomLeft:
//                    if (node.BottomLeft == null)
//                        return node;
//                    node = node.BottomLeft;
//                    break;
//                case QuadNodePosition.BottomRight:
//                    if (node.BottomRight == null)
//                        return node;
//                    node = node.BottomRight;
//                    break;
//                case QuadNodePosition.TopLeft:
//                    if (node.TopLeft == null)
//                        return node;
//                    node = node.TopLeft;
//                    break;
//                case QuadNodePosition.TopRight:
//                    if (node.TopRight == null)
//                        return node;
//                    node = node.TopRight;
//                    break;
//            }
//        }

//        return node;
//    }

//    /// <summary>
//    /// Generates nodes untill the max depth is reached, following a given position.
//    /// </summary>
//    /// <param name="position"></param>
//    /// <returns>The bottom node that was generated</returns>
//    public QuadTreeNode<TContent> GenerateToBottomTileAtPosition(Vector3 position)
//    {
//        if (this.Root == null)
//            return null;

//        QuadTreeNode<TContent> bottomNode = this.Root;

//        while (bottomNode.Layer < this.MaxDepth)
//        {
//            QuadNodePosition relativePosition = bottomNode.Tile.GetRelativePositionInTile(position);
//            switch (relativePosition)
//            {
//                case QuadNodePosition.BottomLeft:
//                    bottomNode.GenerateBottomLeft();
//                    bottomNode = bottomNode.BottomLeft;
//                    break;
//                case QuadNodePosition.BottomRight:
//                    bottomNode.GenerateBottomRight();
//                    bottomNode = bottomNode.BottomRight;
//                    break;
//                case QuadNodePosition.TopLeft:
//                    bottomNode.GenerateTopLeft();
//                    bottomNode = bottomNode.TopLeft;
//                    break;
//                case QuadNodePosition.TopRight:
//                    bottomNode.GenerateTopRight();
//                    bottomNode = bottomNode.TopRight;
//                    break;
//            }
//        }

//        return bottomNode;
//    }

//    public void DrawAllTiles()
//    {
//        this.Root.DrawTiles(this.MaxDepth);
//    }
//}

