using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatStateMachine : StateMachine<CatStateMachine>
{
    public enum CatStates
    {
        Lost, Idle, Following, Inspecting, RollCall, Transforming, Copied, Scattering, Collecting, CopiedCollecting
    }
    
    [SerializeField] private CatLostState lost;
    [SerializeField] private CatIdleState idle;
    [SerializeField] private CatFollowingState following;
    [SerializeField] private CatInspectingState inspecting;
    [SerializeField] private CatRollCallState rollCall;
    [SerializeField] private CatTransformingState transforming;
    [SerializeField] private CatCopiedState copied;
    [SerializeField] private CatScatteringState scattering;
    [SerializeField] private CatCollectingState collecting;
    [SerializeField] private CatCopiedCollectingState copiedCollecting;
    public Dictionary<CatStates, State<CatStateMachine>> CatStatesDictionary { get; private set; }

    private void Awake()
    {
        // Fill dictionary
        CatStatesDictionary = new Dictionary<CatStates, State<CatStateMachine>>
        {
            { CatStates.Idle, idle },
            { CatStates.Lost, lost },
            { CatStates.Following, following },
            { CatStates.Inspecting, inspecting },
            { CatStates.RollCall, rollCall },
            { CatStates.Transforming, transforming },
            { CatStates.Copied, copied },
            { CatStates.Scattering, scattering },
            { CatStates.Collecting, collecting },
            { CatStates.CopiedCollecting, copiedCollecting },
        };
        
        foreach (CatStates state in Enum.GetValues(typeof(CatStates)))
        {
            CatStatesDictionary[state].Initialize(this);
        }
        
        ForceState(CatStates.Lost);
    }
    
    public void ForceState(CatStates state)
    {
        if (CatStatesDictionary.TryGetValue(state, out var newState))
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
