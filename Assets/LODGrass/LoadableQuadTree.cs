using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadableQuadTree<TContainer, TData> : QuadTree<TContainer>
    where TContainer : LoadableDataContainer<TData>
    where TData : struct
{
    public string FolderPath { get; private set; }

    private MonoBehaviour monoBehaviour; // This is here to start coroutines (for async loading)

    public List<QuadTreeNode<LoadableDataContainer<TData>>> LoadedNodes { get; protected set; }

    public LoadableQuadTree(MonoBehaviour monoBehaviour, string folderPath) : base()
    {
        this.monoBehaviour = monoBehaviour;

        if (SetupFolder(folderPath))
            this.FolderPath = folderPath;
    }

    public LoadableQuadTree(MonoBehaviour monoBehaviour, string folderPath, Vector3 position, float size) : base(position, size)
    {
        this.monoBehaviour = monoBehaviour;

        if (SetupFolder(folderPath))
            this.FolderPath = folderPath;
    }

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
    public virtual void UpdateLoaded()
    {
        // NOT IMPLEMENTED!
    }

    public void Insert()
    {

    }

    public void LoadNodeAndUp(QuadTreeNode<TContainer> node)
    {

        node.Content.LoadData
    }
}

public abstract class LoadableQuadTreeNode<TData, TContent, TNode> : QuadTreeNodeBase<TContent, TNode> 
    where TData : struct 
    where TContent : LoadableDataContainer<TData>
    where TNode : LoadableQuadTreeNode<TData, TContent, TNode>
{
    public TContent DataContainer { get; protected set; }

    public LoadableQuadTreeNode(QuadNodePosition relativePosition, float size, TNode parent) : base(relativePosition, size, parent)
    {
        this.Content = CreateContent();
    }
    
    public LoadableQuadTreeNode(Vector3 position, float size) : base(position, size)
    {
        this.Content = CreateContent();
    }

    protected abstract TContent CreateContent();

    public virtual void UnloadData()
    {
        this.Content.UnloadData();
    }
}


public abstract class LoadableDataContainer<TData> : ILoadableDataContainer 
    where TData : struct
{
    public string FileName { get; protected set; }
    public bool IsLoaded { get; protected set; } = false;
    public TData? Data { get; protected set; }

    public LoadableDataContainer(string fileName)
    {
        this.FileName = fileName;
        this.IsLoaded = false;
    }

    public LoadableDataContainer(string fileName, TData data)
    {
        this.FileName = fileName;
        this.Data = data;
        this.IsLoaded = true;
    }

    public virtual void UnloadData()
    {
        this.Data = null;
        Resources.UnloadUnusedAssets(); // Probably a bad idea if multiple are unloaded
    }

    public abstract void SaveData(string folderPath);

    public abstract IEnumerator LoadDataCoroutine(string folderPath);

    public abstract bool LoadData(string folderPath);
}

public interface ILoadableDataContainer
{
    public bool LoadData(string folderPath);
    public IEnumerator LoadDataCoroutine(string folderPath);
    public void UnloadData();
    public void SaveData(string folderPath);
}

