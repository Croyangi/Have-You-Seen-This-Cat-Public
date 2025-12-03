using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class CopyCat : MonoBehaviour
{
    [SerializeField] private Seeker seeker;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CopyCatTraversalProvider traversalProvider;
    public float aggression;
    
    private void Awake()
    {
        GraphMask traversableGraphMask = GraphMask.FromGraphName(PathfindingGraphUtility.CopyCatPathfindingGraph);
        seeker.graphMask = traversableGraphMask;
        followerEntity.pathfindingSettings.graphMask = traversableGraphMask;
        
        seeker.traversalProvider = traversalProvider;
        followerEntity.pathfindingSettings.traversalProvider = traversalProvider;
    }
}
