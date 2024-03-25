using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public abstract class LoadableMQTAbstract<TContent, TLoadableNode>
    : MinimalQuadTreeAbstract<TContent, TLoadableNode>
    where TContent : class
    where TLoadableNode : LoadableMQTNodeAbstract<TContent, TLoadableNode>
{
    public string FolderPath { get; private set; }

    //public List<LoadableMQTNodeAbstract<TContent, TLoadableNode>> LoadedNodes { get; protected set; } = new List<LoadableMQTNodeAbstract<TContent, TLoadableNode>>();
    public List<TLoadableNode> nodesToLoad = new List<TLoadableNode>();
    public List<TLoadableNode> nodesToUnload = new List<TLoadableNode>();

    public LoadableMQTAbstract(string folderPath, int maxDepth = int.MaxValue) 
        : base(maxDepth)
    {
        this.FolderPath = folderPath;
    }

    public LoadableMQTAbstract(string folderPath, Rect3D bounds, int maxDepth = int.MaxValue) 
        : base(bounds, maxDepth)
    {
        this.FolderPath = folderPath;
    }

    public void CreateTreeFromFiles()
    {
        if(Directory.Exists(this.FolderPath))
        {
            this.Root.CreateTreeFromFiles(this.FolderPath, this.MaxDepth);
        }
    }

    public List<TLoadableNode> GetLoadedNodes()
    {
        List<TLoadableNode> loadedNodes = new List<TLoadableNode>();
        if (this.Root == null)
            return loadedNodes;

        RecurseLoadedNodes(ref loadedNodes, this.Root);

        return loadedNodes;
    }

    private void RecurseLoadedNodes(ref List<TLoadableNode> loadedNodes, TLoadableNode node)
    {
        if (node.IsLoaded)
            loadedNodes.Add(node);

        if(node.HasChildren)
        {
            if (node.NE != null)
                RecurseLoadedNodes(ref loadedNodes, node.NE);
            if (node.SE != null)
                RecurseLoadedNodes(ref loadedNodes, node.SE);
            if (node.NW != null)
                RecurseLoadedNodes(ref loadedNodes, node.NW);
            if (node.SW != null)
                RecurseLoadedNodes(ref loadedNodes, node.SW);
        }
    }
}

public abstract class LoadableMQTNodeAbstract<TContent, TLoadableNode>
    : MinimalQuadTreeNodeAbstract<TContent, TLoadableNode>
    where TContent : class
    where TLoadableNode : LoadableMQTNodeAbstract<TContent, TLoadableNode>
{
    public bool IsSaving { get; protected set; } = false;
    public bool IsSaved { get; protected set; } = false;
    public bool IsLoading { get; protected set; } = false;
    public bool IsLoaded { get; protected set; } = false;

    public int Layer { get; protected set; }
    public QuadNodePosition RelativePosition { get; protected set; }
    public UInt32 Index { get; private set; } // Describes Layer and RelativePosition, so maybe redundant. Maybe not since it describes the full position within the tree
    public string FileName { get; private set; }

    public LoadableMQTNodeAbstract(Rect3D bounds)
        : base(bounds)
    {
        this.Layer = this.GetDepth();
        this.RelativePosition = QuadNodePosition.SW;
        GenerateIndex(0, this.Layer, this.RelativePosition);
        this.FileName = ConvertIndexToString(this.Index);
    }

    public LoadableMQTNodeAbstract(TLoadableNode parent, QuadNodePosition relativePosition)
        : base(parent, relativePosition)
    {
        this.Layer = this.GetDepth();
        this.RelativePosition = relativePosition;
        GenerateIndex(parent.Index, this.Layer, relativePosition);
        this.FileName = ConvertIndexToString(this.Index);
    }

    protected void GenerateIndex(UInt32 parentIndex, int layer, QuadNodePosition relativePosition)
    {
        // this method skips a lot of numbers, thus lowering the ammount of possible nodes
        this.Index = parentIndex << (layer * 2) | (uint)relativePosition;         
        
        //ID = ((ParentID + 1) << 16) | ((uint)Layer << 8) | (uint)Position;

    }

    public string GetFileName()
    {
        return ConvertIndexToString(this.Index);
    }

    public static string ConvertIndexToString(UInt32 index)
    {
        //byte[] bytes = BitConverter.GetBytes(index);
        //return Convert.ToBase64String(bytes);

        return index.ToString();
    }

    public void CreateTreeFromFiles(string folderPath, int maxDepth)
    {
        if (this.GetDepth() >= maxDepth)
            return;

        this.GenerateNE();
        this.GenerateNW();
        this.GenerateSW();
        this.GenerateSE();

        CheckAndCreateChildTree(this.NE, folderPath, maxDepth);
        CheckAndCreateChildTree(this.NW, folderPath, maxDepth);
        CheckAndCreateChildTree(this.SW, folderPath, maxDepth);
        CheckAndCreateChildTree(this.SE, folderPath, maxDepth);
    }

    private void CheckAndCreateChildTree(TLoadableNode childNode, string folderPath, int maxDepth)
    {
        string filePath = Path.Combine(folderPath, childNode.FileName);
        if (childNode != null && File.Exists(filePath))
        {
            childNode.IsSaved = true;
            childNode.CreateTreeFromFiles(folderPath, maxDepth);
        }
        else
        {
            // If the file doesn't exist or the child node is null, set the child node to null.
            // This assumes that if the file doesn't exist, there shouldn't be any corresponding node.
            childNode = null;
        }
    }

    public async Task LoadAndUp(string folderPath)
    {
        // Load this node and up
        List<Task> loadingTasks = new List<Task>();
        loadingTasks.Add(this.LoadContent(folderPath)); // Is needed because `TLoadableNode node = this;` is not possible

        TLoadableNode node = this.Parent;

        while (node != null)
        {
            loadingTasks.Add(node.LoadContent(folderPath));

            node = node.Parent;
        }

        await Task.WhenAll(loadingTasks);
    }

    public abstract Task SaveContent(string folderPath);
    public abstract Task LoadContent(string folderPath);
    public abstract Task UnloadContent();

    public void DrawTiles(int depth)
    {
        if (this.Layer > depth)
            return;

        if (this.NE != null)
            this.NE.DrawTiles(depth);
        if (this.NW != null)
            this.NW.DrawTiles(depth);
        if (this.SE != null)
            this.SE.DrawTiles(depth);
        if (this.SW != null)
            this.SW.DrawTiles(depth);

        this.Bounds.DrawTile(Color.blue);
    }
}

