using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// Potential optimizations / problems:
// 1. Load nodes async all at once, not one at a time
// 2. Use a more common struct or class for the bounds of nodes.
//    The current Rect3D is goofy as hell and not every application
//    with this class will be 3D.
//    . Bounds
//    . Rect
// 3. Change how the initial QuadTree is generated. 
//    It would be more intuitive to just say what the grass density should be
// 4. If the user decides to change the grass density, it can only be 4x higher or lower
// 5. Changing terrain size is currently not possible
// 6. Create better a better paint brush
// 7. Optimize grass painting
// 8. Texture2D is overkill for how much data actually has to be given
//    to the compute shader. (grass type (byte), height (float))
// 9. Try to implement the "Double-buffer strategy" from the GoTs GDC talk

// IF IT CRASHES AND BURNS, CHECK GenerateBoundsFromParent in MQTAbstract.
// I am casting Vector3Int to a Vector3

public class Grass : MonoBehaviour
{
    // Debug vars
    public static Material testMat;
    public GameObject testObject;
    public Transform brushTransform;

    // Controls
    public const bool enableEditing = true;
    public bool doUpdateWithCamera = false;

    // Generation input variables
    [SerializeField] string folderPath;// = "/GrassData";
    [SerializeField] Terrain terrain; 
    [SerializeField] float detailMapDensity = 6.5f; // Ghost of Tsushima waarde (ongv: 200m tile oftewel 0.39 texel)
    [SerializeField] int detailMapPixelWidth = 512; // Ghost of Tsushima waarde
    [SerializeField] double maxStoredPixels = 357826560;
    [SerializeField] float grassDensity = 8;

    // Dependencies
    [SerializeField] private Camera camera;
    
    //[field: SerializeField]
    //public const float SplitDistanceMultiplier = 2;

    // Contents
    public LoadableGrassMQT GrassData { get; private set; }
    private GrassRenderer GrassRenderer;

    // Start is called before the first frame update
    void Start()
    {
        string wrongFullFolderPath = Path.Combine(Application.dataPath, folderPath);
        string fullFolderPath = wrongFullFolderPath.Replace("\\", "/");

        Vector2 terrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);

        this.GrassData = new LoadableGrassMQT(
            fullFolderPath, 
            terrain.GetPosition(), 
            terrainSize, 
            detailMapDensity, 
            detailMapPixelWidth,
            testObject
            );
        this.GrassData.CreateTreeFromFiles();

        //this.GrassRenderer = new GrassRenderer(this.GrassData);
    }

    // Update is called once per frame
    void Update()
    {
        //this.GrassRenderer.ProcessAndRender(this, camera, this.GrassData);

        // Test grass painting with an objects position
        if (enableEditing)
            this.GrassData.PaintGrass(brushTransform.position, (int)brushTransform.localScale.x, 0);

        this.GrassData.doUpdateWithCamera = this.doUpdateWithCamera;
        this.GrassData.UpdateLoaded(camera.transform.position);
        
        DrawLoadedTiles(this.GrassData.GetLoadedNodes());
    }

    //
    private void DrawLoadedTiles(List<LoadableGrassMQTNode> loadedNodes)
    {
        foreach(LoadableGrassMQTNode node in loadedNodes)
        {
            node.Bounds.DrawTile(Color.red);
            Debug.Log("Drawing Bounds. Pos: " + node.Bounds.GetPosition() + "; Size: " + node.Bounds.GetSize());
            //GameObject.CreatePrimitive(PrimitiveType.Quad);
        }
    }
}

public class GrassTileData
{
    public Texture2D exampleTexture;

    public GrassTileData(int width, int height)
    {
        exampleTexture = new Texture2D(width, height);
    }

    public GrassTileData(Texture2D texture)
    {
        this.exampleTexture = texture;
    }
}
