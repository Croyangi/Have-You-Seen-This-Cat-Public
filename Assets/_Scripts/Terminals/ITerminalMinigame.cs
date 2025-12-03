using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITerminalMinigame 
{
    public void OnMinigameStart(TerminalMinigameBase minigameBase);
    public void OnMinigameEnd();
    public void OnMinigameFocus();
    public void OnMinigameUnfocus();

    public ITerminalMinigameResult GetResult()
    {
        return null;
    }
}