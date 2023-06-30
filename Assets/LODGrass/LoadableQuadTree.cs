using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class LoadableQuadTree<TData, TContent, TNode> : QuadTreeBase<TNode>
    where TData : struct
    where TContent : LoadableDataContainer<TData>
    where TNode : LoadableQuadTreeNode<TData, TContent, TNode>
{
    public string FolderPath { get; private set; }

    public List<TNode> LoadedNodes { get; protected set; }

    public LoadableQuadTree(string folderPath) : base()
    {
        if (SetupFolder(folderPath))
            this.FolderPath = folderPath;
    }

    public LoadableQuadTree(string folderPath, Vector3 position, float size) : base(position, size)
    {
        if (SetupFolder(folderPath))
            this.FolderPath = folderPath;
    }

    /// <summary>
    /// This method will not work loadable trees!
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    public override void GenerateRoot(Vector3 position, float size)
    {
        //throw new InvalidOperationException("LoadableQuadTree requires a fileName to create the root node.");
        
    }

    public virtual void GenerateRoot(Vector3 position, float size, string fileName)
    {
        TNode node = CreateRootNode(position, size, fileName);
        this.Root = node;
    }

    protected abstract TNode CreateRootNode(Vector3 position, float size, string fileName);

    private bool SetupFolder(string folderPath)
    {
        DirectoryInfo folder = Directory.CreateDirectory(folderPath);
        if (folder == null)
            return false;

        return true;
    }

    /// <summary>
    /// Update the list of loaded nodes
    /// </summary>
    public abstract void UpdateLoaded();

    public void Insert()
    {

    }
}

public abstract class LoadableQuadTreeNode<TData, TContent, TNode> : QuadTreeNodeBase<TContent, TNode> 
    where TData : struct 
    where TContent : LoadableDataContainer<TData>
    where TNode : LoadableQuadTreeNode<TData, TContent, TNode>
    //: QuadTreeNode<LoadableDataContainer<U>> where U : struct where T : LoadableDataContainer<U>
{
    //public LoadableDataContainer<U> DataContainer { get; protected set; }

    public LoadableQuadTreeNode(Vector3 position, float size, string fileName, TNode parent = null) : base(position, size, parent)
    {
        GenerateContent();
    }

    protected virtual void GenerateContent()
    {
        this.Content = CreateContent();
    }

    protected abstract TContent CreateContent();

    public virtual void UnloadData()
    {
        this.Content.UnloadData();
    }

    public override void GenerateBottomLeft()
    {
        this.BottomLeft = new TNode()
    }
}


public abstract class LoadableDataContainer<TData> : ILoadableDataContainer where TData : struct
{
    public string FileName { get; private set; }
    public bool IsLoaded { get; private set; }
    public TData? Data { get; private set; }

    public LoadableDataContainer(TData? data = null)
    {
        this.Data = data;
    }

    public virtual void UnloadData()
    {
        this.Data = null;
        Resources.UnloadUnusedAssets();
    }

    public abstract void SaveData(string fullFilePath);

    public abstract IEnumerator LoadDataCoroutine(string fullFilePath);
}

public interface ILoadableDataContainer
{
    public IEnumerator LoadDataCoroutine(string fullFilePath);
    public void UnloadData();
    public void SaveData(string fullFilePath);
}

