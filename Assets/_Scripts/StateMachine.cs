using System;
using UnityEngine;

public abstract class StateMachine<TMachine> : MonoBehaviour where TMachine : StateMachine<TMachine>
{
    public string currentStateName;
    public State<TMachine> CurrentState { get; private set; }
    private bool _isTransitioning;
    private State<TMachine> _pendingState;
    private Action _pendingCustomExit;

    public void RequestStateChange(State<TMachine> next, Action customExit = null)
    {
        if (_isTransitioning)
        {
            _pendingState = next;
            _pendingCustomExit = customExit;
            return;
        }

        _isTransitioning = true;

        if (customExit != null)
            customExit.Invoke();
        else
            CurrentState?.ExitState();

        CurrentState = next;
        CurrentState.EnterState();
        currentStateName = CurrentState.name;

        _isTransitioning = false;

        if (_pendingState != null)
        {
            var ps = _pendingState;
            var pc = _pendingCustomExit;
            _pendingState = null;
            _pendingCustomExit = null;
            RequestStateChange(ps, pc);
        }
    }
}
