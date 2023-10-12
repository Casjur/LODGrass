using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LoadableQuadTree<TContainer, TData> : QuadTree<TContainer>
    where TContainer : LoadableStructContainer<TData>
    where TData : struct
{
    public string FolderPath { get; private set; }

    public List<QuadTreeNode<LoadableStructContainer<TData>>> LoadedNodes { get; protected set; }
    protected List<QuadTreeNode<LoadableStructContainer<TData>>> nodesToLoad = new List<QuadTreeNode<LoadableStructContainer<TData>>>();


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

    private bool SetupFolder(string folderPath)
    {
        DirectoryInfo folder = Directory.CreateDirectory(folderPath);
        if (folder == null)
            return false;

        return true;
    }

    public override void ExpandTree(int layers) // This should not work like this, but due to generic classes and access modifiers I cant make the nodes responsible for instantiating their own content/fileName...
    {
        ExpandNode(this.Root, layers);
    }

    private void ExpandNode(QuadTreeNode<TContainer> node, int layers)
    {
        if (layers < 1 || node == null)
            return;

        NameFileNode(node);

        layers--;
        node.GenerateAllChildren();
        ExpandNode(node.BottomLeft, layers);
        ExpandNode(node.BottomRight, layers);
        ExpandNode(node.TopLeft, layers);
        ExpandNode(node.TopRight, layers);
    }

    private void NameFileNode(QuadTreeNode<TContainer> node) // Wat kut zeg
    {
        string fileName = QuadTreeNode<TContainer>.ConvertIndexToString(node.Index);
        node.BottomLeft.Content.SetFileName(fileName);
    }

    public void Insert()
    {

    }

    public async Task LoadNodeAndUp(QuadTreeNode<TContainer> node) // List naar array aanpassen wnr Depth werkt
    {
        if (node == null)
            return;

        // Load this node and up
        List<Task> loadingTasks = new List<Task>();
        while (node != null)
        {
            loadingTasks.Add(node.Content.LoadData(this.FolderPath));

            node = node.Parent;
        }

        if (loadingTasks.Count == 0)
            return;

        await Task.WhenAll(loadingTasks);
    }
}

