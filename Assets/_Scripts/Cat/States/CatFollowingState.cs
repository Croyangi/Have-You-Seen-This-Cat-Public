using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatFollowingState : State<CatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatStateHelper stateHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CatAnimationHelper animationHelper;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float movementSpeedScaling;
    [SerializeField] private float basePlayerDistanceDifference;
    [SerializeField] private float currentMovementSpeed;

    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed);
    }

    public override void ExitState()
    {

    }

    public override void FixedUpdateTick()
    {
        float distanceExceeded = Mathf.Clamp(followerEntity.remainingDistance - basePlayerDistanceDifference, 0f, 99999f);
        currentMovementSpeed = movementSpeed + (distanceExceeded * movementSpeedScaling);
        movementHelper.SetMovementSpeed(currentMovementSpeed);
    }

    public override void UpdateTick()
    {
        if (followerEntity.reachedEndOfPath)
        {
            ChangeState(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.Idle]);
        }
    }
}
