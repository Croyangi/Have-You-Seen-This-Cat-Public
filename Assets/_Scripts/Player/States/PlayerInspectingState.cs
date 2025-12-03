using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PlayerInspectingState : State<PlayerStateMachine>
{
    
    [Header("References")]
    [SerializeField] private PlayerInputHelper inputHelper;
    [SerializeField] private PlayerMovementHelper movementHelper;
    [SerializeField] private PlayerInteractHelper interactHelper;
    [SerializeField] private PlayerInspectingHelper inspectingHelper;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float staminaGain;

    public override void EnterState()
    {
        if (!inspectingHelper.CanFreeMoveInspect)
        {
            inputHelper.SetProcessing(false, inputHelper.Terminal, "terminal");
        }
        
        movementHelper.SetMovementSpeed(movementSpeed);
        movementHelper.StaminaGain = staminaGain;
        
        // Begin
        inspectingHelper.OnStartInspection();

        // Shouldn't roll call during inspection
        inputHelper.SetProcessing(false, inputHelper.RollCall, "inspectingState");

    }
    public override void ExitState()
    {
        inputHelper.SetProcessing(true, inputHelper.Terminal, "terminal");
        
        // End
        inspectingHelper.OnEndInspection();
        
        // Resume functions
        inputHelper.SetProcessing(true, inputHelper.RollCall, "inspectingState");
    }

    public override void UpdateTick()
    {
        inspectingHelper.InspectingUpdate();
        
        // State transitions
        if (!inspectingHelper.IsInspecting || inspectingHelper.InspectedObject == null)
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Idle]);
        }
    }
}
