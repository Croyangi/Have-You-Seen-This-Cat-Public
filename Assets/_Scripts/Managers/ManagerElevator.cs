using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerElevator : MonoBehaviour
{
    [field: SerializeField] public ElevatorHelper ElevatorHelper { get; private set; }
    [field: SerializeField] public ElevatorDoor WaitingRoomDoor { get; private set; }
    [field: SerializeField] public ElevatorDoor GameplayFloorDoor { get; private set; }
    [field: SerializeField] public Elevator Elevator { get; private set; }
    
    public static ManagerElevator Instance { get; private set; }

    public static Action OnArrived;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Elevator Manager in the scene.");
        }
        Instance = this;
    }

    private void OnEnable()
    {
    }
    
    private void OnDisable()
    {
        Elevator.OnExitElevator -= ExitElevator;
    }

    public void Arrived()
    {
        GameplayFloorDoor.Open();
        Elevator.OnExitElevator += ExitElevator;
        OnArrived?.Invoke();
    }

    private void ExitElevator()
    {
        GameplayFloorDoor.Close();
        ManagerGame.Instance.StartGame();
        Elevator.OnExitElevator -= ExitElevator;
    }
}
