using System.Collections;
using System.Collections.Generic;
using Febucci.UI.Core;
using Pathfinding;
using UnityEditor;
using UnityEngine;

public class CopyCatHidingState : MonoBehaviour
{
    // [Header("References")]
    // [SerializeField] private CopyCatStateMachine stateMachine;
    // [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    // [SerializeField] private CopyCatAnimationHelper animationHelper;
    // [SerializeField] private FollowerEntity followerEntity;
    // [SerializeField] private Seeker seeker;
    //
    // [Header("Settings")]
    // [SerializeField] private float hesitation;
    // [SerializeField] private float hesitationSet;
    // [SerializeField] private float fakeOutCount;
    // [SerializeField] private float maxFakeOutCount;
    // [SerializeField] private float fakeOutMultiplier;
    // [SerializeField] private bool isFakedOut;
    // [SerializeField] private bool hasCheckedPath;
    //
    // [SerializeField] private GameObject playerHead;
    //
    // [SerializeField] private bool hasLOS;
    // private void Start()
    // {
    //     playerHead = ManagerPlayer.instance.PlayerHead;
    // }
    //
    // public void EnterState()
    // {
    //     hesitation = hesitationSet;
    //     //patience = patienceSet;
    //     fakeOutCount = 0;
    //     isFakedOut = false;
    //     
    //     animationHelper.PlayAnimation(animationHelper.Idle, animationHelper.BaseLayer);
    //     
    //     // Stay stagnant until further notice
    //     //pathfindingHelper.SetAIDestinationWithPosition(transform.position);
    // }
    //
    // public void ExitState()
    // {
    // }
    //
    // public void FixedUpdateState()
    // {
    //     if (pathfindingHelper.RetreatCheck()) return;
    //     
    //     // Pathfind to hiding position
    //     GameObject cat = pathfindingHelper.GetFurthestCatFromPlayer();
    //     if (cat == null)
    //     {
    //         TransitionToState(stateMachine.GetState(CopyCatStateMachine.CopyCatStates.Retreat));
    //         return;
    //     }
    //
    //     if (hasCheckedPath)
    //     {
    //         hasCheckedPath = false;
    //         seeker.StartPath(transform.position, cat.transform.position, OnPathComplete);
    //     }
    //
    //     if (seeker.IsDone())
    //     {
    //         FakeOutCheck();
    //         hasCheckedPath = true;
    //     }
    // }
    //
    // private void FakeOutCheck()
    // {
    //     // Can't fake it out
    //     bool isInFOV = pathfindingHelper.IsInFOV(transform.position);
    //     if (!isInFOV || !hasLOS)
    //     {
    //         hesitation = Mathf.Clamp(hesitation -= Time.fixedDeltaTime, 0, hesitationSet + (fakeOutCount * fakeOutMultiplier));
    //         isFakedOut = false;
    //     }
    //     else if (!isFakedOut)
    //     {
    //         hesitation = hesitationSet + (fakeOutCount * fakeOutMultiplier);
    //         fakeOutCount = Mathf.Clamp(++fakeOutCount, 0, maxFakeOutCount);
    //         isFakedOut = true;
    //     }
    // }
    //
    // private void OnPathComplete(Path path) 
    // {
    //     if (path.error) 
    //     {
    //         Debug.LogError("Path failed: " + path.errorLog);
    //         return;
    //     }
    //     
    //     var abPath = path as ABPath; // Cast path
    //     seeker.PostProcess(abPath); // Simplify path, so we're not working with 40000 nodes, (PRE-CONDITION: NEED MODIFIERS ON OBJ)
    //    
    //    //Vector3 lookAheadPoint = pathfindingHelper.FindPointUnitsForward(abPath.vectorPath, pathfindingHelper.LookAheadDistance);
    //    //hasLOS = pathfindingHelper.IsInLOS(lookAheadPoint, playerHead.transform.position);
    // }
    //
    // public override void UpdateState()
    // {
    //     if (hesitation <= 0)
    //     {
    //         TransitionToState(stateMachine.GetState(CopyCatStateMachine.CopyCatStates.Stalking));
    //     }
    // }
}
