using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatIdleState : State<CatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatStateHelper stateHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CatAnimationHelper animationHelper;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    
    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed);
        animationHelper.ToggleIdleAnimations(true);
    }

    public override void ExitState()
    {
        animationHelper.ToggleIdleAnimations(false);
    }

    public override void UpdateTick()
    {
        if (!followerEntity.reachedEndOfPath)
        {
            ChangeState(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.Following]);
        }
    }
}
