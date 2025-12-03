using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = System.Random;

public class ManagerTablet : MonoBehaviour
{
    public static ManagerTablet Instance { get; private set; }
    
    [field: SerializeField] public TabletAppMimicModifiers TabletAppMimicModifiers { get; private set; }
    [field: SerializeField] public bool IsShowing { get; private set; }
    private List<ITabletApp> _tabletApps;
    [SerializeField] private int index;
    [SerializeField] private List<Rectangle> appOutlines;
    [SerializeField] private GameObject UICamera;
    [SerializeField] private bool isInApp;
    
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;
    
    [SerializeField] private AudioClip modemBeep;
    
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Manager Tablet in the scene.");
        }
        Instance = this;
        
        _playerInput = new PlayerInput();
        
        _tabletApps = new List<ITabletApp>(this.GetComponentsInChildren<ITabletApp>());
        for (int i = 0; i < _tabletApps.Count; i++)
        {
            appOutlines[i].Color = i == index ? selectedColor : unselectedColor;
        }
    }

    private void Start()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        UICamera.transform.localPosition = Vector3.zero;
        index = 0;
        isInApp = true;
        UnsubscribeToActions();
        OnHideTablet();
    }
    
    private void OnEnable()
    {
        _playerInput.Enable();
    }
    
    private void OnDisable()
    {
        _playerInput.Disable();
        UnsubscribeToActions();
    }
    
    private void OnOperatePerformed(InputAction.CallbackContext value)
    {
        if (isInApp) return;
        
        Vector2 input = value.ReadValue<Vector2>();

        if (input.x > 0)
        {
            index = (index + 1) % _tabletApps.Count;
            
            Vector3 pos = ManagerPlayer.instance.PlayerTabletHelper.Tablet.transform.position;
            ManagerSFX.Instance.PlaySFX(modemBeep, pos, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        } else if (input.x < 0)
        {
            index = (index - 1 + _tabletApps.Count) % _tabletApps.Count;
            Vector3 pos = ManagerPlayer.instance.PlayerTabletHelper.Tablet.transform.position;
            ManagerSFX.Instance.PlaySFX(modemBeep, pos, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        }

        for (int i = 0; i < _tabletApps.Count; i++)
        {
            appOutlines[i].Color = i == index ? selectedColor : unselectedColor;
        }
    }

    private void OnEnterPerformed(InputAction.CallbackContext value)
    {
        if (isInApp) return;
        
        Vector3 pos = ManagerPlayer.instance.PlayerTabletHelper.Tablet.transform.position;
        ManagerSFX.Instance.PlaySFX(modemBeep, pos, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        ITabletApp app = _tabletApps[index];
        UICamera.transform.localPosition = app.GetCameraPos();
        app.OnShow();
        isInApp = true;
    }
    
    private void OnExitPerformed(InputAction.CallbackContext value)
    {
        if (!isInApp) return;
        
        Vector3 pos = ManagerPlayer.instance.PlayerTabletHelper.Tablet.transform.position;
        ManagerSFX.Instance.PlaySFX(modemBeep, pos, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        ITabletApp app = _tabletApps[index];
        UICamera.transform.localPosition = app.GetCameraPos();
        app.OnHide();
        isInApp = false;
        UICamera.transform.localPosition = Vector3.zero;
    }

    public void OnToggleTablet()
    {
        IsShowing = !IsShowing;
        if (IsShowing)
        {
            SubscribeToActions();
            OnShowTablet();
        }
        else
        {
            UnsubscribeToActions();
            OnHideTablet();
        }
    }

    private void SubscribeToActions()
    {
        //_playerInput.Tablet.Operate.performed += OnOperatePerformed;
        
        //_playerInput.Tablet.Enter.performed += OnEnterPerformed;
        //_playerInput.Tablet.Exit.performed += OnExitPerformed;
    }

    private void UnsubscribeToActions()
    {
        _playerInput.Tablet.Operate.performed -= OnOperatePerformed;
        
        _playerInput.Tablet.Enter.performed -= OnEnterPerformed;
        _playerInput.Tablet.Exit.performed -= OnExitPerformed;
    }

    private void OnShowTablet()
    {
        if (!isInApp) return;
        
        ITabletApp app = _tabletApps[index];
        UICamera.transform.localPosition = app.GetCameraPos();
        app.OnShow();
    }

    private void OnHideTablet()
    {
        if (!isInApp) return;
        
        ITabletApp app = _tabletApps[index];
        app.OnHide();
    }
}
