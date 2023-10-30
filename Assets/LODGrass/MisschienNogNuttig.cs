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



//public class QuadTreeNode<TContent> //: IQuadTreeNode
//{
//    public QuadTreeNode<TContent> Parent { get; protected set; }

//    public QuadTreeNode<TContent> BottomRight { get; protected set; }
//    public QuadTreeNode<TContent> BottomLeft { get; protected set; }
//    public QuadTreeNode<TContent> TopRight { get; protected set; }
//    public QuadTreeNode<TContent> TopLeft { get; protected set; }

//    public Rect3D Tile { get; set; }

//    public TContent Content { get; protected set; }

//    public UInt32 Index { get; protected set; } // Describes Layer and RelativePosition, so maybe redundant
//    public int Layer { get; protected set; }
//    public QuadNodePosition RelativePosition { get; protected set; }
//    public bool HasChildren { get; protected set; }

//    public QuadTreeNode(QuadNodePosition relativePosition, float size, QuadTreeNode<TContent> parent)
//    {
//        this.Parent = parent;
//        this.Parent.HasChildren = true;
//        this.Layer = parent.Layer + 1; // Probably bad practice to extract variable from a variable that was passed for a different reason
//        //UpdateTreeDepth(this.Layer);
//        this.RelativePosition = relativePosition;

//        this.Tile = new Rect3D(parent.Tile.GetPosition(), relativePosition, size);

//        GenerateIndex(parent.Index, this.Layer, relativePosition);
//    }

//    /// <summary>
//    /// Root node constructor
//    /// </summary>
//    /// <param name="position"></param>
//    /// <param name="size"></param>
//    public QuadTreeNode(Vector3 position, float size)
//    {
//        this.Parent = null;
//        this.Tile = new Rect3D(position, size);

//        this.Layer = 0;
//        this.Index = 0;
//    }

//    protected void GenerateIndex(UInt32 parentIndex, int layer, QuadNodePosition relativePosition)
//    {
//        this.Index = ((uint)relativePosition << (layer * 2)) | parentIndex;
//    }

//    public static string ConvertIndexToString(UInt32 index)
//    {
//        byte[] bytes = BitConverter.GetBytes(index);
//        return Convert.ToBase64String(bytes);
//    }

//    public void GenerateBottomRight()
//    {
//        float size = this.Tile.GetSize() / 2;
//        this.BottomRight = new QuadTreeNode<TContent>(QuadNodePosition.BottomRight, size, this);
//    }
//    public void GenerateBottomLeft()
//    {
//        float size = this.Tile.GetSize() / 2;
//        this.BottomLeft = new QuadTreeNode<TContent>(QuadNodePosition.BottomLeft, size, this);
//    }
//    public void GenerateTopRight()
//    {
//        float size = this.Tile.GetSize() / 2;
//        this.TopRight = new QuadTreeNode<TContent>(QuadNodePosition.TopRight, size, this);
//    }
//    public void GenerateTopLeft()
//    {
//        float size = this.Tile.GetSize() / 2;
//        this.TopLeft = new QuadTreeNode<TContent>(QuadNodePosition.TopLeft, size, this);
//    }

//    public void GenerateAllChildren()
//    {
//        GenerateBottomRight();
//        GenerateBottomLeft();
//        GenerateTopRight();
//        GenerateTopLeft();
//    }

//    public void ExpandNode(int maxDepth, int layers)
//    {
//        if (this.Layer >= maxDepth || layers < 1)
//            return;

//        layers--;

//        this.GenerateBottomLeft();
//        this.BottomLeft.ExpandNode(maxDepth, layers);
//        this.GenerateBottomRight();
//        this.BottomRight.ExpandNode(maxDepth, layers);
//        this.GenerateTopLeft();
//        this.TopLeft.ExpandNode(maxDepth, layers);
//        this.GenerateTopRight();
//        this.TopRight.ExpandNode(maxDepth, layers);
//    }

//    public void DrawTiles(int depth)
//    {
//        if (this.Layer > depth)
//            return;

//        if (this.BottomLeft != null)
//            this.BottomLeft.DrawTiles(depth);
//        if (this.BottomRight != null)
//            this.BottomRight.DrawTiles(depth);
//        if (this.TopLeft != null)
//            this.TopLeft.DrawTiles(depth);
//        if (this.TopRight != null)
//            this.TopRight.DrawTiles(depth);

//        this.Tile.DrawTile(Color.blue);
//    }
//}



//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using UnityEngine;

//public class LoadableQuadTree<TContainer, TData> : QuadTree<TContainer>
//    where TContainer : LoadableStructContainer<TData>
//    where TData : struct
//{
//    public string FolderPath { get; private set; }

//    public List<QuadTreeNode<LoadableStructContainer<TData>>> LoadedNodes { get; protected set; }
//    protected List<QuadTreeNode<LoadableStructContainer<TData>>> nodesToLoad = new List<QuadTreeNode<LoadableStructContainer<TData>>>();


//    public LoadableQuadTree(string folderPath) : base()
//    {
//        if (SetupFolder(folderPath))
//            this.FolderPath = folderPath;
//    }

//    public LoadableQuadTree(string folderPath, Vector3 position, float size) : base(position, size)
//    {
//        if (SetupFolder(folderPath))
//            this.FolderPath = folderPath;
//    }

//    private bool SetupFolder(string folderPath)
//    {
//        DirectoryInfo folder = Directory.CreateDirectory(folderPath);
//        if (folder == null)
//            return false;

//        return true;
//    }

//    public override void ExpandTree(int maxDepth, int layers) // This should not work like this, but due to generic classes and access modifiers I cant make the nodes responsible for instantiating their own content/fileName...
//    {
//        ExpandNode(this.Root, maxDepth, layers);
//    }

//    private void ExpandNode(QuadTreeNode<TContainer> node, int maxDepth, int layers)
//    {
//        if (layers < 1 || node == null || node.Layer >= maxDepth)
//            return;

//        NameFileNode(node);

//        layers--;
//        node.GenerateAllChildren();
//        ExpandNode(node.BottomLeft, maxDepth, layers);
//        ExpandNode(node.BottomRight, maxDepth, layers);
//        ExpandNode(node.TopLeft, maxDepth, layers);
//        ExpandNode(node.TopRight, maxDepth, layers);
//    }

//    private void NameFileNode(QuadTreeNode<TContainer> node) // Wat kut zeg
//    {
//        string fileName = QuadTreeNode<TContainer>.ConvertIndexToString(node.Index);
//        node.BottomLeft.Content.SetFileName(fileName);
//    }

//    public void Insert()
//    {

//    }

//    public async Task LoadNodeAndUp(QuadTreeNode<TContainer> node) // List naar array aanpassen wnr Depth werkt
//    {
//        if (node == null)
//            return;

//        // Load this node and up
//        List<Task> loadingTasks = new List<Task>();
//        while (node != null)
//        {
//            loadingTasks.Add(node.Content.LoadData(this.FolderPath));

//            node = node.Parent;
//        }

//        if (loadingTasks.Count == 0)
//            return;

//        await Task.WhenAll(loadingTasks);
//    }
//}







//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//public static class LoadableQuadTreeGenerator<U, T> where U : struct where T : LoadableQuadTreeNode<U, T>
//{
///// <summary>
///// 
///// </summary>
///// <param name="folderPath"></param>
///// <param name="canvas"></param>
///// <param name="detailMapDensity"> Pixels per meter^2 </param>
///// <param name="detailMapPixelWidth"></param>
///// <param name="maxStoredPixels"></param>
///// <returns></returns>
//public static LoadableQuadTree<U, T> GenerateLoadableQuadTree(string folderPath, Terrain canvas, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels)
//{
//    Vector3 position = canvas.transform.position;

//    float w_canvas = canvas.terrainData.size.x;
//    float h_canvas = canvas.terrainData.size.z;

//    float w_detailMap = Mathf.Sqrt(detailMapDensity);
//    float w_totalPixels = w_canvas * w_detailMap;
//    float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

//    int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2) + 0.5);

//    float w_smallTile = detailMapPixelWidth / w_detailMap;

//    float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

//    // Safeguard to avoid absurd memory/diskspace usage
//    bool isTooLarge = IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
//    if (isTooLarge)
//        throw new Exception("Resulting Tile Tree will be too large!");

//    LoadableQuadTree<U, T> tree = new LoadableQuadTree<U, T>(folderPath, position, w_rootSize);

//    return tree;
//}

//    /// <summary>
//    /// This method assumes you fill the entire canvas with the smallest types of LODTiles.
//    /// </summary>
//    /// <param name="maxSize"> Maximum acceptable number of pixels stored in this tree </param>
//    /// <param name="w_canvasSize"> Terrain width </param>
//    /// <returns> Returns true if the maximum size of a LODTile tree is too large for memory</returns>
//    public static bool IsTreeMemoryTooLarge(double maxSize, float w_canvasSize, float h_canvasSize, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels)
//    {
//        // Calculate number of tiles in each direction
//        int w_noTiles = (int)Math.Ceiling((double)(w_canvasSize / w_minTileSize));
//        int h_noTiles = (int)Math.Ceiling((double)(h_canvasSize / h_minTileSize));

//        // Calculate number of smallest tiles
//        int noSmallTiles = w_noTiles * h_noTiles;

//        // Calculate total number of tiles
//        int noTotalTiles = noSmallTiles;
//        while (noSmallTiles > 4)
//        {
//            noSmallTiles = (int)Math.Ceiling((double)(noSmallTiles / 4));
//            noTotalTiles += noSmallTiles;
//        }

//        // Add 1 for the root
//        noTotalTiles++;

//        // Total number of pixels
//        double noTotalPixels = noTotalTiles * w_pixels * h_pixels;

//        Debug.Log("Total Pixels: " + noTotalPixels);

//        return noTotalPixels > maxSize;
//    }
//}




//public class GrassQuadTree : LoadableQuadTree<LoadableStructContainer<GrassTileData>, GrassTileData>
//{
//    public const float SplitDistanceMultiplier = 2;
//    public int DataMapWidth { get; private set; }

//    public GrassQuadTree(string folderPath, Vector3 position, Vector2 size, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels) 
//        : base(folderPath)
//    {
//        float w_canvas = size.x;
//        float h_canvas = size.y;

//        float w_detailMap = Mathf.Sqrt(detailMapDensity);
//        float w_totalPixels = w_canvas * w_detailMap;
//        float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

//        int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2));// + 0.5);

//        float w_smallTile = detailMapPixelWidth / w_detailMap;

//        float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

//        //// Safeguard to avoid absurd memory/diskspace usage (NOT NEEDED, SINCE THE DETAIL NEEDS TO BE PAINTED LATER)
//        //bool isTooLarge = GrassQuadTree.IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
//        //if (isTooLarge)
//        //{
//        //    Debug.Log("Resulting tree will be too big!");
//        //    throw new Exception("Resulting Tile Tree will be too large!");
//        //}

//        this.MaxDepth = noLayers;
//        this.DataMapWidth = detailMapPixelWidth;

//        this.GenerateRoot(position, w_rootSize);
//    }

//    public override void GenerateRoot(Vector3 position, float size)
//    {
//        this.Root = new QuadTreeNode<LoadableStructContainer<GrassTileData>>(position, size);
//        this.Depth = 1;
//        //this.LoadedNodes.Add(this.Root);
//        //this.NodesToRender.Add(this.Root);
//    }

//    /// <summary>
//    /// This method assumes you fill the entire canvas with the smallest types of LODTiles.
//    /// </summary>
//    /// <param name="maxSize"> Maximum acceptable number of pixels stored in this tree </param>
//    /// <param name="w_canvasSize"> Terrain width </param>
//    /// <returns> Returns true if the maximum size of a LODTile tree is too large for memory</returns>
//    public static bool IsTreeMemoryTooLarge(double maxSize, float w_canvasSize, float h_canvasSize, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels)
//    {
//        // Calculate number of tiles in each direction
//        int w_noTiles = (int)Math.Ceiling((double)(w_canvasSize / w_minTileSize));
//        int h_noTiles = (int)Math.Ceiling((double)(h_canvasSize / h_minTileSize));

//        // Calculate number of smallest tiles
//        int noSmallTiles = w_noTiles * h_noTiles;

//        // Calculate total number of tiles
//        int noTotalTiles = noSmallTiles;
//        while (noSmallTiles > 4)
//        {
//            noSmallTiles = (int)Math.Ceiling((double)(noSmallTiles / 4));
//            noTotalTiles += noSmallTiles;
//        }

//        // Add 1 for the root
//        noTotalTiles++;
//        Debug.Log(noTotalTiles);


//        // Total number of pixels
//        double noTotalPixels = noTotalTiles * w_pixels * h_pixels;

//        Debug.Log("Total Pixels: " + noTotalPixels);

//        return noTotalPixels > maxSize;
//    }


//    //public override async Task UpdateLoaded()
//    //{
//    //    throw new InvalidOperationException("Requires Camera (position) to check for needed changes to loaded tiles");
//    //}

//    /// <summary>
//    /// Update the list of loaded nodes
//    /// </summary>
//    public async void UpdateLoaded(Vector3 cameraPosition)
//    {
//        this.nodesToLoad.Clear();

//        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
//        foreach (QuadTreeNode<LoadableStructContainer<GrassTileData>> node in this.LoadedNodes)
//        {
//            UpdateNodeToLoad(node, cameraPosition);
//        }

//        // Load all found nodes that need loading
//        Task[] loadingTasks = new Task[this.nodesToLoad.Count];
//        for (int i = 0; i < this.nodesToLoad.Count; i++)
//        {
//            loadingTasks[i] = this.nodesToLoad[i].Content.LoadData(this.FolderPath);
//        }

//        await Task.WhenAll(loadingTasks);

//        // DEBUG: 
//        foreach(QuadTreeNode<LoadableStructContainer<GrassTileData>> node in this.nodesToLoad)
//        {
//            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
//            quad.transform.position = node.Tile.GetPosition();
//            quad.transform.rotation = Quaternion.Euler(Vector3.right);

//            quad.GetComponent<Renderer>().material.SetTexture("_MainTex", node.Content.Data.Value.exampleTexture); 
//        }
//    }

//    private bool UpdateNodeToLoad(QuadTreeNode<LoadableStructContainer<GrassTileData>> node, Vector3 cameraPosition)
//    {
//        if (node == null)
//            return false;

//        float distance = Vector3.Distance(node.Tile.GetCenterPosition(), cameraPosition);

//        // Node might be too close
//        if (distance < SplitDistanceMultiplier * node.Tile.GetSize())
//        {
//            // Node is too close -> Split node
//            if (node.HasChildren)
//            {
//                UpdateNodeToLoad(node.BottomLeft, cameraPosition);
//                UpdateNodeToLoad(node.BottomRight, cameraPosition);
//                UpdateNodeToLoad(node.TopLeft, cameraPosition);
//                UpdateNodeToLoad(node.TopRight, cameraPosition);

//                return true;
//            }

//            this.nodesToLoad.Add(node);
//            return true;
//        }

//        if (node.Parent == null)
//        {
//            this.nodesToLoad.Add(node);
//            return true;
//        }

//        float distanceToParent = Vector3.Distance(node.Parent.Tile.GetCenterPosition(), cameraPosition);

//        // Node is too far away -> render parent
//        if (distanceToParent >= SplitDistanceMultiplier * node.Parent.Tile.GetSize())
//        {
//            if (this.nodesToLoad.Contains(node.Parent))
//                return true;

//            this.nodesToLoad.Add(node.Parent);
//            return true;
//        }

//        // Node is fine as it is
//        this.nodesToLoad.Add(node);
//        return true;
//    }

//    // brushSize naar world size veranderen
//    public async void PaintGrass(Vector3 brushWorldPosition, int brushSize, int grassTypeIndex )
//    {
//        // Dont do anything if brush is not on terrain
//        if (!this.Root.Tile.IsPointInTile(brushWorldPosition))
//            return;

//        //List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> nodesToPaint = new List<QuadTreeNode<LoadableStructContainer<GrassTileData>>>();

//        // Get the bottom node at position
//        QuadTreeNode<LoadableStructContainer<GrassTileData>> bottomNode = this.GenerateToBottomTileAtPosition(brushWorldPosition);
//        if (bottomNode == null)
//            return;

//        // Load nodes
//        await LoadNodeAndUp(bottomNode);

//        List<Task> savingTasks = new List<Task>();

//        // Check whether or not the brush crosses into neighbouring tile
//        Rect brushBounds = new Rect(brushWorldPosition.x, brushWorldPosition.y, brushSize, brushSize);

//        if (bottomNode.Tile.IsRectOnlyInTile(brushBounds))
//        { // Brush is only in this tile
//            // If new node, create initial data for tile
//            if (bottomNode.Content.Data == null)
//                bottomNode.Content.Data = new GrassTileData(this.DataMapWidth, this.DataMapWidth);

//            Texture2D tex = bottomNode.Content.Data.Value.exampleTexture;

//            // Convert to UV space
//            Vector2Int relativePosition = new Vector2Int(
//                (int)(brushWorldPosition.x - bottomNode.Tile.Tile.x),
//                (int)(brushWorldPosition.z - bottomNode.Tile.Tile.y)
//                );
//            Vector2Int uv = relativePosition / (int)bottomNode.Tile.GetSize();

//            // Set pixels of "brush"
//            for (int x = uv.x; x < tex.width && x < (uv.x + brushSize); x++)
//            {
//                for (int y = uv.y; y < tex.height && y < (uv.y + brushSize); y++)
//                {
//                    tex.SetPixel(x, y, Color.red);
//                }
//            }

//            tex.Apply();
//            savingTasks.Add(bottomNode.Content.SaveData(this.FolderPath));
//        }
//        else
//        {
//            // Brush crosses into a neighbouring tile
//            return;
//        }

//        // Carry painted changes over to parents
//        QuadTreeNode<LoadableStructContainer<GrassTileData>> node = bottomNode;
//        Texture2D source = bottomNode.Content.Data.Value.exampleTexture;

//        for(int mipLevel = 1; node.Parent != null; mipLevel++)
//        {
//            // Go up
//            node = node.Parent;

//            // If new node, create initial data for tile
//            if (node.Content.Data == null)
//                node.Content.Data = new GrassTileData(this.DataMapWidth, this.DataMapWidth);

//            // Paint mipmap on corner
//            Texture2D target = node.Content.Data.Value.exampleTexture;

//            switch (node.RelativePosition)
//            {
//                case QuadNodePosition.TopRight:
//                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(255, 255));
//                    break;
//                case QuadNodePosition.TopLeft:
//                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 255));
//                    break;
//                case QuadNodePosition.BottomRight:
//                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(255, 0));
//                    break;
//                case QuadNodePosition.BottomLeft:
//                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 0));
//                    break;
//                default:
//                    PaintTextureOnTexture(ref target, source, mipLevel, new Vector2Int(0, 0));
//                    break;
//            }

//            // Save changes
//            savingTasks.Add(node.Content.SaveData(this.FolderPath));
//        }

//        await Task.WhenAll(savingTasks);
//    }

//    private Texture2D PaintTextureOnTexture(ref Texture2D target, Texture2D source, int mipLevel, Vector2Int position)
//    {
//        Color[] targetPixels = target.GetPixels();
//        Color[] sourcePixels = source.GetPixels();

//        int multiplier = (int)Math.Pow(2, mipLevel);

//        for (int y = 0; y < source.height; y++)
//        {
//            for (int x = 0; x < source.width; x++)
//            {
//                int targetIndex = y * target.width + x;
//                int sourceIndex = y * source.width * multiplier + x * multiplier; // Gaat sowieso buiten array range

//                if (targetIndex < targetPixels.Length && sourceIndex < sourcePixels.Length)
//                {
//                    targetPixels[targetIndex] = sourcePixels[sourceIndex];
//                }
//                else
//                {
//                    Debug.Log("Source or target texture assumed to be 'too long'");
//                }

//                //target.SetPixel(
//                //    x + position.x, 
//                //    y + position.y, 
//                //    target.GetPixel(x << mipLevel, y, mipLevel)
//                //    );
//            }
//        }

//        return target;
//    }

//    private void PaintRect()
//    {

//    }
//}




//string filePath = Path.Combine(folderPath, this.FileName);

//if (File.Exists(filePath))
//{
//    byte[] fileData = null;
//    await Task.Run(() =>
//    {
//        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
//        {
//            fileData = new byte[fileStream.Length];
//            fileStream.Read(fileData, 0, (int)fileStream.Length);
//        }
//    });

//    if (fileData != null)
//    {
//        Texture2D texture = new Texture2D(2, 2); // Set the initial size to your preference
//        bool loadSuccess = texture.LoadImage(fileData);

//        if (loadSuccess)
//        {
//            return texture;
//        }
//        else
//        {
//            Debug.LogError("Failed to load texture from file.");
//            return null;
//        }
//    }
//}
//else
//{
//    Debug.LogError("File not found: " + filePath);
//}
//return null;