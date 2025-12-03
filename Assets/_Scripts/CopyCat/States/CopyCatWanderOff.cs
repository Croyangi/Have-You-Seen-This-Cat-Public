// using System.Collections;
// using System.Collections.Generic;
// using Pathfinding;
// using UnityEngine;
//
// public class CopyCatWanderOff : State
// {
//     
//     [Header("Building Block References")] 
//     [SerializeField] private CopyCatStateMachine stateMachine;
//     [SerializeField] private AIDestinationSetter aiDestinationSetter;
//     [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
//     [SerializeField] private FollowerEntity followerEntity;
//     [SerializeField] private Seeker seeker;
//     [SerializeField] private CopyCatAnimationHelper animationHelper;
//
//     [Header("References")]
//     [SerializeField] private GameObject playerHead;
//     [SerializeField] public GraphMask copyCatGraphMask;
//     [SerializeField] private CopyCatSearchingState searchingState;
//
//     [Header("Settings")] 
//     [SerializeField] private float movementSpeed;
//     [SerializeField] private float stopDistance;
//     [SerializeField] private bool hasBegunMoving;
//
//     public override void EnterState()
//     {
//         AutoRepathPolicy arp = new AutoRepathPolicy();
//         arp.mode = Pathfinding.AutoRepathPolicy.Mode.Dynamic;
//         followerEntity.autoRepath = arp;
//         aiDestinationSetter.target = null;
//
//         playerHead = ManagerPlayer.instance.PlayerHead;
//
//         followerEntity.stopDistance = stopDistance;
//         followerEntity.maxSpeed = movementSpeed;
//         
//         WanderOffSearch();
//     }
//
//     public override void ExitState()
//     {
//
//         animationHelper.SetRotationWithVelocityCheck(false);
//         animationHelper.SetCanRotate(true);
//     }
//
//     public override void FixedUpdateState()
//     {
//         //searchingState.DetectPlayerUpdate();
//         
//         if (followerEntity.reachedEndOfPath && !followerEntity.isStopped && hasBegunMoving)
//         {
//             TransitionToState(stateMachine.GetState(CopyCatStateMachine.CopyCatStates.Roaming));
//             return;
//         }
//
//         // Brief delay between transitions
//         hasBegunMoving = true;
//
//         animationHelper.PlayAnimation(animationHelper.Walk, animationHelper.BaseLayer);
//     }
//     
//     [ContextMenu("Search")]
//     public void WanderOffSearch()
//     {
//         PathNNConstraint constraint = new PathNNConstraint();
//         constraint.graphMask = copyCatGraphMask;
//         constraint.walkable = true;
//         
//         GraphNode startNode = AstarPath.active.GetNearest(transform.position, constraint).node;
//         GraphNode furthestNode = FindFurthestPoint(startNode, playerHead.transform.position);
//         
//         ABPath abPath = ABPath.Construct(transform.position, (Vector3) furthestNode.position);
//         var path = seeker.StartPath(abPath, OnPathComplete);
//         path.BlockUntilCalculated();
//     }
//
//     private void OnPathComplete(Path p)
//     {
//         followerEntity.SetPath(p);
//     }
//
//     private GraphNode FindFurthestPoint(GraphNode startNode, Vector3 avoidancePosition, int maxDepth = 100)
//     {
//         Queue<(GraphNode node, int depth)> queue = new Queue<(GraphNode, int)>();
//         HashSet<GraphNode> visited = new HashSet<GraphNode>();
//
//         queue.Enqueue((startNode, 0));
//         visited.Add(startNode);
//
//         GraphNode furthestNode = startNode;
//         float maxDistance = Vector3.Distance((Vector3)startNode.position, avoidancePosition);
//
//         while (queue.Count > 0)
//         {
//             var (current, depth) = queue.Dequeue();
//
//             // Stop if we exceed max depth
//             if (depth > maxDepth)
//             {
//                 Debug.LogWarning("REACHED MAX FURTHEST");
//                 return furthestNode;
//             }
//
//             var connections = new List<GraphNode>();
//             current.GetConnections(connections.Add);
//
//             foreach (var neighbor in connections)
//             {
//                 if (!visited.Contains(neighbor))
//                 {
//                     visited.Add(neighbor);
//
//                     // Only proceed if the node is walkable and not part of the avoidance set
//                     if (neighbor.Walkable && copyCatGraphMask.Contains((int) neighbor.GraphIndex))
//                     {
//                         queue.Enqueue((neighbor, depth + 1));
//
//                         float distance = Vector3.Distance((Vector3)neighbor.position, avoidancePosition);
//                         if (distance > maxDistance)
//                         {
//                             maxDistance = distance;
//                             furthestNode = neighbor;
//                         }
//                     }
//                 }
//             }
//         }
//         return furthestNode;
//     }
//     
//     private void OnDrawGizmosSelected()
//     {
//         if (Application.isPlaying)
//         {
//             // Optional: Draw debug visuals here
//         }
//     }
//     */
// }