using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField]
    string folderPath;
    Terrain terrain;
    int detailMapDensity;
    int detailMapPixelWidth;
    double maxStoredPixels;

    // Start is called before the first frame update
    void Start()
    {
        LoadableQuadTree<GrassLODTile> grass = LoadableQuadTreeGenerator<GrassLODTile>.GenerateLoadableQuadTree(folderPath, terrain, detailMapDensity, detailMapPixelWidth, maxStoredPixels);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
