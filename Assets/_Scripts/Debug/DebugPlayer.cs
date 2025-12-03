using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPlayer : MonoBehaviour, IDebugCommandSource
{
    [SerializeField] private bool isSpeedyPlayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stamina;
    
    public IEnumerable<DebugCommand> GetCommands()
    {
        yield return new DebugCommand(
            "player_speedy",
            "Toggles player movement hacks.",
            "player_speedy",
            args =>
            {
                StartCoroutine(ToggleSpeedyPlayer());
            }
            
        );
    }
    
    private IEnumerator ToggleSpeedyPlayer()
    {
        PlayerMovementHelper pmh = ManagerPlayer.instance.PlayerMovementHelper;
        isSpeedyPlayer = !isSpeedyPlayer;

        while (isSpeedyPlayer)
        {
            pmh.Stamina = stamina;
            pmh.MovementSpeed = movementSpeed;
            yield return null;
        }
    }
}
