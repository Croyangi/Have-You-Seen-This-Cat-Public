using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class State<TMachine> : MonoBehaviour where TMachine : StateMachine<TMachine>
{
    protected TMachine stateMachine;

    public virtual void Initialize(TMachine machine)
    {
        stateMachine = machine;
    }
    
    public abstract void EnterState();
    public abstract void ExitState();

    protected void ChangeState(State<TMachine> next, Action customExit = null)
    {
        stateMachine.RequestStateChange(next, customExit);
    }
    
    public virtual void FixedUpdateTick()
    {
        // Default implementation
    }

    public virtual void UpdateTick() 
    {
        // Default implementation
    }
}
