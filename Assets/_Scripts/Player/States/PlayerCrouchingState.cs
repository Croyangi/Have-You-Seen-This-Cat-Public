using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchingState : State<PlayerStateMachine>
{
    
    [Header("References")]
    [SerializeField] private PlayerInputHelper inputHelper;
    [SerializeField] private PlayerMovementHelper movementHelper;
    
    private bool _hasPressedCrouch;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float staminaGain;

    [SerializeField] private float crouchTime;
    [SerializeField] private float standTime;

    public override void EnterState()
    {
        movementHelper.StaminaGain = staminaGain;
        movementHelper.SetMovementSpeed(movementSpeed);

        inputHelper.SetProcessing(false, inputHelper.Crouching, "crouching");
    }
    public override void ExitState()
    {
        inputHelper.SetProcessing(true, inputHelper.Crouching, "crouching");
    }

    public override void UpdateTick()
    {
        if (movementHelper.IsCrouchPressed && !_hasPressedCrouch)
        {
            _hasPressedCrouch = true;
            StopAllCoroutines();
            StartCoroutine(movementHelper.OnCrouch(crouchTime));
        }
        else if (movementHelper.IsCrouchPressed && _hasPressedCrouch)
        {
            _hasPressedCrouch = false;
        }
        
        // State transitions
        if (movementHelper.IsStanding)
        {
            ChangeState(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Idle]);
        }
    }
}
