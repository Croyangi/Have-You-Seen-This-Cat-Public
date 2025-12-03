using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatCopiedState : State<CatStateMachine>
{
    [Header("References")]
    [SerializeField] private GameObject catObj;
    [SerializeField] private Cat cat;
    [field: SerializeField] public GameObject CatModel { get; private set; }

    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CatAnimationHelper animationHelper;

    [Header("Settings")]
    [SerializeField] private float stopDistance;
    [SerializeField] private float movementSpeed;

    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed); 
        //animationHelper.PlayAnimation(animationHelper.WaddleAnim, animationHelper.BaseLayer, 1f);
        ManagerCat.instance.RemoveCat(catObj);
        ManagerCat.instance.ResetCatChainAiTarget();
        followerEntity.stopDistance = stopDistance;
        
        cat.IsInspectable = false;
        cat.IsInteractable = false;
    }

    public override void ExitState()
    {

    }
}
