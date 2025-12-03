using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCatIdleState : State<CopyCatStateMachine>
{
    [Header("References")]
    [SerializeField] private CopyCatStateMachine stateMachine;
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    
    // DEAD STATE, JUST MEANT FOR SPAWNING PURPOSES

    public override void EnterState()
    {
        animationHelper.PlayAnimation(animationHelper.Idle, animationHelper.BaseLayer);
    }

    public override void ExitState()
    {
        
    }

    [ContextMenu("Switch To Roaming State")]
    public void SwitchToRoamingState()
    {
        //TransitionToState(stateMachine.GetState(CopyCatStateMachine.CopyCatStates.Roaming));
    }
    
    [ContextMenu("Switch To Hunting State")]
    public void SwitchToHuntingState()
    {
        //TransitionToState(stateMachine.GetState(CopyCatStateMachine.CopyCatStates.Hunting));
    }
}
