using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatRollCallState : State<CatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatStateHelper stateHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CatAnimationHelper animationHelper;
    [SerializeField] private GameObject cat;
    [SerializeField] private bool isInPosition;

    [Header("Settings")] 
    [SerializeField] private float stopDistance;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float slerpScale;
    [SerializeField] private float playerStareDistance;

    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed);
        isInPosition = false;
        followerEntity.stopDistance = stopDistance;
    }

    public override void ExitState()
    {
        animationHelper.ToggleIdleAnimations(false);
    }

    public override void FixedUpdateTick()
    {
        Vector3 playerPosition = ManagerPlayer.instance.PlayerObj.transform.position;
        float playerDistance = Vector3.Distance(cat.transform.position, playerPosition);
        
        if (followerEntity.reachedEndOfPath)
        {
            if (playerStareDistance >= playerDistance)
            {
                playerPosition.y = cat.transform.position.y;
                Quaternion targetRotation = Quaternion.LookRotation(playerPosition - cat.transform.position, Vector3.up);
                cat.transform.rotation = Quaternion.Slerp(cat.transform.rotation, targetRotation, Time.fixedDeltaTime * slerpScale);
            }
            
            if (!isInPosition)
            {
                isInPosition = true;
                animationHelper.PlayAnimation(animationHelper.StillAnim, animationHelper.BaseLayer, 0.3f);
                animationHelper.ToggleIdleAnimations(true);
            }
        }
        else
        {
            isInPosition = false;
            //animationHelper.PlayAnimation(animationHelper.WaddleAnim, animationHelper.BaseLayer, 0.3f);
            animationHelper.ToggleIdleAnimations(false);
        }
    }
}
