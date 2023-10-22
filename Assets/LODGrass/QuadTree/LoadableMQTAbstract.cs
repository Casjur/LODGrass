using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class LoadableMQTAbstract<TContent, TLoadableNode>
    : MinimalQuadTreeAbstract<TContent, TLoadableNode>
    where TContent : class
    where TLoadableNode : LoadableMQTNodeAbstract<TContent, TLoadableNode>
{
    public string FolderPath { get; private set; }

    public List<LoadableMQTNodeAbstract<TContent, TLoadableNode>> LoadedNodes { get; protected set; }
    public List<TLoadableNode> nodesToLoad = new List<TLoadableNode>();

    public LoadableMQTAbstract(string folderPath)
    {
        this.FolderPath = folderPath;
    }

    public LoadableMQTAbstract(string folderPath, Rect3D bounds) : base(bounds)
    {
        this.FolderPath = folderPath;
    }
}

public abstract class LoadableMQTNodeAbstract<TContent, TLoadableNode>
    : MinimalQuadTreeNodeAbstract<TContent, TLoadableNode>
    where TContent : class
    where TLoadableNode : LoadableMQTNodeAbstract<TContent, TLoadableNode>
{
    public int Layer { get; protected set; }
    public QuadNodePosition RelativePosition { get; protected set; }
    public UInt32 Index { get; private set; } // Describes Layer and RelativePosition, so maybe redundant
    public string FileName { get; private set; }

    public LoadableMQTNodeAbstract(Rect3D bounds)
        : base(bounds)
    {
        this.Layer = this.GetDepth();
        this.Index = 0;
    }

    public LoadableMQTNodeAbstract(TLoadableNode parent, QuadNodePosition relativePosition)
        : base(parent, relativePosition)
    {
        this.Layer = this.GetDepth();
        GenerateIndex(parent.Index, this.Layer, relativePosition);
        this.FileName = ConvertIndexToString(this.Index);
    }

    protected void GenerateIndex(UInt32 parentIndex, int layer, QuadNodePosition relativePosition)
    {
        this.Index = ((uint)relativePosition << (layer * 2)) | parentIndex;
    }

    public string GetFileName()
    {
        return ConvertIndexToString(this.Index);
    }

    public static string ConvertIndexToString(UInt32 index)
    {
        byte[] bytes = BitConverter.GetBytes(index);
        return Convert.ToBase64String(bytes);
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

