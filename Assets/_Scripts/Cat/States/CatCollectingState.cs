using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatCollectingState : State<CatStateMachine>
{
    
    [Header("References")] 
    [SerializeField] private Cat cat;
    [SerializeField] private bool isTransitioning;

    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatStateHelper stateHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CatAnimationHelper animationHelper;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    
    public override void EnterState()
    {
        cat.IsInteractable = false;
        
        movementHelper.SetMovementSpeed(movementSpeed);
        animationHelper.ToggleIdleAnimations(true);
    }

    public override void ExitState()
    {
        cat.IsInteractable = true;
        
        animationHelper.ToggleIdleAnimations(false);
    }
}
