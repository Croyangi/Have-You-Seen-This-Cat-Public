using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatInvestigatingState : MonoBehaviour
{
    // [Header("Building Block References")]
    // [SerializeField] private CopyCatStateMachine stateMachine;
    // [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    // [SerializeField] private FollowerEntity followerEntity;
    // [SerializeField] private Seeker seeker;
    // [SerializeField] private CopyCatAnimationHelper animationHelper;
    //
    // [Header("References")]
    // [SerializeField] private CopyCatSearchingState searchingState;
    //
    // [Header("Settings")]
    // [SerializeField] private float lookingAroundTimer;
    // [SerializeField] private float lookingAroundTimerSet;
    //
    // public override void EnterState()
    // {
    //     
    //     lookingAroundTimer = lookingAroundTimerSet;
    //     
    //     animationHelper.PlayAnimation(animationHelper.Roar, animationHelper.BaseLayer, forcePlay: true);
    // }
    //
    // public override void ExitState()
    // {
    // }
    //
    // public override void FixedUpdateState()
    // {
    //     searchingState.WanderOffTimerUpdate();
    //     
    //     ////
    //     lookingAroundTimer = Mathf.Max(0f, lookingAroundTimer -= Time.fixedDeltaTime);
    //     if (lookingAroundTimer <= 0)
    //     {
    //         TransitionToState(stateMachine.GetState(CopyCatStateMachine.CopyCatStates.Searching));
    //         return;
    //     }
    //     
    // }
    //
    //
    // private void OnDrawGizmosSelected()
    // {
    //     if (Application.isPlaying && stateMachine.currentState == this)
    //     {
    //         
    //     }
    // }
}
