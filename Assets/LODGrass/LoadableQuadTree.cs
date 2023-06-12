using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class LoadableQuadTree<V> : QuadTreeBase<V>
    where U : struct 
    where V : LoadableQuadTreeNode< , LoadableDataContainer<struct>>
    //where T : LoadableDataContainer<U>
{
    public string FolderPath { get; private set; }

    public abstract List<V> LoadedNodes { get; protected set; }

    public LoadableQuadTree(string folderPath, Vector3 position, float size) : base(position, size)
    {
        if (SetupFolder(folderPath))
            this.FolderPath = folderPath;
    }

    protected override void GenerateRoot(Vector3 position, float size)
    {
        throw new InvalidOperationException("LoadableQuadTree requires a fileName to create the root node.");
    }

    protected virtual void GenerateRoot(Vector3 position, float size, string fileName)
    {
        V node = CreateRootNode(position, size, fileName);
        this.Root = node;
    }

    protected abstract V CreateRootNode(Vector3 position, float size, string fileName);

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

public abstract class LoadableQuadTreeNode<TData, TContent> : QuadTreeNodeBase<TContent, LoadableQuadTreeNode<TData, TContent>> 
    where TData : struct 
    where TContent : LoadableDataContainer<TData> 
    //: QuadTreeNode<LoadableDataContainer<U>> where U : struct where T : LoadableDataContainer<U>
{
    //public LoadableDataContainer<U> DataContainer { get; protected set; }

    public LoadableQuadTreeNode(Vector3 position, float size, string fileName, LoadableQuadTreeNode<TData, TContent> parent = null) : base(position, size, parent)
    {
        GenerateContent();
    }

    protected virtual void GenerateContent()
    {
        this.Content = CreateContent();
    }

    protected abstract LoadableDataContainer<TData> CreateContent();

    public virtual void UnloadData()
    {
        this.Content.UnloadData();
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

    /// <summary>
    /// Loads all data the Tile is supposed to store.
    /// !Note: Can only be called from a monoscript class!
    /// </summary>
    //public IEnumerator LoadDataCoroutine(string path)
    //{
    //    ResourceRequest request = Resources.LoadAsync<Texture2D>(path); // Assuming the texture is in the "Resources" folder

    //    yield return request;

    //    if (request.asset != null && request.asset is Texture2D)
    //    {
    //        Texture2D texture = (Texture2D)request.asset;

    //        // Create the struct with the loaded Texture2D
    //        this.Data = new GrassTileData
    //        {
    //            exampleTexture = texture
    //        };

    //        this.IsLoaded = true;
    //    }
    //}
}

public interface ILoadableDataContainer
{
    public IEnumerator LoadDataCoroutine(string fullFilePath);
    public void UnloadData();
    public void SaveData(string fullFilePath);
}

