using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatTransformingState : State<CatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatAnimationHelper animationHelper;
    [SerializeField] private CapsuleCollider capsuleCollider;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed);
        followerEntity.isStopped = true;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        capsuleCollider.enabled = false;

        animationHelper.IsDynamicWalking = false;
        animationHelper.PlayAnimation(animationHelper.Transform, animationHelper.BaseLayer);
    }

    public override void ExitState()
    {
    }
}
