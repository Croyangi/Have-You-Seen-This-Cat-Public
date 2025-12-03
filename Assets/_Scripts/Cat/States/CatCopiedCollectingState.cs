using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatCopiedCollectingState : State<CatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;

    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private CatAnimationHelper animationHelper;
    [SerializeField] private FollowerEntity followerEntity;
    

    private LayerMask _layerMask;
    [SerializeField] private LayerMask cullLayer;
    
    public override void EnterState()
    {
        animationHelper.PlayAnimation(animationHelper.SitAnim, animationHelper.BaseLayer);
        movementHelper.SetMovementSpeed(0f);
        animationHelper.IsDynamicWalking = false;
        aiDestinationSetter.target = null;
        aiDestinationSetter.enabled = false;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        _layerMask = capsuleCollider.excludeLayers;
        capsuleCollider.excludeLayers = cullLayer;
    }

    public override void ExitState()
    {
        animationHelper.IsDynamicWalking = true;
        aiDestinationSetter.enabled = true;
        
        capsuleCollider.excludeLayers = _layerMask;
    }
}
