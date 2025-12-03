using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : State<PlayerStateMachine>
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

    public override void UpdateTick()
    {
        if (movementHelper.IsCrouchPressed)
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Crouching]);
        }
        
        if (movementHelper.InputMovementCheck())
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Walking]);
        }
    }
}
