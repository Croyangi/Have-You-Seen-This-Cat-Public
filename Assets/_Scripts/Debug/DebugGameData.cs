using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGameData : MonoBehaviour, IDebugCommandSource
{
    public IEnumerable<DebugCommand> GetCommands()
    {
        yield return new DebugCommand(
            "data_reset",
            "Resets all game data.",
            "data_reset",
            args =>
            {
                ResetGameData();
            }
            
        );
    }
    
    private void ResetGameData()
    {
        ManagerDataPersistence.Instance.NewGame();
    }
}
