using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GrassRenderer
{
    // VERGEET NIET DE ROOT AAN DE TILES TO LOAD TOE TE VOEGEN

    private List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> nodesToLoad = new List<QuadTreeNode<LoadableStructContainer<GrassTileData>>>();
    private const float SplitDistanceMultiplier = 2;

    private List<RenderTile> tilesToRender;

    public GrassRenderer()
    {
    }

    public void ProcessAndRender(Camera camera, GrassQuadTree tree) //List<QuadTree<GrassDataContainer>> tilesToRender)  //GrassQuadTree tree)
    {
        if (nodesToLoad.Count == 0)
            this.nodesToLoad.Add(tree.Root);

        // Subdivide loadable tiles based on camera distance
        UpdateNodesToLoad(camera.transform.position);
        DrawTilesToLoad();

        // Load tiles
        LoadNodes(tree.FolderPath, this.nodesToLoad);
        //this.monoBehaviour.StartCoroutine(LoadNodesAllAsync(tree.FolderPath, this.nodesToLoad));
        
        // Frustum cull loaded tiles

        // Subdivide loadable tiles further into render tiles
        //List < RenderTile > tilesToRender = GenerateRenderTiles();

        // Make sure ???

        // Frustum cull render tiles

        // Render remaining tiles

    }

    public virtual void UpdateNodesToLoad(Vector3 cameraPosition) // Should probably be in the Grass class
    {
        Debug.Log("noNodesToLoad: " + this.nodesToLoad.Count);

        QuadTreeNode<LoadableStructContainer<GrassTileData>>[] nodesToLoadCopy = new QuadTreeNode<LoadableStructContainer<GrassTileData>>[this.nodesToLoad.Count];
        this.nodesToLoad.CopyTo(nodesToLoadCopy);
        this.nodesToLoad = new List<QuadTreeNode<LoadableStructContainer<GrassTileData>>>(); // Bad idea (extra costs)

        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
        foreach (QuadTreeNode<LoadableStructContainer<GrassTileData>> node in nodesToLoadCopy)
        {
            UpdateNodeToLoad(node, cameraPosition);
        }
    }

    private bool UpdateNodeToLoad(QuadTreeNode<LoadableStructContainer<GrassTileData>> node, Vector3 cameraPosition)
    {
        if (node == null)
            return false;

        float distance = Vector3.Distance(node.Tile.GetCenterPosition(), cameraPosition);

        // Node might be too close
        if (distance < SplitDistanceMultiplier * node.Tile.GetSize())
        {
            // Node is too close -> Split node
            if (node.HasChildren)
            {
                UpdateNodeToLoad(node.BottomLeft, cameraPosition);
                UpdateNodeToLoad(node.BottomRight, cameraPosition);
                UpdateNodeToLoad(node.TopLeft, cameraPosition);
                UpdateNodeToLoad(node.TopRight, cameraPosition);

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

        float distanceToParent = Vector3.Distance(node.Parent.Tile.GetCenterPosition(), cameraPosition);

        // Node is too far away -> render parent
        if (distanceToParent >= SplitDistanceMultiplier * node.Parent.Tile.GetSize())
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

    private async Task LoadNodes(string folderPath, List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> tilesToLoad)
    {
        foreach(QuadTreeNode<LoadableStructContainer<GrassTileData>> tile in tilesToLoad)
        {
            tile.Content.LoadData(folderPath);
        }
    }

    private IEnumerator LoadNodesAllAsync(string folderPath, List<QuadTreeNode<LoadableStructContainer<GrassTileData>>> tilesToLoad)
    {
        // NOT IMPLEMENTED. not sure if I should
        yield return null;
    }

    //private void GenerateRenderTiles(List<QuadTreeNode<GrassDataContainer>> loadedTiles)
    //{
    //    List<RenderTile> tilesToRender = new List<RenderTile>();

    //}


    public void DrawTilesToLoad()
    {
        foreach (QuadTreeNode<LoadableStructContainer<GrassTileData>> node in this.nodesToLoad)
        {
            node.Tile.DrawTile(Color.red);
        }
    }
}

public class RenderTile
{
    public Texture2D? RenderTexture { get; private set; }

}
