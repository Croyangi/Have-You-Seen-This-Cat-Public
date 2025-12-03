using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkingState : State<PlayerStateMachine>
{
    
    [Header("References")]
    [SerializeField] private PlayerInputHelper inputHelper;
    [SerializeField] private PlayerMovementHelper movementHelper;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float staminaGain;


    public override void EnterState()
    {
        movementHelper.StaminaGain = staminaGain;
        movementHelper.SetMovementSpeed(movementSpeed);
    }
    public override void ExitState()
    {

    }

    public override void FixedUpdateTick()
    {
        
    }

    public override void UpdateTick()
    {
        // State transitions
        if (movementHelper.IsCrouchPressed)
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Crouching]);
        }

        // IDLE
        if (!movementHelper.InputMovementCheck())
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Idle]);
        }

        // RUNNING
        if (movementHelper.InputMovementCheck() && movementHelper.IsRunningPressed && !movementHelper.IsExhausted)
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Running]);
        }
    }
}
