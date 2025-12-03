using UnityEngine;

public static class LayerUtility
{
    public static readonly string PathfindingObstacle = "PathfindingObstacle";
    public static readonly string Pathfinding = "Pathfinding";
    public static readonly string CopyCatPathfindingObstacle = "CopyCatPathfindingObstacle";
    public static readonly string CopyCatRespawnObstacle = "CopyCatPathfindingObstacle";
    public static readonly string LOSBlocker = "LOSBlocker";
    public static readonly string InteractBlocker = "InteractBlocker";

    public static readonly LayerMask BlocksLOS = LayerMask.GetMask(PathfindingObstacle, Pathfinding, CopyCatPathfindingObstacle, CopyCatRespawnObstacle, LOSBlocker);
    public static readonly LayerMask Environment = LayerMask.GetMask(PathfindingObstacle, Pathfinding, CopyCatPathfindingObstacle, InteractBlocker, CopyCatRespawnObstacle);

    public static bool IsInLayer(GameObject obj, int layer)
    {
        return obj.layer == layer;
    }
}