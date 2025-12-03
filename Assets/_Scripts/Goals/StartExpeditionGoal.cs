using UnityEngine;

public class StartExpeditionGoal : Goal
{
    private void OnEnable()
    {
        ManagerElevator.Instance.Elevator.ElevatorTerminal.OnTerminalUnlocked += OnComplete;
    }

    public override void OnShow()
    {
        base.OnShow();
        SetText();
    }

    private void SetText()
    {
        string text = goalText;
        OnUpdateText?.Invoke(this, text);
    }
}
