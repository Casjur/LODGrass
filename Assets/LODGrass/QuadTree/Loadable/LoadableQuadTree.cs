using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadableQuadTree<TContainer, TData> : QuadTree<TContainer>
    where TContainer : LoadableStructContainer<TData>
    where TData : struct
{
    public string FolderPath { get; private set; }

    private MonoBehaviour monoBehaviour; // This is here to start coroutines (for async loading)

    public List<QuadTreeNode<LoadableStructContainer<TData>>> LoadedNodes { get; protected set; }

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

    public async void LoadNodeAndUp(QuadTreeNode<TContainer> node)
    {
        if (node == null)
            return;

        List<Task> loadingTasks = new List<Task>();
        while (node != null)
        {
            loadingTasks.Add(
                new Task(node.Content.LoadDataCoroutine(this.FolderPath))
                );

            node = node.Parent;
        }

        if (loadingTasks.Count == 0)
            return;

        foreach(Task task in loadingTasks)
        {

        }

        // Load node
        monoBehaviour.StartCoroutine(node.Content.LoadDataCoroutine(this.FolderPath));

        // And up
        LoadNodeAndUp(node.Parent);
    }
}

