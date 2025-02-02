using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GrassRenderer
{
    ComputeShader _grassShaderOriginal;
    Mesh grassMesh1;
    Material grassMaterial1;
    Mesh grassMesh2;
    Material grassMaterial2;
    Mesh grassMesh3;
    Material grassMaterial3;

    // VERGEET NIET DE ROOT AAN DE TILES TO LOAD TOE TE VOEGEN

    //private List<LoadableGrassMQTNode> nodesToLoad = new List<LoadableGrassMQTNode>();
    private const float SplitDistanceMultiplier = 2;
    private const int MaxDepth = 2;

    //private List<RenderTileNode> tilesToRender;

    Dictionary<LoadableGrassMQTNode, RenderTileTree> renderTileTrees = new Dictionary<LoadableGrassMQTNode, RenderTileTree>();

    public GrassRenderer(LoadableGrassMQT grassData)
    {

    }

    public void ProcessAndRender(Camera camera, LoadableGrassMQT grassDataTree) //List<QuadTree<GrassDataContainer>> tilesToRender)  //GrassQuadTree tree)
    {
        // Find all loaded nodes
        List<LoadableGrassMQTNode> nodesToRender = grassDataTree.GetLoadedNodes(); 

        // "Frustum" cull loaded nodes
        ViewCulling(ref nodesToRender, camera);

        // Subdivide loaded nodes into render-tiles
        UpdateRenderTrees(nodesToRender, camera.transform.position);

        // Render remaining render-tiles 


        foreach (KeyValuePair<LoadableGrassMQTNode, RenderTileTree> nodeTreePair in renderTileTrees)
        {
            // Frustum culling

            // Occlusion culling?

            // Render remaining tiles
            Render();
        }

        // Calculate how many RenderTiles are in the LoadedTile and how many should be drawn


        // if (nodesToLoad.Count == 0)
        //     this.nodesToLoad.Add(tree.Root);   
    }

    /// <summary>
    /// Culls tiles that are not in view of the camera
    /// </summary>
    /// <param name="camera"></param>
    void ViewCulling(ref List<LoadableGrassMQTNode> nodesToRender, Camera camera)
    {
        // Remove tiles that are not in view
        foreach (LoadableGrassMQTNode node in nodesToRender)//(KeyValuePair<LoadableGrassMQTNode, RenderTileTree> nodeTreePair in renderTileTrees)
        {
            //LoadableGrassMQTNode node = nodeTreePair.Key;
            Rect3D tile = node.Bounds;
            if(!TileInView(camera, tile))
                nodesToRender.Remove(node);
        }

        // Retruns true if any of the corners of the tile are in view
        bool TileInView(Camera camera, Rect3D tile)
        {
            return 
                InfiniteCameraCanSeePoint(camera, tile.NE) ||
                InfiniteCameraCanSeePoint(camera, tile.NW) ||
                InfiniteCameraCanSeePoint(camera, tile.SE) ||
                InfiniteCameraCanSeePoint(camera, tile.SW);
        }

        // Returns true if the point is in view of the camera
        bool InfiniteCameraCanSeePoint(Camera camera, Vector3 point)
        {
            Vector3 viewportPoint = camera.WorldToViewportPoint(point); //GrassTile.TwoDimToThreeDim(point));
            return viewportPoint.z > 0 && new Rect(0, 0, 1, 1).Contains(viewportPoint);
        }
    }

    // /// <summary>
    // /// Updates the tiles that should be rendered
    // /// </summary>
    // /// <param name="camera"></param>
    // /// <param name="grassData"></param>
    // void UpdateTilesToRender(ref List<LoadableGrassMQTNode> nodesToRender, Camera camera)
    // {
    //     UpdateRenderTrees(ref nodesToRender, camera.transform.position);

    //     //List<RenderTileNode> newTilesToRender = new List<RenderTileNode>();
    // }

    /// <summary>
    /// Updates the render trees, based on the camera position
    /// </summary>
    /// <param name="grassData"></param>
    /// <param name="cameraPosition"></param>
    void UpdateRenderTrees(List<LoadableGrassMQTNode> nodesToRender, Vector3 cameraPosition)
    {
        //List<LoadableGrassMQTNode> loadedNodes = grassData.GetLoadedNodes();
        Dictionary<LoadableGrassMQTNode, RenderTileTree> updatedRenderTrees = new Dictionary<LoadableGrassMQTNode, RenderTileTree>();
        foreach(LoadableGrassMQTNode loadedNode in nodesToRender)
        {
            // Check if the tree already exists
            bool treeExists = this.renderTileTrees.TryGetValue(loadedNode, out RenderTileTree renderTree);
            if(treeExists)
            {
                updatedRenderTrees.Add(loadedNode, renderTree);
            }
            else
            {
                renderTree = new RenderTileTree(loadedNode, MaxDepth); // NIET HARD CODEN!
                updatedRenderTrees.Add(loadedNode, renderTree);
            }

            renderTree.UpdateTilesToRender(cameraPosition);
        }

        this.renderTileTrees = updatedRenderTrees;
    }

    void Render()
    {

        ComputeBuffer dispatchIndirectBuffer = new ComputeBuffer(,);

        ComputeShader grassGenerator = ComputeShader.Instantiate(_grassShaderOriginal);
        int kernelID = grassGenerator.FindKernel("Main");
        grassGenerator.DispatchIndirect(kernelID,);
        
        RenderParams rp = new RenderParams(material);
        rp.worldBounds = ; //tile bounds
        Mesh mesh;
        GraphicsBuffer cmdBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 0);

        Graphics.RenderMeshIndirect(in rp, grassMesh1, cmdBuffer, ...);

        dispatchIndirectBuffer.Release();
    }
    

    //private void GenerateRenderTiles(List<LoadableGrassMQTNode> loadedTiles)
    //{
    //    List<RenderTile> tilesToRender = new List<RenderTile>();

    //}

    //void GPUInstancer()
    //{
    //    // Group render tiles in groups of size that is best for performance

    //    // Execute compute shader



    //    foreach(RenderTileNode renderNode in tilesToRender)
    //    {
    //        RenderParams bla = new RenderParams();

    //        ComputeShader cs = Instantiate(csOriginal);
    //        cs.DispatchIndirect(...);

    //        RenderParams rp = new RenderParams(material);
    //        rp.worldBounds = ; //tile bounds
    //        Mesh mesh;
    //        GraphicsBuffer cmdBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 0);
    //        Graphics.RenderMeshIndirect(rp, mesh, cmdBuffer);
            
    //    }

    //}

}

public struct GrassBladeData
{
    int memorySize()
    {
        return sizeof(float) * 4;
    }

    public Vector3 position;
    //public Vector3 normal;
    // public float width;
    // public float height;
    // public float bend;
    public float color;
}