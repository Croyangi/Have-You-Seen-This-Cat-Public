using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHelper : MonoBehaviour
{
    [Header("Technical References")]
    private PlayerInput _playerInput;

    // Each IProcessable maps to a dictionary of sources -> bool (true = enabled, false = blocked)
    private Dictionary<IProcessable, Dictionary<string, bool>> _processablesState = new();

    [Header("Getters")]
    [field: SerializeField] public PlayerCameraHelper CameraHelper { get; private set; }
    [field: SerializeField] public PlayerInteractHelper InteractHelper { get; private set; }
    [field: SerializeField] public PlayerInspectingHelper InspectingHelper { get; private set; }
    [field: SerializeField] public PlayerMovementHelper MovementHelper { get; private set; }
    [field: SerializeField] public PlayerTabletHelper TabletHelper { get; private set; }
    [field: SerializeField] public PlayerFlashlightHelper FlashlightHelper { get; private set; }
    [field: SerializeField] public PlayerCatHelper CatHelper { get; private set; }
    [field: SerializeField] public PlayerVisibilityHelper VisibilityHelper { get; private set; }
    [field: SerializeField] public PlayerPauseHelper PlayerPauseHelper { get; private set; }

    public List<IProcessable> All { get; private set; }
    public List<IProcessable> Terminal { get; private set; }
    public List<IProcessable> Tablet { get; private set; }
    public List<IProcessable> Crouching { get; private set; }
    public List<IProcessable> RollCall { get; private set; }

    [ContextMenu("Debug Processing States")]
    public void DebugProcessingStates()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Processing State Debug ===");

        foreach (var kvp in _processablesState)
        {
            IProcessable ip = kvp.Key;
            var sources = kvp.Value;

            string state = ip.IsProcessing ? "ENABLED" : "BLOCKED";

            string sourcesList = sources.Count > 0
                ? string.Join(", ", sources.Select(s => $"{s.Key}:{(s.Value ? "T" : "F")}"))
                : "None";

            sb.AppendLine($"[{kvp.Key}] State: {state}, Sources: {sourcesList}");
        }

        Debug.Log(sb.ToString());
    }

    private void Awake()
    {
        // INIT
        All = new List<IProcessable> { CameraHelper, InteractHelper, InspectingHelper, MovementHelper, TabletHelper, FlashlightHelper, VisibilityHelper, PlayerPauseHelper };
        Terminal = new List<IProcessable> { CameraHelper, MovementHelper, TabletHelper, CatHelper };
        Tablet = new List<IProcessable> { InteractHelper, InspectingHelper };
        Crouching = new List<IProcessable> { InteractHelper };
        RollCall = new List<IProcessable> { CatHelper };

        foreach (IProcessable processable in All)
        {
            _processablesState.Add(processable, new Dictionary<string, bool>());
        }

        InitializeProcessing(); // Force-enable everything on start
    }

    public void SetProcessing(bool state, List<IProcessable> processables, string source, bool isOverride = false)
    {
        foreach (IProcessable ip in processables)
        {
            ApplyState(state, ip, source.ToLower(), isOverride);
            ip.OnProcessingChanged();
        }
    }

    private void InitializeProcessing()
    {
        foreach (IProcessable ip in All)
        {
            _processablesState[ip].Clear();
            ip.IsProcessing = true; // default: enabled when no sources exist
        }
    }

    // Applies state with explicit source tracking
    private void ApplyState(bool state, IProcessable ip, string source, bool isOverride = false)
    {
        if (!_processablesState.ContainsKey(ip))
            _processablesState[ip] = new Dictionary<string, bool>();

        if (isOverride)
        {
            _processablesState[ip].Clear();
            _processablesState[ip][source] = state;
            ip.IsProcessing = state;
            return;
        }

        _processablesState[ip][source] = state;

        // Resolution rule: enabled if ALL sources are true
        ip.IsProcessing = !_processablesState[ip].ContainsValue(false);
    }
}