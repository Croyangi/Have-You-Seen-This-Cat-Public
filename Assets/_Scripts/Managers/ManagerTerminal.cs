using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ManagerTerminal : MonoBehaviour
{
    // Manager
    public static ManagerTerminal instance { get; private set; }

    [SerializeField] private GameObject currentTerminal;
    [SerializeField] private GameObject currentMinigame;
    private ITerminalMinigame _currentITerminalMinigame;
    private ITerminal _currentITerminal;
    [SerializeField] private TerminalMinigameBase terminalMinigameBase;
    [SerializeField] private GameObject terminalMinigameBasePrefab;
    [field: SerializeField] public bool IsFocused { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Terminal Manager in the scene.");
        }
        instance = this;
    }
}
