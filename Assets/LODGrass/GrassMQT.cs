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
    // Debug variables
    public GameObject testObject;

    //
    public bool doUpdateWithCamera = false;

    public const float SplitDistanceMultiplier = 2;
    public int DataMapWidth { get; private set; }

    // ! Change names with w_ and h_, since its confusing with naming conventions
    public LoadableGrassMQT(string folderPath, Vector3 position, Vector2 size, float detailMapDensity, int detailMapPixelWidth, GameObject testObject) 
        : base(folderPath)
    {
        this.testObject = testObject;

        // Calculate the sizes of nodes and create the root node
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
    public LoadableGrassMQT(string folderPath, Rect3D bounds, GameObject testObject, int maxDepth = 5) 
        : base(folderPath, bounds, maxDepth)
    {
        this.testObject = testObject;
    }

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
        // This method should probably only be executed one at a time, (since there is no point in trying to load stuff so quickly after one-another)
        // but still on a different thread to save time
    {
        // ___Unloading nodes___
        Task[] unloadingTasks = new Task[this.nodesToUnload.Count];
        for (int i = 0; i < this.nodesToUnload.Count; i++)
        {
            unloadingTasks[i] = this.nodesToUnload[i].UnloadContent();
        }

        await Task.WhenAll(unloadingTasks);
        this.nodesToUnload.Clear();

        // ___Loading nodes___
        this.nodesToLoad.Clear();

        if (!this.doUpdateWithCamera) // could maybe be placed before the previous line
            return;

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
    }

    private void UpdateNodeToLoad(LoadableGrassMQTNode node, Vector3 cameraPosition)
    {
        if (node == null)
            return;

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

                if(!this.nodesToUnload.Contains(node) && node.IsLoaded)
                    this.nodesToUnload.Add(node);

                return;
            }

            if(!node.IsLoaded && !node.IsLoading) // might be unnecessary since also being checked l
                this.nodesToLoad.Add(node);
            return;
        }

        if (nodesToLoad.Count == 0 && node.Parent == null)
        {
            if (!node.IsLoaded && !node.IsLoading) // might be unnecessary since also being checked l
                this.nodesToLoad.Add(node);
            return;
        }

        float distanceToParent = Vector3.Distance(node.Parent.Bounds.GetCenterPosition(), cameraPosition);

        // Node is too far away -> render parent
        if (distanceToParent >= SplitDistanceMultiplier * node.Parent.Bounds.GetSize())
        {
            // GAAT NIET GOED HIER MET UITLADEN VAN NODES

            if (!node.Parent.IsLoaded && !node.IsLoading) // might be unnecessary since also being checked l
            {
                if (!this.nodesToLoad.Contains(node.Parent))
                    this.nodesToLoad.Add(node.Parent);

                if(node.IsLoaded && !this.nodesToUnload.Contains(node))
                    this.nodesToUnload.Add(node);
            }

            return;
        }

        // Node is fine as it is
        // !why add to nodesToLoad if already fine?!
        if (!node.IsLoaded && !node.IsLoading) // might be unnecessary since also being checked l
            this.nodesToLoad.Add(node);

        return;
    }

    // brushSize naar world size veranderen
    public async void PaintGrass(Vector3 brushWorldPosition, int brushSize, int grassTypeIndex)
    {
        // Dont do anything if brush is not on terrain
        if (!this.Root.Bounds.IsPointInTile(brushWorldPosition))
        {
            Debug.Log("Brush is outside the root node!");
            return;
        }

        //List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> nodesToPaint = new List<QuadTreeNode<LoadableStructContainer<GrassTileData>>>();

        // __Paint on bottom node__
        // Get the bottom node at position
        LoadableGrassMQTNode bottomNode = this.GenerateToBottomTileAtPosition(brushWorldPosition);
        if (bottomNode == null)
        {
            Debug.Log("No bottom node possible/found!");
            return;
        }

        // Load nodes
        await bottomNode.LoadAndUp(this.FolderPath);

        List<Task> savingTasks = new List<Task>();

        // Check whether or not the brush crosses into neighbouring tile
        Rect brushBounds = new Rect(brushWorldPosition.x, brushWorldPosition.z, brushSize, brushSize);

        if (bottomNode.Bounds.IsRectOnlyInTile(brushBounds))
        { // Brush is only in this tile
            //GameObject.Instantiate(testObject, brushWorldPosition, Quaternion.identity); // Debug

            // If new node, create initial data for tile
            if (bottomNode.Content == null)
                bottomNode.Content = new GrassTileData(this.DataMapWidth, this.DataMapWidth);

            Texture2D tex = bottomNode.Content.exampleTexture;

            // Convert to UV space
            Vector2Int relativePosition = new Vector2Int(
                (int)(brushWorldPosition.x - bottomNode.Bounds.Tile.x),
                (int)(brushWorldPosition.z - bottomNode.Bounds.Tile.y)
                );
            Vector2Int uv = (relativePosition * this.DataMapWidth) / (int)bottomNode.Bounds.GetSize();

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

        // __Carry painted changes over to parents__
        // NOTE: better to do this with just the brush and not the entire texture
        Texture2D source = bottomNode.Content.exampleTexture;

        LoadableGrassMQTNode sourceNode = bottomNode;
        Vector2Int position = new Vector2Int(0, 0);
        for (int mipLevel = 1; sourceNode.Parent != null; mipLevel++)
        {
            LoadableGrassMQTNode targetNode = sourceNode.Parent;

            // If new node, create initial data for tile
            if (targetNode.Content == null)
                targetNode.Content = new GrassTileData(this.DataMapWidth, this.DataMapWidth);

            // Paint mipmap on corner
            Texture2D target = targetNode.Content.exampleTexture;
            int sourceRelativeSize = this.DataMapWidth / 2; 
            position = position / 2;
            switch (sourceNode.RelativePosition)
            {
                case QuadNodePosition.NW:
                    position += new Vector2Int(0, sourceRelativeSize - 1);
                    PaintTextureOnTexture(ref target, source, mipLevel, position); //new Vector2Int(0, 255));
                    break;
                case QuadNodePosition.SW:
                    // position += vec(0,0)
                    PaintTextureOnTexture(ref target, source, mipLevel, position); //new Vector2Int(0, 0));
                    break;
                case QuadNodePosition.NE:
                    position += new Vector2Int(sourceRelativeSize - 1, sourceRelativeSize - 1);
                    PaintTextureOnTexture(ref target, source, mipLevel, position); //new Vector2Int(255, 255));
                    break;
                case QuadNodePosition.SE:
                    position += new Vector2Int(sourceRelativeSize - 1, 0);
                    PaintTextureOnTexture(ref target, source, mipLevel, position); //new Vector2Int(255, 0));
                    break;
                default:
                    PaintTextureOnTexture(ref target, source, mipLevel, position); //new Vector2Int(0, 0));
                    break;
            }

            // Save changes
            savingTasks.Add(targetNode.SaveContent(this.FolderPath));

            // Go up
            sourceNode = sourceNode.Parent;
        }

        await Task.WhenAll(savingTasks);

    }

    private Texture2D PaintTextureOnTexture(ref Texture2D target, Texture2D source, int mipLevel, Vector2Int position)
    {
        Color32[] sourcePixels = source.GetPixels32(mipLevel);
        Color32[] targetPixels = target.GetPixels32();

        int sourceSize = source.width / (1 << mipLevel);

        for (int y = 0; y < sourceSize; y++)
        {
            for (int x = 0; x < sourceSize; x++)
            {
                int sourceIndex = (y * sourceSize) + x;
                int targetIndex = ((y + position.y) * target.width) + x + position.x;
                targetPixels[targetIndex] = sourcePixels[sourceIndex];
            }
        }

        target.SetPixels32(targetPixels);
        target.Apply();

        return target;
    }
}



/// <summary>
/// Node of LoadableGrassMQT
/// </summary>
public class LoadableGrassMQTNode : LoadableMQTNodeAbstract<GrassTileData, LoadableGrassMQTNode>
{
    // Debug vars
    GameObject quadVisual;

    public LoadableGrassMQTNode(Rect3D bounds)
        : base(bounds)
    {
        this.CreateQuadRepresentation(); // debug
    }

    public LoadableGrassMQTNode(LoadableGrassMQTNode parent, QuadNodePosition relativePosition)
        : base(parent, relativePosition)
    {
        this.CreateQuadRepresentation(); // debug
    }

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
        {
            this.IsLoading = false;
            return;
        }

        string filePath = Path.Combine(folderPath, this.FileName);
        string webFilePath = "file://" + filePath;

        if (!this.IsSaved) /*  && !this.IsSaving && */ // Dont attempt to load non-existent data
        {
            if (File.Exists(filePath))
                this.IsSaved = true;
            else
            {
                this.IsLoading = false;
                return;
            }
            //this.Content = new GrassTileData(new Texture2D(512, 512)); // DONT HARDCODE THIS!!!
            //this.IsLoaded = true;
            //this.IsLoading = false;
            //await this.SaveContent(folderPath);
            
            //return;
        }

        Debug.Log("Load content: " + this.FileName);

        using (UnityWebRequest www = UnityWebRequest.Get(webFilePath)) //UnityWebRequestTexture.GetTexture(filePath))
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Get the loaded texture
                //Texture2D texture = DownloadHandlerTexture.GetContent(www);

                Texture2D texture = new Texture2D(512, 512);
                texture.LoadImage(www.downloadHandler.data);
                //texture.LoadRawTextureData(www.downloadHandler.data);
                texture.Apply();

                this.Content = new GrassTileData(texture);
                this.IsLoaded = true;
            }
            else
            {
                this.IsLoaded = false;

                Debug.LogError("Texture loading failed: " + www.error);
                Debug.LogError("DownloadHandler error: " + www.downloadHandler.error);
            }
        }

        this.IsLoading = false;

        this.UpdateQuadRepresentation(); // debug

        return;
    }


    //private SemaphoreSlim saveSemaphore = new SemaphoreSlim(1, 1);
    private readonly object saveLock = new object();


    // ChatGPT version 1
    public async override Task SaveContent(string folderPath)
    {
        bool lockAcquired = false;

        try
        {
            Monitor.TryEnter(saveLock, ref lockAcquired);

            if (!lockAcquired)
            {
                Debug.Log("Already saving!");
                throw new InvalidOperationException("Save operation is already in progress.");
            }

            this.IsSaving = true;

            //byte[] textureData = this.Content.exampleTexture.GetRawTextureData();
            byte[] textureData = this.Content.exampleTexture.EncodeToPNG();

            string saveFilePath = Path.Combine(folderPath, this.FileName);

            Debug.Log($"Writing to file: {saveFilePath}");
            await using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
            {
                fileStream.Write(textureData, 0, textureData.Length);
                //await fileStream.WriteAsync(textureData, 0, textureData.Length);
            }
            Debug.Log($"Write to file completed: {saveFilePath}");

            Debug.Log("Content saved successfully.");
            this.IsSaved = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Content save error: " + ex.Message);
        }
        finally
        {
            if (lockAcquired)
            {
                Monitor.Exit(saveLock);
            }

            this.IsSaving = false;
        }
    }

    //public async override Task SaveContent(string folderPath)
    //{
    //    // This could make it faster, since it most of method calls wont reach the lock
    //    //if (this.IsSaving)
    //    //    return;

    //    lock (saveLock)
    //    {
    //        if (this.IsSaving)
    //        {
    //            Debug.Log("Already saving!");
    //            throw new InvalidOperationException("Save operation is already in progress.");
    //            return;
    //        }

    //        this.IsSaving = true;
    //    }

    //    try
    //    {
    //        byte[] textureData = this.Content.exampleTexture.GetRawTextureData();

    //        string saveFilePath = Path.Combine(folderPath, this.FileName);

    //        using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
    //        {
    //            await fileStream.WriteAsync(textureData, 0, textureData.Length);
    //            fileStream.Close();
    //        }

    //        Debug.Log("Content saved successfully.");

    //        this.IsSaved = true;
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError("Content save error: " + ex.Message);
    //    }
    //    finally
    //    {
    //        lock(saveLock) // Lock might not be necesarry here, since there shouldnt be any race conditions
    //        {
    //            this.IsSaving = false;
    //        }
    //    }
    //}

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

    private void CreateQuadRepresentation() // Debug method
    {
        this.quadVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        this.quadVisual.name = this.FileName;
        this.quadVisual.transform.position = this.Bounds.GetCenterPosition() + Vector3.up * (this.Layer + 1);
        this.quadVisual.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        this.quadVisual.transform.localScale = Vector3.one * this.Bounds.GetSize();

        if(this.Content != null)
            this.quadVisual.GetComponent<Renderer>().material.SetTexture("_MainTex", this.Content.exampleTexture);
    }

    private void UpdateQuadRepresentation() // Debug method
    {
        if(this.Content != null)
            this.quadVisual.GetComponent<Renderer>().material.SetTexture("_MainTex", this.Content.exampleTexture);
    }

    public void DeleteQuadRepresentation()
    {
        GameObject.Destroy(this.quadVisual);
    }
}

