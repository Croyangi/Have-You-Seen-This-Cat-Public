using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

public class CopyCatRespawnState : State<CopyCatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CopyCatAnimationHelper animationHelper;

    [SerializeField] private float respawnTimer;
    [SerializeField] private float respawnSet;
    
    [SerializeField] private Vector3 respawnPos;
    private GameObject _playerHead;
    
    [Header("Cat")]
    [SerializeField] private GameObject copyCat;
    [SerializeField] private GameObject copyCatModel;
    [SerializeField] private GameObject hitboxes;
    [SerializeField] private Rigidbody copyCatRigidBody;

    public override void EnterState()
    {
        respawnTimer = respawnSet;
        _playerHead = ManagerPlayer.instance.PlayerHead;
        
        copyCatModel.SetActive(false);
        hitboxes.SetActive(false);
        copyCatRigidBody.isKinematic = true;
        copyCatRigidBody.useGravity = false;
        animationHelper.SetCanRotate(false);
        
        followerEntity.isStopped = true;
        followerEntity.enabled = false;
    }

    public override void ExitState()
    {
        respawnPos = FindFurthestPointDirectional(transform.position, _playerHead.transform.position);
        
        copyCat.transform.position = respawnPos;
        copyCatModel.SetActive(true);
        hitboxes.SetActive(true);
        copyCatRigidBody.isKinematic = false;
        copyCatRigidBody.useGravity = true;
        animationHelper.SetCanRotate(true);
        copyCatRigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        
        followerEntity.isStopped = false;
        followerEntity.enabled = true;
    }

    public override void FixedUpdateTick()
    {
        respawnTimer = Mathf.Clamp(respawnTimer -= Time.fixedDeltaTime, 0, respawnSet);
        if (respawnTimer <= 0)
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming]);
        }
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
        if (Application.isPlaying && stateMachine.CurrentState == stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Respawn])
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(respawnPos, 1);
            
            Gizmos.color = Color.white;
            DrawStringGizmo.DrawString(respawnTimer.ToString("F2"), respawnPos + new Vector3(0, 2f, 0), Gizmos.color, new Vector2(0.5f, 0.5f), 15f);
        }
    }
}
