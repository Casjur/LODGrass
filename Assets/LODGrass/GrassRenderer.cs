using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassRenderer
{
    private MonoBehaviour monoBehaviour; // This seems like a bad idea

    // VERGEET NIET DE ROOT AAN DE TILES TO LOAD TOE TE VOEGEN

    private List<QuadTreeNode<GrassDataContainer>> nodesToLoad = new List<QuadTreeNode<GrassDataContainer>>();
    private const float SplitDistanceMultiplier = 2;

    private List<RenderTile> tilesToRender;

    public GrassRenderer(MonoBehaviour monoBehaviour)
    {
        this.monoBehaviour = monoBehaviour;
    }

    public void ProcessAndRender(Camera camera, GrassQuadTree tree) //List<QuadTree<GrassDataContainer>> tilesToRender)  //GrassQuadTree tree)
    {
        if (nodesToLoad.Count == 0)
            this.nodesToLoad.Add(tree.Root);

        // Subdivide loadable tiles based on camera distance
        UpdateNodesToLoad(camera.transform.position);
        DrawTilesToLoad();

        // Load tiles
        LoadNodes(this.monoBehaviour, tree.FolderPath, this.nodesToLoad);
        //this.monoBehaviour.StartCoroutine(LoadNodesAllAsync(tree.FolderPath, this.nodesToLoad));
        
        // Frustum cull loaded tiles

        // Subdivide loadable tiles further into render tiles
        //List < RenderTile > tilesToRender = GenerateRenderTiles();

        // Make sure ???

        // Frustum cull render tiles

        // Render remaining tiles

    }

    public virtual void UpdateNodesToLoad(Vector3 cameraPosition) // Should maybe be in the GrassRenderer class
    {
        Debug.Log("noNodesToLoad: " + this.nodesToLoad.Count);

        QuadTreeNode<GrassDataContainer>[] nodesToLoadCopy = new QuadTreeNode<GrassDataContainer>[this.nodesToLoad.Count];
        this.nodesToLoad.CopyTo(nodesToLoadCopy);
        this.nodesToLoad = new List<QuadTreeNode<GrassDataContainer>>(); // Bad idea (extra costs)

        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
        foreach (QuadTreeNode<GrassDataContainer> node in nodesToLoadCopy)
        {
            UpdateNodeToLoad(node, cameraPosition);
        }
    }

    private bool UpdateNodeToLoad(QuadTreeNode<GrassDataContainer> node, Vector3 cameraPosition)
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

    private void LoadNodes(MonoBehaviour monoBehaviour, string folderPath, List<QuadTreeNode<GrassDataContainer>> tilesToLoad)
    {
        foreach(QuadTreeNode<GrassDataContainer> tile in tilesToLoad)
        {
            monoBehaviour.StartCoroutine(tile.Content.LoadDataCoroutine(folderPath));
        }
    }

    private IEnumerator LoadNodesAllAsync(string folderPath, List<QuadTreeNode<GrassDataContainer>> tilesToLoad)
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
        foreach (QuadTreeNode<GrassDataContainer> node in this.nodesToLoad)
        {
            node.Tile.DrawTile(Color.red);
        }
    }
}

public class RenderTile
{
    public Texture2D? RenderTexture { get; private set; }

}
