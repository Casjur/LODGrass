using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTileTree : MinimalQuadTreeAbstract<RenderTexture, RenderTileNode>
{
    public override RenderTileNode GenerateRoot(Rect3D bounds)
    {
        throw new System.NotImplementedException();
    }
}

public class RenderTileNode : MinimalQuadTreeNodeAbstract<RenderTexture, RenderTileNode>
{
    
    public override void GenerateNE()
    {
        throw new System.NotImplementedException();
    }

    public override void GenerateNW()
    {
        throw new System.NotImplementedException();
    }

    public override void GenerateSE()
    {
        throw new System.NotImplementedException();
    }

    public override void GenerateSW()
    {
        throw new System.NotImplementedException();
    }
}
