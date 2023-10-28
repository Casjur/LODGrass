using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GrassRenderer
{
    // VERGEET NIET DE ROOT AAN DE TILES TO LOAD TOE TE VOEGEN

    private List<LoadableGrassMQTNode> nodesToLoad = new List<LoadableGrassMQTNode>();
    private const float SplitDistanceMultiplier = 2;

    private List<RenderTile> tilesToRender;

    public GrassRenderer()
    {
    }

    public void ProcessAndRender(Camera camera, LoadableGrassMQT tree) //List<QuadTree<GrassDataContainer>> tilesToRender)  //GrassQuadTree tree)
    {
        if (nodesToLoad.Count == 0)
            this.nodesToLoad.Add(tree.Root);

        // Subdivide loadable tiles based on camera distance
        UpdateNodesToLoad(camera.transform.position);
        DrawTilesToLoad();

        // Load tiles (NIET IN Render class! Maar in Grass of QuadTree.
        // Render moet het doen met de tiles die op dat moment geladen zijn.
        //LoadNodes(tree.FolderPath, this.nodesToLoad);
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

        LoadableGrassMQTNode[] nodesToLoadCopy = new LoadableGrassMQTNode[this.nodesToLoad.Count];
        this.nodesToLoad.CopyTo(nodesToLoadCopy);
        this.nodesToLoad = new List<LoadableGrassMQTNode>(); // Bad idea (extra costs)

        // Iterate over loaded nodes, and determine which need to be loaded or unloaded
        foreach (LoadableGrassMQTNode node in nodesToLoadCopy)
        {
            UpdateNodeToLoad(node, cameraPosition);
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

    private void GenerateRenderTiles(List<LoadableGrassMQTNode> loadedTiles)
    {
        List<RenderTile> tilesToRender = new List<RenderTile>();

    }


    public void DrawTilesToLoad()
    {
        foreach (LoadableGrassMQTNode node in this.nodesToLoad)
        {
            node.Bounds.DrawTile(Color.red);
        }
    }
}

public struct RenderTile
{
    public Texture2D RenderTexture { get; private set; }

}
