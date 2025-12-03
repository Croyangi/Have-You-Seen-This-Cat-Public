using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerUIHelper : MonoBehaviour
{
    [Header("Technical References")]
    private PlayerInput _playerInput;

    private Dictionary<IUI, Dictionary<string, bool>> _uiStates = new();

    [Header("Getters")] 
    [SerializeField] private Canvas PPCanvas;
    [field: SerializeField] public GenericUI StaminaBar { get; private set; }
    [field: SerializeField] public CrosshairUI Crosshair { get; private set; }
    [field: SerializeField] public GenericUI RollCall { get; private set; }
    [field: SerializeField] public GenericUI Tablet { get; private set; }
    [field: SerializeField] public GenericUI HoverFlavorText { get; private set; }
    [field: SerializeField] public GenericUI InspectFlavorText { get; private set; }
    [field: SerializeField] public VisibilityEyesUI VisibilityEyes { get; private set; }
    [field: SerializeField] public GenericUI CameraOverlay { get; private set; }
    [field: SerializeField] public CameraOverlayUI CameraOverlayExtras { get; private set; }
    [field: SerializeField] public GenericUI Dialogue { get; private set; }
    
    public List<IUI> All { get; private set; }
    public List<IUI> Gameplay { get; private set; }
    public List<IUI> Terminal { get; private set; }
    
    
    
    [ContextMenu("Debug UI States")]
    public void DebugUIStates()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== UI State Debug ===");

        foreach (var kvp in _uiStates)
        {
            IUI ui = kvp.Key;
            var sources = kvp.Value;

            string state = ui.IsVisible ? "VISIBLE" : "HIDDEN";

            string sourcesList = sources.Count > 0
                ? string.Join(", ", sources.Select(s => $"{s.Key}:{(s.Value ? "T" : "F")}"))
                : "None";

            sb.AppendLine($"[{kvp.Key}] State: {state}, Sources: {sourcesList}");
        }

        Debug.Log(sb.ToString());
    }

    private void Awake()
    {
        _uiStates.Clear();
        
        // INIT
        All = new List<IUI> { StaminaBar, Crosshair, RollCall, Tablet, HoverFlavorText, InspectFlavorText, VisibilityEyes, CameraOverlay, CameraOverlayExtras, Dialogue };
        Gameplay = new List<IUI>() { RollCall, Tablet, VisibilityEyes };
        Terminal = new List<IUI>() { Crosshair, RollCall, Tablet, VisibilityEyes, CameraOverlayExtras };

        string debug = "";
        foreach (IUI ui in All)
        {
            _uiStates.Add(ui, new Dictionary<string, bool>());
            debug += ui + "\n";
        }
        //Debug.Log(debug);
        
        InitializeVisibility();
        PPCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.tag == "UICamera" || cam.gameObject.name == "UI")
            {
                PPCanvas.worldCamera = cam;
                return;
            }
        }
    }

    private void Start()
    {
        SetVisibility(false, Gameplay, "game");
    }

    private void OnDestroy()
    {
        All.Clear();
        Gameplay.Clear();
        Terminal.Clear();
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += GameStart;
        ManagerGame.OnGameEnd += GameEnd;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= GameStart;
        ManagerGame.OnGameEnd -= GameEnd;
    }

    public bool GetVisibility(IUI ui)
    {
        return ui.IsVisible;
    }

    private void GameStart()
    {
        SetVisibility(true, Gameplay, "game");
    }

    private void GameEnd()
    {
        SetVisibility(false, Gameplay, "game");
    }

    public void SetVisibility(bool state, List<IUI> uis, string source, bool isOverride = false)
    {
        foreach (IUI ui in uis)
        {
            if (ui != null)
            {
                ApplyState(state, ui, source.ToLower(), isOverride);
                ui.OnVisibilityChanged();
            }
        }
    }
    
    private bool _switch = false;
    [ContextMenu("Test")] 
    public void TestToggle()
    {
        SetVisibility(_switch, All, "testUIHelper");
        _switch = !_switch;
    }
    
    private void InitializeVisibility()
    {
        foreach (IUI ui in All)
        {
            _uiStates[ui].Clear();
            ui.IsVisible = _uiStates[ui].Count == 0;
        }
    }

    // Applies state with consideration to every other queued state, only for false to true
    private void ApplyState(bool state, IUI ui, string source, bool isOverride = false)
    {
        if (!_uiStates.ContainsKey(ui))
            _uiStates[ui] = new Dictionary<string, bool>();

        if (isOverride)
        {
            // Overrides ignore all other sources
            _uiStates[ui].Clear();
            _uiStates[ui][source] = state;
            ui.IsVisible = state;
            return;
        }

        // Always record the intent of this source
        _uiStates[ui][source] = state;

        // Final resolution rule: UI is visible if *all* sources say true
        ui.IsVisible = !_uiStates[ui].ContainsValue(false);
    }
}
