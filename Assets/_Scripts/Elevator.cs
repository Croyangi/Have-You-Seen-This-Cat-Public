using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public Action OnEnterElevator;
    public Action OnExitElevator;
    
    [field: SerializeField] public InteractableObject ElevatorTerminalInteractable { get; private set; }
    [field: SerializeField] public ElevatorTerminal ElevatorTerminal { get; private set; }

    public void InvokeOnEnterElevator()
    {
        OnEnterElevator?.Invoke();
    }
    
    public void InvokeOnExitElevator()
    {
        OnExitElevator?.Invoke();
    }
    
}
