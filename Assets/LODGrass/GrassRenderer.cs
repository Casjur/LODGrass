using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    private List<RenderTileNode> tilesToRender;

    Dictionary<LoadableGrassMQTNode, RenderTileTree> renderTileTrees = new Dictionary<LoadableGrassMQTNode, RenderTileTree>();

    public GrassRenderer(LoadableGrassMQT grassData)
    {

        
    }

    public void ProcessAndRender(Camera camera, LoadableGrassMQT grassDataTree) //List<QuadTree<GrassDataContainer>> tilesToRender)  //GrassQuadTree tree)
    {
        UpdateTilesToRender(camera, grassDataTree);

        foreach (KeyValuePair<LoadableGrassMQTNode, RenderTileTree> nodeTreePair in renderTileTrees)
        {
            // Frustum culling

            // Occlusion culling?

            // Render remaining tiles
            Render();
        }

        // Calculate how many RenderTiles are in the LoadedTile and how many should be drawn
        

        




        if (nodesToLoad.Count == 0)
            this.nodesToLoad.Add(tree.Root);


        // Subdivide loadable tiles based on camera distance
        //UpdateNodesToLoad(camera.transform.position);
        //DrawTilesToLoad();

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

    void UpdateTilesToRender(Camera camera, LoadableGrassMQT grassData)
    {
        UpdateRenderTrees(grassData, camera.transform.position);

        List<RenderTileNode> newTilesToRender = new List<RenderTileNode>();
    }

    void UpdateRenderTrees(LoadableGrassMQT grassData, Vector3 cameraPosition)
    {
        List<LoadableGrassMQTNode> loadedNodes = grassData.GetLoadedNodes();
        Dictionary<LoadableGrassMQTNode, RenderTileTree> updatedRenderTrees = new Dictionary<LoadableGrassMQTNode, RenderTileTree>();
        foreach(LoadableGrassMQTNode loadedNode in loadedNodes)
        {
            bool treeExists = this.renderTileTrees.TryGetValue(loadedNode, out RenderTileTree renderTree);
            if(treeExists)
            {
                updatedRenderTrees.Add(loadedNode, renderTree);
            }
            else
            {
                renderTree = new RenderTileTree(loadedNode, 2); // NIET HARD CODEN!
                updatedRenderTrees.Add(loadedNode, renderTree);
            }

            renderTree.UpdateTilesToRender(cameraPosition);
        }
    }

    void Render(List<RenderReferenceTile>)
    {

        ComputeBuffer dispatchIndirectBuffer = new ComputeBuffer();

        ComputeShader grassGenerator = ComputeShader.Instantiate(_grassShaderOriginal);

        grassGenerator.DispatchIndirect();

        RenderParams rp = new RenderParams(material);
        rp.worldBounds = ; //tile bounds
        Mesh mesh;
        GraphicsBuffer cmdBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 0);

        Graphics.RenderMeshIndirect(in rp, grassMesh1, cmdBuffer, ...);
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
