using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatTraversalProvider : MonoBehaviour, ITraversalProvider
{
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;

    public bool CanTraverse(ref TraversalConstraint traversalConstraint, GraphNode node)
    {
        return DefaultITraversalProvider.CanTraverse(ref traversalConstraint, node);
    }

    public bool CanTraverse(ref TraversalConstraint traversalConstraint, GraphNode from, GraphNode to)
    {
        // Default behavior: just defer to node check
        return DefaultITraversalProvider.CanTraverse(ref traversalConstraint, to);
    }

    public float GetTraversalCostMultiplier(ref TraversalCosts traversalCosts, GraphNode node)
    {
        if (pathfindingHelper.IsFearless) return 1f;
        // Thread-safe check (no Unity calls)
        if (pathfindingHelper.VisibleNodes.Contains(node))
            return 5000f;
        return 1f;
    }

    public uint GetConnectionCost(ref TraversalCosts traversalCosts, GraphNode from, GraphNode to)
    {
        // Default cost model: tag cost + node penalty
        return traversalCosts.GetTagEntryCost(to.Tag) + to.Penalty;
    }
}