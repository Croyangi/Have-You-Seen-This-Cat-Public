using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine<PlayerStateMachine>
{
    public enum PlayerStates
    {
        Idle, Walking, Running, Crouching, Inspecting, Jumpscare
    }
    
    [SerializeField] private PlayerIdleState idle;
    [SerializeField] private PlayerWalkingState walking;
    [SerializeField] private PlayerRunningState running;
    [SerializeField] private PlayerCrouchingState crouching;
    [SerializeField] private PlayerInspectingState inspecting;
    [SerializeField] private PlayerJumpscareState jumpscare;
    public Dictionary<PlayerStates, State<PlayerStateMachine>> PlayerStatesDictionary;

    private void Awake()
    {
        // Fill dictionary
        PlayerStatesDictionary = new Dictionary<PlayerStates, State<PlayerStateMachine>>
        {
            { PlayerStates.Idle, idle },
            { PlayerStates.Walking, walking },
            { PlayerStates.Running, running },
            { PlayerStates.Crouching, crouching },
            { PlayerStates.Inspecting, inspecting },
            { PlayerStates.Jumpscare, jumpscare },
        };

        foreach (PlayerStates state in Enum.GetValues(typeof(PlayerStates)))
        {
            PlayerStatesDictionary[state].Initialize(this);
        }
        
        ForceSetState(PlayerStates.Idle);
    }
    
    public void ForceSetState(PlayerStates state)
    {
        if (PlayerStatesDictionary.TryGetValue(state, out var newState))
        {
            RequestStateChange(newState);
        }
        else
        {
            Debug.LogWarning("Invalid state: " + state);
        }
    }

    private void FixedUpdate()
    {
        CurrentState?.FixedUpdateTick();
    }

    private void Update()
    {
        CurrentState?.UpdateTick();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && CurrentState != null)
        {
            Gizmos.color = Color.white;
            DrawStringGizmo.DrawString(CurrentState.GetType().Name,
                transform.position + Vector3.up, Gizmos.color, new Vector2(0.5f, 0.5f), 20f);
        }
    }
}
