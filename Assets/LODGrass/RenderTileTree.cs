using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTileTree : MinimalQuadTreeAbstract<RenderReferenceTile, RenderTileNode>
{
    public LoadableGrassMQTNode ReferenceData { get; private set; }
    public List<RenderTileNode> TilesToRender { get; private set; } = new List<RenderTileNode>();

    public const float SplitDistanceMultiplier = 2;

    public RenderTileTree(LoadableGrassMQTNode referenceData, int maxDepth) : base(referenceData.Bounds, maxDepth)
    {
        this.ReferenceData = referenceData;
    }

    public RenderTileTree(Rect3D bounds) : base(bounds)
    {
    }

    public override RenderTileNode GenerateRoot(Rect3D bounds)
    {
        RenderTileNode root = new RenderTileNode(bounds, this.ReferenceData.Content.exampleTexture.width);
        return root;
    }

    public void UpdateTilesToRender(Vector3 cameraPos)
    {
        List<RenderTileNode> updatedNodesToRender = new List<RenderTileNode>();
        foreach(RenderTileNode node in this.TilesToRender)
        {
            UpdateNodeToRender(node, cameraPos, ref updatedNodesToRender);
        }
    }

    void UpdateNodeToRender(RenderTileNode node, Vector3 cameraPosition, ref List<RenderTileNode> updatedNodesToRender)
    {
        if (node == null)
            return;

        float distance = Vector3.Distance(node.Bounds.GetCenterPosition(), cameraPosition);

        // Node might be too close
        if (distance < SplitDistanceMultiplier * node.Bounds.GetSize())
        {
            // Node is too close -> Split node
            if (node.HasChildren)
            {
                UpdateNodeToRender(node.NE, cameraPosition, ref updatedNodesToRender);
                UpdateNodeToRender(node.NW, cameraPosition, ref updatedNodesToRender);
                UpdateNodeToRender(node.SE, cameraPosition, ref updatedNodesToRender);
                UpdateNodeToRender(node.SW, cameraPosition, ref updatedNodesToRender);

                // remove current node (already done, since new list)
                return;
            }

            updatedNodesToRender.Add(node);
            return;
        }

        if(this.TilesToRender.Count == 0 && node.Parent == null)
        {
            updatedNodesToRender.Add(node);
            return;
        }

        float distanceToParent = Vector3.Distance(node.Parent.Bounds.GetCenterPosition(), cameraPosition);

        // Node is too far away -> render parent
        if (distanceToParent >= SplitDistanceMultiplier * node.Parent.Bounds.GetSize())
        {
            if(updatedNodesToRender.Contains(node))
                return;

            updatedNodesToRender.Add(node.Parent);

            return;
        }

        // Node is fine as it is
        if (updatedNodesToRender.Contains(node))
            return;

        updatedNodesToRender.Add(node);

        return;
    }
}

public class RenderTileNode : MinimalQuadTreeNodeAbstract<RenderReferenceTile, RenderTileNode>
{
    public RenderTileNode(Rect3D bounds, int resolution) 
        : base(bounds)
    {
        this.Content = new RenderReferenceTile(0, 0, resolution);
    }

    public RenderTileNode(RenderTileNode parent, QuadNodePosition relativePosition) 
        : base(parent, relativePosition)
    {
        int resolution = parent.Content.resolution / 2;
        int x = RelativePositions[(int)relativePosition].x * resolution;
        int y = RelativePositions[(int)relativePosition].z * resolution;

        this.Content = new RenderReferenceTile(x, y, resolution);
    }

    public override void GenerateNE()
    {
        if (this.Content.resolution > 1)
            this.NE = new RenderTileNode(this.Parent, QuadNodePosition.NE);
        else
            throw new System.Exception("Blud is trynna create a texture 1x1 or sumting");
    }

    public override void GenerateNW()
    {
        if (this.Content.resolution > 1)
            this.NW = new RenderTileNode(this.Parent, QuadNodePosition.NW);
        else
            throw new System.Exception("Blud is trynna create a texture 1x1 or sumting");
    }

    public override void GenerateSE()
    {
        if (this.Content.resolution > 1)
            this.SE = new RenderTileNode(this.Parent, QuadNodePosition.SE);
        else
            throw new System.Exception("Blud is trynna create a texture 1x1 or sumting");
    }

    public override void GenerateSW()
    {
        if (this.Content.resolution > 1)
            this.SW = new RenderTileNode(this.Parent, QuadNodePosition.SW);
        else
            throw new System.Exception("Blud is trynna create a texture 1x1 or sumting");
    }
}

// Litteraly the same as RectInt, but with less. (might take up less memory?)
/// <summary>
/// Specifies where the shader should read from the target image.
/// </summary>
public struct RenderReferenceTile 
{
    public int x;
    public int y;
    public int resolution;

    public RenderReferenceTile(int x, int y, int resolution)
    {
        this.x = x;
        this.y = y;
        this.resolution = resolution;
    }
}

