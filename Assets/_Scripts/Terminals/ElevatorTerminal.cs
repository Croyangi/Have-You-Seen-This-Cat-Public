using System.Collections.Generic;
using UnityEngine;

public class ElevatorTerminal : Terminal
{
    public void Interact()
    {
        if (isUnlocked)
        {
            TerminalUnlockedInteraction();
        }
        else if (!isTransitioning)
        {
            if (IsOn)
            {
                TerminalLockedInteraction();
            }
            else
            {
                TerminalBootStart();                
            }
        }
    }
}
