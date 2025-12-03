using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatRoamingState : State<CopyCatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] private CopyCatSpawnPointHelper spawnpointHelper;
    [SerializeField] private CopyCatLuringState luringState;

    private State<CopyCatStateMachine> _previousState;
    private int _previousStateInARow;

    public bool isAlwaysStalking;

    [ContextMenu("Test")]
    public void Test()
    {
        followerEntity.isStopped = false;
    }

    public override void EnterState()
    {
        followerEntity.isStopped = true;
        animationHelper.PlayAnimation(animationHelper.Walk, animationHelper.BaseLayer);
        
        // Contains state transitions, be careful of internal enters and exits, like isStopped
        ProcessStateChange();
    }

    public override void ExitState()
    {
        followerEntity.isStopped = false;
    }

    public override void FixedUpdateTick()
    {
        if (pathfindingHelper.RetreatCheck()) return;
    }

    private void ProcessStateTransition(State<CopyCatStateMachine> state)
    {
        _previousStateInARow = _previousState == state ? _previousStateInARow + 1 : 0;
        _previousState = state;
    }

    // Basically flip a coin between stalking or luring
    private void ProcessStateChange()
    {
        // Increases stalking chance the more found cats
        bool shouldStalk = isAlwaysStalking;

        if (!shouldStalk && ManagerCat.instance.FoundCats.Count > 0)
        {
            int foundCats = ManagerCat.instance.FoundCats.Count;

            int weight = 0;
            if (_previousState == stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Stalking])
            {
                weight += _previousStateInARow;
            }

            int rand = UnityEngine.Random.Range(0, foundCats + 1);
            shouldStalk = rand > weight;
            //Debug.Log("Should Stalk?: "+ shouldStalk + " - (0, " + foundCats + ")=" + rand + " > " + weight);
        }

        State<CopyCatStateMachine> stalking = stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Stalking];
        State<CopyCatStateMachine> luring = stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Luring];
        
        if (shouldStalk || isAlwaysStalking)
        {
            ProcessStateTransition(stalking);
            ChangeState(stalking);
            return;
        }
        
        if (luringState.GetValidCatsAroundPlayer().Count > 0)
        {
            ProcessStateTransition(luring);
            ChangeState(luring);
            return;
        }
        
        if (ManagerCat.instance.FoundCats.Count > 0)
        {
            ProcessStateTransition(stalking);
            ChangeState(stalking);
            return;
        }

        StartCoroutine(NoGoalStandby());
    }

    [SerializeField] private float noGoalStandbyTimer = 5f;
    private IEnumerator NoGoalStandby()
    {
        yield return new WaitForSeconds(noGoalStandbyTimer);
        ProcessStateChange();
        
    }
}
