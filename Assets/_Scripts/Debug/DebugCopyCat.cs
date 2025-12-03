using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCopyCat : MonoBehaviour, IDebugCommandSource
{
    [SerializeField] private DebugCopyCatXRay xRay;
    
    public IEnumerable<DebugCommand> GetCommands()
    {
        yield return new DebugCommand(
            "copycat_radar",
            "Toggles CopyCat radar.",
            "copycat_radar",
            args =>
            {

                CopyCatRadar();
            }
            
        );
    }
    
    private void CopyCatRadar()
    {
        xRay.OnTogglePerformed();
    }
}
