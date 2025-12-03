using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEditor;
using UnityEngine;

public class CopyCatStalkingState : State<CopyCatStateMachine>
{
    
    [Header("References")] 
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private CopyCatStalkingToCopiedState stalkingToCopiedState;
    
    private GameObject _playerHead;
    [SerializeField] private GameObject targetCat;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stopDistance;
    [SerializeField] private float lazyUpdateTimer;
    [SerializeField] private float lazyUpdateTimerSet;
    
    [SerializeField] private bool isWaitingForOpportunity;
    [SerializeField] private float opportunityTimer;
    [SerializeField] private float opportunityTimerSet;
    
    
    [SerializeField] private float copyingRangeTolerance; // Tolerance extends further than range in case of cat moving
    
    private void Start()
    {
        _playerHead = ManagerPlayer.instance.PlayerHead;
    }

    public override void EnterState()
    {
        followerEntity.maxSpeed = movementSpeed;
        followerEntity.stopDistance = stopDistance;
        lazyUpdateTimer = lazyUpdateTimerSet;
        opportunityTimer = opportunityTimerSet;
        ProcessWeightedCat();
    }

    public override void ExitState()
    {
    }

    public override void FixedUpdateTick()
    {
        if (pathfindingHelper.RetreatCheck()) return;
        
        // Lazy tick rate
        lazyUpdateTimer = Mathf.Max(0, lazyUpdateTimer -= Time.fixedDeltaTime);
        if (lazyUpdateTimer <= 0)
        {
            LazyUpdate();
            lazyUpdateTimer = lazyUpdateTimerSet;
        }
        
        // If cats exist, but no valid target available, wait for an opportunity
        if (isWaitingForOpportunity)
        {
            animationHelper.PlayAnimation(animationHelper.Idle, animationHelper.BaseLayer);
            followerEntity.maxSpeed = 0;
            opportunityTimer = Mathf.Max(0, opportunityTimer -= Time.fixedDeltaTime);
            if (opportunityTimer <= 0)
            {
                ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming]);
                return;
            }
        } else
        {
            animationHelper.PlayAnimation(animationHelper.Walk, animationHelper.BaseLayer);
            followerEntity.maxSpeed = movementSpeed;
        }
        

        // Swaps place with a cat if close enough
        CatSwapCheck();
    }
    
    private void LazyUpdate()
    {
        ProcessWeightedCat();
        
        if (targetCat != null && !pathfindingHelper.IsPathPossible(targetCat.transform.position))
        {
            isWaitingForOpportunity = true;
            return;
        }
        
        if (isWaitingForOpportunity && targetCat != null && pathfindingHelper.IsPathPossible(targetCat.transform.position) && !pathfindingHelper.CanPlayerSeePosition(targetCat.transform.position))
        {
            isWaitingForOpportunity = false;
            return;
        }
    }

    private void ProcessWeightedCat()
    {
        GameObject weightedCat = GetWeightedCat();
        if (ManagerCat.instance.FoundCats.Count == 0)
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming]);
            return;
        }
        
        if (weightedCat == null)
        {
            isWaitingForOpportunity = true;
            //Debug.Log("weightedCat == null");
            return;
        }
        
        targetCat = weightedCat;
        aiDestinationSetter.target = weightedCat.transform;
    }
    
    private GameObject GetWeightedCat()
    {
        // Edge case, dev tool
        if (ManagerCat.instance.FoundCats.Count == 0)
        {
            Debug.LogWarning("No cats trailing player!");
            return null;
        }
        
        float furthestDistance = Mathf.NegativeInfinity;
        GameObject closestCat = null;
        foreach (GameObject cat in ManagerCat.instance.FoundCats)
        {
            if (pathfindingHelper.CanPlayerSeePosition(cat.transform.position)) continue;
            
            float distance = Vector3.Distance(cat.transform.position, _playerHead.transform.position);
            if (distance > furthestDistance)
            {
                closestCat = cat;
                furthestDistance = distance;
            }
        }
        
        return closestCat;
    }

    private void CatSwapCheck()
    {
        if (targetCat == null) return;
        if (Vector3.Distance(transform.position, targetCat.transform.position) > stopDistance + copyingRangeTolerance) return;
        if (pathfindingHelper.CanPlayerSeeCopyCat()) return;
        
        ManagerCopyCat.Instance.SetMimicCat(targetCat);
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.StalkingToCopied]);
        return;
    }
    
    
}
