using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;

public class CopyCatRetreatState : State<CopyCatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    private GameObject _playerHead;
    [SerializeField] private Seeker seeker;

    [Header("Settings")]
    [SerializeField] private float stopDistance;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float restPeriod;
    [SerializeField] private float restPeriodSet;
    [SerializeField] private float trappedDistanceThreshold;
    [SerializeField] private int maxDepthSearchFurthestPoint;
    [SerializeField] private int maxDepthSearchNearestAvoidance;

    [SerializeField] private int retreatCount;
    [SerializeField] private int forceHuntingCount;
    
    private Vector3 _startNode;
    private Vector3 _furthestDestination;
    private List<GraphNode> _avoidanceNodes = new List<GraphNode>();

    private void Start()
    {
        _playerHead = ManagerPlayer.instance.PlayerHead;
    }

    public override void EnterState()
    {
        pathfindingHelper.IsFearless = true;
        followerEntity.isStopped = false;
        aiDestinationSetter.enabled = false;
        
        restPeriod = restPeriodSet;
        
        followerEntity.maxSpeed = movementSpeed;
        followerEntity.stopDistance = stopDistance;

        retreatCount++;
        if (retreatCount >= forceHuntingCount && pathfindingHelper.GetCopyCatCompleteLOS())
        {
            SetForceHunting();
            return;
        }
        
        RetreatSearch();
    }

    private void SetForceHunting()
    {
        retreatCount = 0;
        ManagerPlayer.instance.PlayerVFXHelper.Scare();
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting]);
    }
    
    public override void ExitState()
    {
        pathfindingHelper.IsFearless = false;
        aiDestinationSetter.enabled = true;
    }

    public override void FixedUpdateTick()
    {
        if (!followerEntity.hasPath)
        {
            RetreatSearch();
            return;
        }
        
        if (followerEntity.reachedEndOfPath)
        {
            restPeriod = Mathf.Clamp(restPeriod -= Time.fixedDeltaTime, 0, restPeriodSet);
            
            animationHelper.PlayAnimation(animationHelper.Idle, animationHelper.BaseLayer);

            if (pathfindingHelper.CanPlayerSeeCopyCat())
            {
                SetForceHunting();
                return;
            }
        }
        else
        {
            restPeriod = restPeriodSet;
            animationHelper.PlayAnimation(animationHelper.Chase, animationHelper.BaseLayer);
        }
        
        if (restPeriod <= 0)
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming]);
        }
        
    }
    
    [ContextMenu("Search")]
    public void RetreatSearch()
    {
        _furthestDestination = FindFurthestPointDirectional(transform.position, _playerHead.transform.position);
        
        // In rare cases, almost dev-altered-cases, furthest node will be right next to start node due to already being far from player, this prevents that.
        if (Vector3.Distance(transform.position, _furthestDestination) < trappedDistanceThreshold && pathfindingHelper.CanPlayerSeeCopyCat())
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting]);
            return;
        }
        
        ABPath abPath = ABPath.Construct(transform.position, _furthestDestination);
        var path = seeker.StartPath(abPath, OnPathComplete);
        path.BlockUntilCalculated();
    }

    private void OnPathComplete(Path p)
    {
        followerEntity.SetPath(p);
        followerEntity.destination = _furthestDestination;
        followerEntity.SearchPath();
    }

    private Vector3 FindFurthestPointDirectional(Vector3 origin, Vector3 avoidancePosition, int directions = 16, float rayDistance = 40f)
    {
        GraphMask traversableGraphMask = GraphMask.FromGraphName(PathfindingGraphUtility.CopyCatPathfindingGraph);
        NNConstraint constraint = NNConstraint.Default;
        constraint.graphMask = traversableGraphMask;
        constraint.walkable = true;
        constraint.constrainTags = true;
        constraint.tags = ~(1 << 3);

        Vector3 bestPoint = origin;
        float bestDist = 0f;

        for (int i = 0; i < directions; i++)
        {
            // Evenly spaced rays in a circle
            float angle = i * (360f / directions);
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));

            // Cast away from player, not just outward from origin
            Vector3 target = avoidancePosition + dir * rayDistance;

            // Snap this target to nearest navmesh point
            NNInfo nn = AstarPath.active.GetNearest(target, constraint);
            if (nn.node != null && nn.node.Walkable)
            {
                float dist = Vector3.Distance(nn.position, avoidancePosition);
                if (dist > bestDist)
                {
                    bestDist = dist;
                    bestPoint = nn.position;
                }
            }
        }

        return bestPoint;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && stateMachine.CurrentState == stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Retreat])
        {
            foreach (GraphNode node in _avoidanceNodes)
            {
                //Gizmos.color = Color.red;
                //Gizmos.DrawCube((Vector3) node.position, Vector3.one * 0.3f);
            }
            
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(_furthestDestination, Vector3.one * 2f);


            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(_startNode, Vector3.one * 0.5f);
        }
    }
}
