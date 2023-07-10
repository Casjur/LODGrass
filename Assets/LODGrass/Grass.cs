using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Grass : MonoBehaviour
{
    // 
    public const bool enableEditing = false;

    // Generation input variables
    [SerializeField]
    string folderPath = "Assets/GrassData";
    [SerializeField]
    Terrain terrain;
    [SerializeField]
    int detailMapDensity = 4;
    [SerializeField]
    int detailMapPixelWidth = 512;
    [SerializeField]
    double maxStoredPixels = 357826560;
    [SerializeField]
    float grassDensity = 8;

    //
    [SerializeField]
    private Camera camera;

    // Contents
    public GrassQuadTree GrassData { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        string fullFolderPath = Application.dataPath + folderPath;
        Vector2 terrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);

        this.GrassData = new GrassQuadTree(
            fullFolderPath, 
            terrain.GetPosition(), 
            terrainSize, 
            detailMapDensity, 
            detailMapPixelWidth, 
            maxStoredPixels
            );

        // Test expansion
        this.GrassData.ExpandTree(this.GrassData.MaxDepth);
    }

    // Update is called once per frame
    void Update()
    {
        this.GrassData.UpdateNodesToRender(this.camera.transform.position);
        //this.GrassData.DrawAllTiles();
        //this.GrassData.DrawTilesToRender();
    }

    private void Render()
    {

    }
}

public class GrassQuadTree : LoadableQuadTree<GrassTileData, GrassDataContainer, GrassQuadTreeNode>
{
    public List<GrassQuadTreeNode> NodesToRender { get; protected set; } = new List<GrassQuadTreeNode>();


    public GrassQuadTree(string folderPath, Vector3 position, Vector2 size, float detailMapDensity, int detailMapPixelWidth, double maxStoredPixels) 
        : base(folderPath)
    {
        float w_canvas = size.x;
        float h_canvas = size.y;

        float w_detailMap = Mathf.Sqrt(detailMapDensity);
        float w_totalPixels = w_canvas * w_detailMap;
        float w_minNoTiles = w_totalPixels / detailMapPixelWidth;

        int noLayers = (int)Math.Ceiling(Math.Log(w_minNoTiles, 2) + 0.5);

        float w_smallTile = detailMapPixelWidth / w_detailMap;

        float w_rootSize = Mathf.Pow(2, noLayers) * w_smallTile;

        // Safeguard to avoid absurd memory/diskspace usage
        bool isTooLarge = GrassQuadTree.IsTreeMemoryTooLarge(maxStoredPixels, w_canvas, h_canvas, w_smallTile, w_smallTile, detailMapPixelWidth, detailMapPixelWidth);
        if (isTooLarge)
            throw new Exception("Resulting Tile Tree will be too large!");

        this.MaxDepth = noLayers;
        this.GenerateRoot(position, w_rootSize);
    }

    Vector2 GetSize()
    {
        return new Vector2(1.2f, 0.1f);
    }

    public override void GenerateRoot(Vector3 position, float size)
    {
        this.Root = new GrassQuadTreeNode(position, size);
        this.Depth = 1;
        //this.LoadedNodes.Add(this.Root);
        this.NodesToRender.Add(this.Root);
    }

    protected override GrassQuadTreeNode CreateRootNode(Vector3 position, float size)
    {
        GrassQuadTreeNode node = new GrassQuadTreeNode(position, size);
        return node;
    }

    /// <summary>
    /// This method assumes you fill the entire canvas with the smallest types of LODTiles.
    /// </summary>
    /// <param name="maxSize"> Maximum acceptable number of pixels stored in this tree </param>
    /// <param name="w_canvasSize"> Terrain width </param>
    /// <returns> Returns true if the maximum size of a LODTile tree is too large for memory</returns>
    public static bool IsTreeMemoryTooLarge(double maxSize, float w_canvasSize, float h_canvasSize, float w_minTileSize, float h_minTileSize, int w_pixels, int h_pixels)
    {
        // Calculate number of tiles in each direction
        int w_noTiles = (int)Math.Ceiling((double)(w_canvasSize / w_minTileSize));
        int h_noTiles = (int)Math.Ceiling((double)(h_canvasSize / h_minTileSize));

        // Calculate number of smallest tiles
        int noSmallTiles = w_noTiles * h_noTiles;

        // Calculate total number of tiles
        int noTotalTiles = noSmallTiles;
        while (noSmallTiles > 4)
        {
            noSmallTiles = (int)Math.Ceiling((double)(noSmallTiles / 4));
            noTotalTiles += noSmallTiles;
        }

        // Add 1 for the root
        noTotalTiles++;

        // Total number of pixels
        double noTotalPixels = noTotalTiles * w_pixels * h_pixels;

        Debug.Log("Total Pixels: " + noTotalPixels);

        return noTotalPixels > maxSize;
    }

    public void ExpandTree(int layers)
    {
        if(layers < 1)
            return;
        
        this.Root.ExpandNode(layers);
    }

    public virtual void UpdateNodesToRender(Vector3 cameraPosition)
    {
        Debug.Log("NoNodesToRender: " + this.NodesToRender.Count);

        GrassQuadTreeNode[] nodesToRenderCopy = new GrassQuadTreeNode[this.NodesToRender.Count];
        this.NodesToRender.CopyTo(nodesToRenderCopy);
        this.NodesToRender = new List<GrassQuadTreeNode>(); // Bad idea (extra costs)

        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
        foreach(GrassQuadTreeNode node in nodesToRenderCopy)
        {
            UpdateNodeToRender(node, cameraPosition);
        }
    }

    private void UpdateNodeToRender1(GrassQuadTreeNode node, Vector3 cameraPosition)
    {
        // 
        if(UpdateNodeToRenderDown(node, cameraPosition))
            return;
        


        // If camera in parent tile, keep as it is
        if(node.Parent.Tile.IsPointInTile(cameraPosition))
        {


        }


        if(node == null)
            return false;
        
        float dist = Vector3.Distance(node.Tile.GetCenterPosition(), cameraPosition); // node.Tile.DistanceTo(cameraPosition);
        int layer = DistanceToLayer(dist); 

        // This could be the node
        if(node.Layer == layer)
        {
            this.NodesToRender.Add(node);
            node.Tile.DrawTile(Color.green);
            return true;
        }

        // If node is higher than required
        if(node.Layer < layer)
        {
            // Split node
            bool isChildRendering = false;
            isChildRendering = isChildRendering || UpdateNodeToRender(node.BottomLeft, cameraPosition);
            isChildRendering = isChildRendering || UpdateNodeToRender(node.BottomRight, cameraPosition);
            isChildRendering = isChildRendering || UpdateNodeToRender(node.TopLeft, cameraPosition);
            isChildRendering = isChildRendering || UpdateNodeToRender(node.TopRight, cameraPosition);
        }



        // If current node is lower than required
        if(node.Layer > layer)

        return false;
    }

    private bool UpdateNodeToRenderDown(GrassQuadTreeNode node, Vector3 cameraPosition)
    {
        // If camera is in Tile, split it
        if(!node.Tile.IsPointInTile(cameraPosition))
            return false;
        
        bool isChildRendering = false;
        isChildRendering = isChildRendering || UpdateNodeToRenderDown(node.BottomLeft, cameraPosition);
        isChildRendering = isChildRendering || UpdateNodeToRenderDown(node.BottomRight, cameraPosition);
        isChildRendering = isChildRendering || UpdateNodeToRenderDown(node.TopLeft, cameraPosition);
        isChildRendering = isChildRendering || UpdateNodeToRenderDown(node.TopRight, cameraPosition);

        if(!isChildRendering)
            this.NodesToRender.Add(node);
        
        return true;
    }

    private bool UpdateNodeToRenderUp(GrassQuadTreeNode node, Vector3 cameraPosition)
    {

    }

    private bool ForceChildrenToRender(GrassQuadTreeNode node, Vector3 cameraPosition)

    private bool UpdateNodeToRender(GrassQuadTreeNode node, Vector3 cameraPosition) // Needs another name
    {
        if(node == null)
            return false;

        float dist = node.Tile.DistanceTo(cameraPosition);
        int layer = DistanceToLayer(dist); 
        //Debug.Log("dist: "+ dist + "; layer: " + layer);

        // This could be the node
        if(node.Layer == layer)
        {
            this.NodesToRender.Add(node);
            node.Tile.DrawTile(Color.green);
            return true;
        }

        // // Node could be higher
        // if(node.Layer > layer)
        // {
        //     Debug.Log("node.Layer: " + node.Layer + "; layer: " + layer); 
        //     if(UpdateNodeToRender(node.Parent, cameraPosition)) // Don't know if the return is correct here
        //         return true;
            
        //     this.NodesToRender.Add(node);
        //     node.Tile.DrawTile(Color.black);
        //     return true;
        // }

        
        // Node is deeper
        bool isChildRendering = false;
        isChildRendering = isChildRendering || UpdateNodeToRender(node.BottomLeft, cameraPosition);
        isChildRendering = isChildRendering || UpdateNodeToRender(node.BottomRight, cameraPosition);
        isChildRendering = isChildRendering || UpdateNodeToRender(node.TopLeft, cameraPosition);
        isChildRendering = isChildRendering || UpdateNodeToRender(node.TopRight, cameraPosition);

        // There are no deeper nodes
        if(!isChildRendering)
        {
            Debug.Log("No children rendering.");
            node.Tile.DrawTile(Color.yellow);
            this.NodesToRender.Add(node);
            return true;
        }
        
        return false;
    }

    private int DistanceToLayer(float distance)
    {
        return (int)(1000 / distance); // replace 100 with RenderDistance
    }

    public override void UpdateLoaded()
    {
        throw new InvalidOperationException("Requires Camera (position) to check for needed changes to loaded tiles");
    }

    public void DrawTilesToRender()
    {
        foreach(GrassQuadTreeNode node in this.NodesToRender)
        {
            node.Tile.DrawTile(Color.red);
        }
    }

    public void DrawAllTiles()
    {
        this.Root.DrawTiles(this.MaxDepth);
    }
}

public class GrassQuadTreeNode : LoadableQuadTreeNode<GrassTileData, GrassDataContainer, GrassQuadTreeNode>
{
    public GrassQuadTreeNode(QuadNodePosition relativePosition, float size, GrassQuadTreeNode parent) 
        : base(relativePosition, size, parent)
    {

    }

    public GrassQuadTreeNode(Vector3 position, float size) : base(position, size)
    {
        
    }

    public override void GenerateBottomLeft()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.BottomLeft = new GrassQuadTreeNode(QuadNodePosition.BottomLeft, newSize, this);
    }

    public override void GenerateBottomRight()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.BottomRight = new GrassQuadTreeNode(QuadNodePosition.BottomRight, newSize, this);
    }

    public override void GenerateTopLeft()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.TopLeft = new GrassQuadTreeNode(QuadNodePosition.TopLeft, newSize, this);
    }

    public override void GenerateTopRight()
    {
        float newSize = this.Tile.Tile.width / 2f;

        this.TopRight = new GrassQuadTreeNode(QuadNodePosition.TopRight, newSize, this);
    }

    public override void UnloadData()
    {
        this.Content.UnloadData();
    }

    protected override GrassDataContainer CreateContent()
    {
        return new GrassDataContainer();
    }

    public void ExpandNode(int layers)
    {
        if(layers < 1)
            return;
        
        layers--;

        this.GenerateBottomLeft();
        this.BottomLeft.ExpandNode(layers);
        this.GenerateBottomRight();
        this.BottomRight.ExpandNode(layers);
        this.GenerateTopLeft();
        this.TopLeft.ExpandNode(layers);
        this.GenerateTopRight();
        this.TopRight.ExpandNode(layers);
    }

    public void DrawTiles(int depth)
    {
        if(this.Layer > depth)
            return;
        
        if(this.BottomLeft != null)
            this.BottomLeft.DrawTiles(depth);
        if(this.BottomRight != null)
            this.BottomRight.DrawTiles(depth);
        if(this.TopLeft != null)
            this.TopLeft.DrawTiles(depth);
        if(this.TopRight != null)
            this.TopRight.DrawTiles(depth);

        this.Tile.DrawTile(Color.blue);
    }
}

public class GrassDataContainer : LoadableDataContainer<GrassTileData>
{
    public override IEnumerator LoadDataCoroutine(string fullFilePath)
    {
        throw new System.NotImplementedException();
    }

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

    //public IEnumerator LoadTextureFromDisk(string filePath, Action<Texture2D> onComplete)
    //{
    //    var uri = Path.Combine(Application.streamingAssetsPath, filePath);
    //    using (var request = UnityWebRequestTexture.GetTexture(uri))
    //    {
    //        yield return request.SendWebRequest();
    //        if (request.result == UnityWebRequest.Result.Success)
    //        {
    //            this.Data
    //            texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    //            onComplete?.Invoke(texture);
    //        }
    //        else
    //        {
    //            Debug.LogError($"Failed to load texture from disk: {request.error}");
    //            onComplete?.Invoke(null);
    //        }
    //    }
    //}


    public override void SaveData(string fullFilePath)
    {
        throw new System.NotImplementedException();
    }
}

public struct GrassTileData
{
    public Texture2D exampleTexture;
}
