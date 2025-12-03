using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatPathfindingHelper : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] public AIDestinationSetter aiDestinationSetter;
    [SerializeField] private Seeker seeker;
    [SerializeField] private FollowerEntity followerEntity;
    
    [Header("References")]
    [SerializeField] private GameObject playerHead;
    [SerializeField] private Transform target;
    [SerializeField] private Transform parent;
    [SerializeField] private Camera cameraObj;
    [SerializeField] private CopyCatStateMachine stateMachine;
    [SerializeField] private CapsuleCollider bodyHitbox;
    [SerializeField] private CapsuleCollider headHitbox;

    [Header("Settings")] 
    [SerializeField] private float playerFOVDistance;
    [SerializeField] private float retreatTimer; 
    [SerializeField] private float retreatTime;
    [SerializeField] private float retreatInstantDistance;
    [SerializeField] private bool dev_fearless;
    
    [SerializeField] private float lazyUpdateTime;
    [SerializeField] private float lazyUpdateInterval;
    
    [field: SerializeField] public bool CanSeePlayer { get; private set; }
    [field: SerializeField] public bool IsFearless { get; set; }
    
    [SerializeField] private CopyCatTraversalProvider copyCatTraversalProvider;
    private TraversalConstraint _copyCatPathfindingConstraint;

    
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        
        GraphMask traversableGraphMask = GraphMask.FromGraphName(PathfindingGraphUtility.CopyCatPathfindingGraph);
        TraversalConstraint constraint = new TraversalConstraint(copyCatTraversalProvider);
        constraint.graphMask = traversableGraphMask;
        _copyCatPathfindingConstraint = constraint;
        
        foreach (var graph in AstarPath.active.data.graphs)
        {
            var rcg = graph as RecastGraph;
            if (rcg != null && rcg.name == PathfindingGraphUtility.CopyCatPathfindingGraph)
            {
                _copyCatGraph = graph;
                break;
            }
        }
        
        playerHead = ManagerPlayer.instance.PlayerHead;
        cameraObj = Camera.main;
        
        retreatTimer = retreatTime;

        _visibilityHelper = ManagerPlayer.instance.PlayerVisibilityHelper;
    }

    [SerializeField] private GameObject test;
    private void FixedUpdate()
    {
        if (lazyUpdateTime <= 0 && _copyCatGraph != null)
        {
            UpdateVisibleNodes();
            lazyUpdateTime = lazyUpdateInterval;
        }
        else
        {
            lazyUpdateTime -= Time.fixedDeltaTime;
        }
    }
    
    private HashSet<GraphNode> visibleNodes = new HashSet<GraphNode>();
    public HashSet<GraphNode> VisibleNodes => visibleNodes;


    private NavGraph _copyCatGraph;
    private void UpdateVisibleNodes()
    {
        visibleNodes.Clear();
        
        _copyCatGraph.GetNodes(node =>
        {
            Vector3 nodePos = (Vector3)node.position;
            if (CanPlayerSeeNode(nodePos))
                visibleNodes.Add(node);
        });
    }
    
    private bool CanPlayerSeeNode(Vector3 position)
    {
        position += Vector3.up * 0.1f; // Leeway for nodes on the ground
        if (Vector3.Distance(position, playerHead.transform.position) > playerFOVDistance) return false;
        return IsInFOV(position) && IsInLOS(playerHead.transform.position, position);
    }


    /// <summary>
    /// Returns if player can see CopyCat based on FOV and LOS.
    /// </summary>
    public bool CanPlayerSeeCopyCat()
    {
        if (playerHead == null) return false;
        if (Vector3.Distance(parent.position, playerHead.transform.position) > playerFOVDistance) return false;
        if (IsInFOV(parent.position) && GetCopyCatPartialLOS()) return true;
        return false;
    }

    public bool CanPlayerSeePosition(Vector3 position)
    {
        if (Vector3.Distance(position, playerHead.transform.position) > playerFOVDistance) return false;
        return IsInFOV(position) && IsInLOS(playerHead.transform.position, position);
    }

    [SerializeField] private float detectPlayerTimer;
    [SerializeField] private float detectPlayerTimerSet;
    private PlayerVisibilityHelper _visibilityHelper;
    public void CanSeePlayerUpdate(bool isForced = false)
    {
        if (GetCopyCatPartialLOS())
        {
            detectPlayerTimer = Mathf.Max(detectPlayerTimer -= _visibilityHelper.GetVisibilityMultiplier() * Time.deltaTime, 0f);
        }
        else
        {
            detectPlayerTimer = Mathf.Min(detectPlayerTimer += Time.deltaTime, detectPlayerTimerSet);
        }

        CanSeePlayer = detectPlayerTimer <= 0 || isForced;
    }
    
    public bool IsPathPossible(Vector3 pos, float acceptableDistance = 2f)
    {
        ABPath path = ABPath.Construct(transform.position, pos, null);
        path.traversalConstraint = _copyCatPathfindingConstraint;
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();
        
        return path.CompleteState == PathCompleteState.Complete && Vector3.Distance(path.endPoint, pos) <= acceptableDistance;
    }
    
    public bool GetCopyCatCompleteLOS()
    {
        float rad = bodyHitbox.radius;
        
        Vector3[] directions = { Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right };

        foreach (Vector3 dir in directions)
        {
            Vector3 checkPos = transform.position + dir * rad + Vector3.up * bodyHitbox.height / 2;
            if (!IsInLOS(checkPos, playerHead.transform.position)) return false;
        }

        return true;
    }
    
    public bool GetCopyCatPartialLOS()
    {
        float rad = bodyHitbox.radius;
        
        Vector3[] directions = { Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right };

        foreach (Vector3 dir in directions)
        {
            Vector3 checkPos = transform.position + dir * rad + Vector3.up * bodyHitbox.height / 2;
            if (IsInLOS(checkPos, playerHead.transform.position)) return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Returns the furthest cat from Player
    /// </summary>
    public GameObject GetFurthestCatFromPlayer()
    {
        // Edge case, dev tool
        if (ManagerCat.instance.FoundCats.Count == 0)
        {
            Debug.LogWarning("No cats trailing player!");
            return null;
        }
        
        float furthestDistance = Mathf.NegativeInfinity;
        GameObject closestCat = null;
        foreach (GameObject cat in ManagerCat.instance.FoundCats)
        {
            float distance = Vector3.Distance(cat.transform.position, playerHead.transform.position);
            if (distance > furthestDistance)
            {
                closestCat = cat;
                furthestDistance = distance;
            }
        }
        
        return closestCat;
    }
    
    /// <summary>
    /// Function that, if ran continuously, will check if the player can see the CopyCat and retreat if so
    /// </summary>
    public bool RetreatCheck()
    {
        if (dev_fearless || !CanPlayerSeeCopyCat())
        {
            retreatTimer = retreatTime;
            return false;
        }
        
        if (Vector3.Distance(transform.position, playerHead.transform.position) <= retreatInstantDistance)
        {
            stateMachine.RequestStateChange(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Retreat]);
            return true;
        }
        
        retreatTimer = Mathf.Clamp(retreatTimer - Time.fixedDeltaTime, 0f, retreatTime);
        if (retreatTimer <= 0)
        {
            stateMachine.RequestStateChange(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Retreat]);
            return true;
        }

        return false;
    }


    [SerializeField] private float horizontalFOVReduction;
    private float GetHorizontalFOV()
    {
        float halfHorizontalFOV = Mathf.Atan(Mathf.Tan((cameraObj.fieldOfView - horizontalFOVReduction) * Mathf.Deg2Rad * 0.5f) * cameraObj.aspect) * Mathf.Rad2Deg;
        return halfHorizontalFOV;
    }
    
    /// <summary>
    /// Sets the AI's destination based on a world position, converting it to the local position relative to the parent.
    /// </summary>
    /// <param name="position">The target position in world space.</param>
    public void SetAIDestinationWithPosition(Vector3 position)
    {
        Vector3 localPosition = parent.InverseTransformPoint(position);
        target.localPosition = localPosition;
        aiDestinationSetter.target = target.transform;
    }
    
    /// <summary>
    /// Returns if a position is in the player's FOV.
    /// </summary>
    public bool IsInFOV(Vector3 position)
    {
        Vector3 flatForward = new Vector3(playerHead.transform.forward.x, 0, playerHead.transform.forward.z).normalized;
        Vector3 flatDir = (position - playerHead.transform.position);
        flatDir.y = 0;
        flatDir.Normalize();

        float dot = Vector3.Dot(flatForward, flatDir);
        float fovThreshold = Mathf.Cos(GetHorizontalFOV() * Mathf.Deg2Rad);
        return dot >= fovThreshold;
    }

    
    /// <summary>
    /// Returns if there is a clear path between two points.
    /// </summary>
    public bool IsInLOS(Vector3 origin, Vector3 end)
    {
        Vector3 direction = (end - origin).normalized;
        float range = Vector3.Distance(origin, end);
        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, range, LayerUtility.BlocksLOS))
        {
            return false; // Obstacle detected
        }

        return true; // Clear line of sight
    }

    
    private void OnDrawGizmos()
{
    if (!Application.isPlaying || playerHead == null || cameraObj == null)
        return;

    Gizmos.color = Color.yellow;
    foreach (GraphNode node in VisibleNodes)
    {
        Gizmos.DrawSphere((Vector3)node.position, 0.3f);
    }

    
    
    Vector3 GetLOSAdjustedEndpoint(Vector3 origin, Vector3 direction, float maxDistance)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, LayerUtility.BlocksLOS))
        {
            return direction * hit.distance;
        }

        return direction * maxDistance;
    }
    
    void DrawFOVArc(Vector3 origin, Vector3 forward, float halfAngle, float radius, int segments = 20)
    {
        Vector3 prevPoint = origin + Quaternion.Euler(0, -halfAngle, 0) * forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float step = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
            Vector3 nextPoint = origin + Quaternion.Euler(0, step, 0) * forward * radius;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    // --- DRAW PLAYER FOV CONE ---
    Vector3 playerPos = playerHead.transform.position;
    Vector3 forward = new Vector3(playerHead.transform.forward.x, 0, playerHead.transform.forward.z).normalized;

    float halfHorizontalFOV = GetHorizontalFOV();
    float maxDistance = playerFOVDistance;

    Vector3 leftDir = Quaternion.Euler(0, -halfHorizontalFOV, 0) * forward;
    Vector3 rightDir = Quaternion.Euler(0, halfHorizontalFOV, 0) * forward;

    // Perform LOS-aware endpoints for FOV cone
    Vector3 leftEnd = playerPos + GetLOSAdjustedEndpoint(playerPos, leftDir, maxDistance);
    Vector3 rightEnd = playerPos + GetLOSAdjustedEndpoint(playerPos, rightDir, maxDistance);
    Vector3 forwardEnd = playerPos + GetLOSAdjustedEndpoint(playerPos, forward, maxDistance);

    // FOV cone outline
    Gizmos.color = new Color(0, 1, 0, 0.25f);
    Gizmos.DrawLine(playerPos, leftEnd);
    Gizmos.DrawLine(playerPos, rightEnd);
    Gizmos.DrawLine(leftEnd, rightEnd);
    Gizmos.DrawLine(playerPos, forwardEnd);

    // Optional arc visualization
    DrawFOVArc(playerPos, forward, halfHorizontalFOV, maxDistance);

    // --- DRAW COPYCAT LOS RAYS ---
    float rad = bodyHitbox.radius;
    float heightOffset = bodyHitbox.height / 2;
    Vector3[] directions = { Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right };

    foreach (Vector3 dir in directions)
    {
        Vector3 checkPos = transform.position + dir * rad + Vector3.up * heightOffset;
        bool hasLOS = IsInLOS(checkPos, playerHead.transform.position);

        Gizmos.color = hasLOS ? Color.green : Color.red;
        Gizmos.DrawLine(checkPos, playerHead.transform.position);
    }
}
}
