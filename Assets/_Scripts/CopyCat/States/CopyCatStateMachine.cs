using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCatStateMachine : StateMachine<CopyCatStateMachine>
{
    public enum CopyCatStates
    {
        Idle, Roaming, Stalking, Retreat, Luring, LuringToCopied, StalkingToCopied, Copied, Transforming, Hunting, Charging, Searching, Jumpscare, Respawn
    }
    
    [SerializeField] private CopyCatIdleState idle;
    [SerializeField] private CopyCatRoamingState roaming;
    [SerializeField] private CopyCatStalkingState stalking;
    [SerializeField] private CopyCatRetreatState retreat;
    [SerializeField] private CopyCatLuringState luring;
    [SerializeField] private CopyCatLuringToCopiedState luringToCopied;
    [SerializeField] private CopyCatStalkingToCopiedState stalkingToCopied;
    [SerializeField] private CopyCatCopiedState copied;
    [SerializeField] private CopyCatTransformingState transforming;
    [SerializeField] private CopyCatHuntingState hunting;
    [SerializeField] private CopyCatChargingState charging;
    [SerializeField] private CopyCatSearchingState searching;
    [SerializeField] private CopyCatJumpscareState jumpscare;
    [SerializeField] private CopyCatRespawnState respawn;
    public Dictionary<CopyCatStates, State<CopyCatStateMachine>> CopyCatStatesDictionary { get; private set; }
    private void Awake()
    {
        // Fill dictionary
        CopyCatStatesDictionary = new Dictionary<CopyCatStates, State<CopyCatStateMachine>>
        {
            { CopyCatStates.Idle, idle },
            { CopyCatStates.Roaming, roaming },
            { CopyCatStates.Stalking, stalking },
            { CopyCatStates.Retreat, retreat },
            { CopyCatStates.Luring, luring },
            { CopyCatStates.LuringToCopied, luringToCopied },
            { CopyCatStates.StalkingToCopied, stalkingToCopied },
            { CopyCatStates.Copied, copied },
            { CopyCatStates.Transforming, transforming },
            { CopyCatStates.Hunting, hunting },
            { CopyCatStates.Charging, charging },
            { CopyCatStates.Searching, searching },
            { CopyCatStates.Jumpscare, jumpscare },
            { CopyCatStates.Respawn, respawn }
        };
        
        foreach (CopyCatStates state in Enum.GetValues(typeof(CopyCatStates)))
        {
            CopyCatStatesDictionary[state].Initialize(this);
        }
    }
    
    public void ForceState(CopyCatStates state)
    {
        if (CopyCatStatesDictionary.TryGetValue(state, out var newState))
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
