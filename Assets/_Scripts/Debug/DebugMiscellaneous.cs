using System.Collections.Generic;
using UnityEngine;

public class DebugMiscellaneous : MonoBehaviour, IDebugCommandSource
{
    [SerializeField] private GameObject directionalLight;
    
    public IEnumerable<DebugCommand> GetCommands()
    {
        yield return new DebugCommand(
            "timescale",
            "Sets Time Scale.",
            "timescale <timescale>",
            args =>
            {
                float timeScale = 1f;

                if (args.Length > 0)
                {
                    if (!float.TryParse(args[0], out timeScale) || timeScale < 0)
                    {
                        Debug.LogWarning("Invalid amount.");
                        return;
                    }
                }
                
                SetTimeScale(timeScale);
            }
            
        );
        
        yield return new DebugCommand(
            "brighten",
            "Toggles directional light.",
            "brighten",
            args =>
            {
                ToggleLight();
            }
            
        );
    }
    
    private void SetTimeScale(float timeScale = 1f)
    {
        Time.timeScale = timeScale;
    }

    private void ToggleLight()
    {
        directionalLight.SetActive(!directionalLight.activeSelf);
    }
}
