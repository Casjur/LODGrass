using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LoadableGrassMQT : LoadableMQTAbstract<GrassTileData, LoadableGrassMQTNode>
{
    public const float SplitDistanceMultiplier = 2;
    public int DataMapWidth { get; private set; }

    // ! Change names with w_ and h_, since its confusing with naming conventions
    public LoadableGrassMQT(string folderPath, Vector3 position, Vector2 size, float detailMapDensity, int detailMapPixelWidth) 
        : base(folderPath)
    {
        float w_canvas = size.x;
        float h_canvas = size.y;

        float w_detailMap = Mathf.Sqrt(detailMapDensity);
        float w_totalPixels = w_canvas * w_detailMap;
        float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

        int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2));// + 0.5);

        float w_smallTile = detailMapPixelWidth / w_detailMap;

        float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

        this.MaxDepth = noLayers;
        this.DataMapWidth = detailMapPixelWidth;

        this.Root = this.GenerateRoot(new Rect3D(position, w_rootSize));
        Debug.Log("Root created. Position: " + position + "; w_rootSize: " + w_rootSize + ";");
    }

    /// <summary>
    /// !!! DONT USE THIS !!!
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="bounds"></param>
    public LoadableGrassMQT(string folderPath, Rect3D bounds) 
        : base(folderPath, bounds)
    { }

    public override LoadableGrassMQTNode GenerateRoot(Rect3D bounds)
    {
        LoadableGrassMQTNode root = new LoadableGrassMQTNode(bounds);
        return root;
    }

    public LoadableGrassMQTNode GetBottomNodeAtPosition(Vector3 position)
    {
        // Dont do anything if position is not on terrain
        if (!this.Root.Bounds.IsPointInTile(position))
            return null;

        LoadableGrassMQTNode node = this.Root;

        // Find bottom node on position
        while (node != null)
        {
            if (!node.HasChildren)
                return node;

            QuadNodePosition relativePosition = node.Bounds.GetRelativePositionInTile(position);
            switch (relativePosition)
            {
                case QuadNodePosition.NE:
                    if (node.NE == null)
                        return node;
                    node = node.NE;
                    break;
                case QuadNodePosition.NW:
                    if (node.NW == null)
                        return node;
                    node = node.NW;
                    break;
                case QuadNodePosition.SE:
                    if (node.SE == null)
                        return node;
                    node = node.SE;
                    break;
                case QuadNodePosition.SW:
                    if (node.SW == null)
                        return node;
                    node = node.SW;
                    break;
            }
        }

        return node;
    }

    /// <summary>
    /// Update the list of loaded nodes
    /// </summary>
    public async void UpdateLoaded(Vector3 cameraPosition)
    {
        this.nodesToLoad.Clear();



        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
        List<LoadableGrassMQTNode> loadedNodes = this.GetLoadedNodes();

        if(loadedNodes.Count == 0)
        {
            Debug.Log("Only load root.");
            this.nodesToLoad.Add(this.Root);
            await this.Root.LoadContent(this.FolderPath);

            return;
        }

        foreach (LoadableGrassMQTNode node in loadedNodes)
        {
            UpdateNodeToLoad(node, cameraPosition);
        }

        // Load all found nodes that need loading
        Task[] loadingTasks = new Task[this.nodesToLoad.Count];
        for (int i = 0; i < this.nodesToLoad.Count; i++)
        {
            Debug.Log("Loading task: " + i);
            loadingTasks[i] = this.nodesToLoad[i].LoadContent(this.FolderPath);
        }

        await Task.WhenAll(loadingTasks);

        // DEBUG: 
        foreach (LoadableGrassMQTNode node in this.nodesToLoad)
        {
            Debug.Log("Make quad");
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.position = node.Bounds.GetCenterPosition();
            quad.transform.rotation = Quaternion.Euler(Vector3.right);

            quad.GetComponent<Renderer>().material.SetTexture("_MainTex", node.Content.exampleTexture);
        }
    }

    private bool UpdateNodeToLoad(LoadableGrassMQTNode node, Vector3 cameraPosition)
    {
        if (node == null)
            return false;

        float distance = Vector3.Distance(node.Bounds.GetCenterPosition(), cameraPosition);

        // Node might be too close
        if (distance < SplitDistanceMultiplier * node.Bounds.GetSize())
        {
            // Node is too close -> Split node
            if (node.HasChildren)
            {
                UpdateNodeToLoad(node.NE, cameraPosition);
                UpdateNodeToLoad(node.NW, cameraPosition);
                UpdateNodeToLoad(node.SE, cameraPosition);
                UpdateNodeToLoad(node.SW, cameraPosition);

                if(!this.nodesToUnload.Contains(node))
                    this.nodesToUnload.Add(node);

                return true;
            }

            this.nodesToLoad.Add(node);
            return true;
        }

        if (node.Parent == null)
        {
            this.nodesToLoad.Add(node);
            return true;
        }

        float distanceToParent = Vector3.Distance(node.Parent.Bounds.GetCenterPosition(), cameraPosition);

        // Node is too far away -> render parent
        if (distanceToParent >= SplitDistanceMultiplier * node.Parent.Bounds.GetSize())
        {
            if (this.nodesToLoad.Contains(node.Parent))
                return true;

            this.nodesToLoad.Add(node.Parent);
            return true;
        }

        // Node is fine as it is
        this.nodesToLoad.Add(node);
        return true;
    }

    // brushSize naar world size veranderen
    public async void PaintGrass(Vector3 brushWorldPosition, int brushSize, int grassTypeIndex)
    {
        // Dont do anything if brush is not on terrain
        if (!this.Root.Bounds.IsPointInTile(brushWorldPosition))
            return;

        //List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> nodesToPaint = new List<QuadTreeNode<LoadableStructContainer<GrassTileData>>>();

        // Get the bottom node at position
        LoadableGrassMQTNode bottomNode = this.GenerateToBottomTileAtPosition(brushWorldPosition);
        if (bottomNode == null)
            return;

        // Load nodes
        await bottomNode.LoadAndUp(this.FolderPath);

        List<Task> savingTasks = new List<Task>();

        // Check whether or not the brush crosses into neighbouring tile
        Rect brushBounds = new Rect(brushWorldPosition.x, brushWorldPosition.y, brushSize, brushSize);

        if (bottomNode.Bounds.IsRectOnlyInTile(brushBounds))
        { // Brush is only in this tile
            // If new node, create initial data for tile
            if (bottomNode.Content == null)
                bottomNode.Content = new GrassTileData(this.DataMapWidth, this.DataMapWidth);

            Texture2D tex = bottomNode.Content.exampleTexture;

            // Convert to UV space
            Vector2Int relativePosition = new Vector2Int(
                (int)(brushWorldPosition.x - bottomNode.Bounds.Tile.x),
                (int)(brushWorldPosition.z - bottomNode.Bounds.Tile.y)
                );
            Vector2Int uv = relativePosition / (int)bottomNode.Bounds.GetSize();

            // Set pixels of "brush"
            for (int x = uv.x; x < tex.width && x < (uv.x + brushSize); x++)
            {
                for (int y = uv.y; y < tex.height && y < (uv.y + brushSize); y++)
                {
                    tex.SetPixel(x, y, Color.red);
                }
            }

            tex.Apply();
            savingTasks.Add(bottomNode.SaveContent(this.FolderPath));
        }
        else
        {
            // Brush crosses into a neighbouring tile
            return;
        }

        // Carry painted changes over to parents
        LoadableGrassMQTNode node = bottomNode;
        Texture2D source = bottomNode.Content.exampleTexture;

        for (int mipLevel = 1; node.Parent != null; mipLevel++)
        {
            // Go up
            node = node.Parent;

            // If new node, create initial data for tile
            if (node.Content == null)
                node.Content = new GrassTileData(this.DataMapWidth, this.DataMapWidth);

            // Paint mipmap on corner
            Texture2D target = node.Content.exampleTexture;

            switch (node.RelativePosition)
            {
                case QuadNodePosition.NW:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(255, 255));
                    break;
                case QuadNodePosition.SW:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 255));
                    break;
                case QuadNodePosition.NE:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(255, 0));
                    break;
                case QuadNodePosition.SE:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 0));
                    break;
                default:
                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 0));
                    break;
            }

            // Save changes
            savingTasks.Add(node.SaveContent(this.FolderPath));
        }

        await Task.WhenAll(savingTasks);
    }

    private Texture2D PaintTextureOnTexture(ref Texture2D target, Texture2D source, int mipLevel, Vector2Int position)
    {
        Color[] targetPixels = target.GetPixels();
        Color[] sourcePixels = source.GetPixels();

        int multiplier = (int)Math.Pow(2, mipLevel);

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                int targetIndex = y * target.width + x;
                int sourceIndex = y * source.width * multiplier + x * multiplier; // Gaat sowieso buiten array range

                if (targetIndex < targetPixels.Length && sourceIndex < sourcePixels.Length)
                {
                    targetPixels[targetIndex] = sourcePixels[sourceIndex];
                }
                else
                {
                    Debug.Log("Source or target texture assumed to be 'too long'");
                }

                //target.SetPixel(
                //    x + position.x, 
                //    y + position.y, 
                //    target.GetPixel(x << mipLevel, y, mipLevel)
                //    );
            }
        }

        return target;
    }
}



/// <summary>
/// Node of LoadableGrassMQT
/// </summary>
public class LoadableGrassMQTNode : LoadableMQTNodeAbstract<GrassTileData, LoadableGrassMQTNode>
{
    public LoadableGrassMQTNode(Rect3D bounds)
        : base(bounds)
    { }

    public LoadableGrassMQTNode(LoadableGrassMQTNode parent, QuadNodePosition relativePosition)
        : base(parent, relativePosition)
    { }

    public override void GenerateNE()
    {
        this.NE = new LoadableGrassMQTNode(this, QuadNodePosition.NE);
    }
    public override void GenerateNW()
    {
        this.NW = new LoadableGrassMQTNode(this, QuadNodePosition.NW);
    }
    public override void GenerateSE()
    {
        this.SE = new LoadableGrassMQTNode(this, QuadNodePosition.SE);
    }
    public override void GenerateSW()
    {
        this.SW = new LoadableGrassMQTNode(this, QuadNodePosition.SW);
    }



    public async override Task LoadContent(string folderPath)
    {
        if (this.IsLoading)
            return;
        this.IsLoading = true;

        if (this.Content != null)
            return;

        string filePath = "file://" + folderPath + "/" + this.FileName;

        if (!this.IsSaved && !this.IsSaving && !File.Exists(filePath)) // Dont attempt to load non-existent data
        {
            this.Content = new GrassTileData(new Texture2D(512, 512)); // DONT HARDCODE THIS!!!
            this.IsLoaded = true;
            this.IsLoading = false;
            await this.SaveContent(folderPath);
            
            return;
        }

        Debug.Log("Load content: " + this.FileName);

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(filePath))
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Get the loaded texture
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                this.Content = new GrassTileData(texture);
            }
            else
            {
                this.IsLoaded = false;
                Debug.LogError("Texture loading failed: " + www.error);
            }
        }

        this.IsLoaded = true;
    }


    private SemaphoreSlim saveSemaphore = new SemaphoreSlim(1, 1);

    public async override Task SaveContent(string folderPath)
    {
        if (await saveSemaphore.WaitAsync(0))
        {
            try
            {
                this.IsSaving = true;

                byte[] textureData = this.Content.exampleTexture.GetRawTextureData();

                string saveFilePath = Path.Combine(folderPath, this.FileName);

                using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
                {
                    await fileStream.WriteAsync(textureData, 0, textureData.Length);
                }

                Debug.Log("Content saved successfully.");

                this.IsSaved = true;
                this.IsSaving = false;
            }
            catch (Exception ex)
            {
                Debug.LogError("Content save error: " + ex.Message);
            }
            finally
            {
                saveSemaphore.Release();
            }
        }
        else
        {
            Debug.LogError("Content save failed. Another operation is already in progress.");
        }
    }

    //public async override Task SaveContent(string folderPath)
    //{
    //    //if (!this.IsSaving)
    //    //    this.IsSaving = true;
    //    //else
    //    //    return;


    //    //// Get the raw texture data
    //    //byte[] textureData = this.Content.exampleTexture.GetRawTextureData();

    //    //// Save the raw texture data to a file
    //    //string saveFilePath = Path.Combine(folderPath, this.FileName);

    //    //using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
    //    //{
    //    //    await fileStream.WriteAsync(textureData, 0, textureData.Length);
    //    //}

    //    //this.IsSaved = true;
    //    //this.IsSaving = false;



    //}

    public override Task UnloadContent()
    {
        this.Content = null;
        this.IsLoaded = false;

        return Task.CompletedTask;
    }
}

