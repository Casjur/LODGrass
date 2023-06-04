using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    // Input variables
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
    public GrassQuadTree GrassData { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        string f = Application.dataPath;
        this.GrassData = (GrassQuadTree)LoadableQuadTreeGenerator<LoadableQuadTreeNode<GrassTileData>>.GenerateLoadableQuadTree(f, terrain, detailMapDensity, detailMapPixelWidth, maxStoredPixels);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class GrassQuadTree : LoadableQuadTree<LoadableQuadTreeNode<GrassTileData>>
{
    public GrassQuadTree(string folderPath, Vector3 position, float size) : base(folderPath, position, size)
    {
    }
}

public class GrassQuadTreeNode : LoadableQuadTreeNode<GrassTileData>
{
    public GrassQuadTreeNode(Vector3 position, float size, string fileName, QuadTreeNode<LoadableDataContainer<GrassTileData>> parent = null) : base(position, size, fileName, parent)
    {
        this.DataContainer = new GrassDataContainer();
    }
}

public class GrassDataContainer : LoadableDataContainer<GrassTileData>
{
    public override IEnumerator LoadDataCoroutine(string fullFilePath)
    {
        throw new System.NotImplementedException();
    }

    public override void SaveData(string fullFilePath)
    {
        throw new System.NotImplementedException();
    }
}
