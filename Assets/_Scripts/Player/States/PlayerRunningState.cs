using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunningState : State<PlayerStateMachine>
{
    [Header("References")]
    [SerializeField] private PlayerInputHelper inputHelper;
    [SerializeField] private PlayerMovementHelper movementHelper;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float staminaGain;


    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed);
        movementHelper.StaminaGain = staminaGain;
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

        // IF NOT RUNNING CONDITIONS
        if ((!movementHelper.IsRunningPressed || !movementHelper.InputMovementCheck()))
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Walking]);
        }

        // IF NOT ENOUGH STAMINA
        if (movementHelper.Stamina <= 0)
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Walking]);
        }
    }
}
