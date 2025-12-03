using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCatHelper : MonoBehaviour, IProcessable
{

    [Header("References")]
    private ManagerCat _managerCat;
    private ManagerGame _managerGame;
    [SerializeField] private PlayerUIHelper uiHelper;
    
    [Header("Settings")]
    [field: SerializeField] public bool IsProcessing { get; set; }

    [field: SerializeField] public bool HasObtainedRollCall { get; set; }
    [SerializeField] private float rollCallCooldown;
    [SerializeField] private float rollCallCooldownTimer;


    private PlayerInput _playerInput;
    private void Awake()
    {
        // Instantiate new Unity's Input System
        _playerInput = new PlayerInput();
        ObtainRollCall();
    }

    private void Start()
    {
        _managerCat = ManagerCat.instance;
        _managerGame = ManagerGame.Instance;
        if (_managerCat != null)
        {
            _managerCat.OnRollCall(false);
        }
    }
    
    private void GameStart()
    {
        ProcessObtainCheck();
    }

    private void OnEnable()
    {
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        UnsubscribeToInput();
        _playerInput.Disable();
    }
    
    private void SubscribeToInput()
    {
        ManagerGame.OnGameStart += GameStart;
        _playerInput.Player.RollCall.performed += OnInteractPerformed;
    }
    
    private void UnsubscribeToInput()
    {
        ManagerGame.OnGameStart -= GameStart;
        _playerInput.Player.RollCall.performed -= OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing || _managerCat.FoundCats.Count == 0 || rollCallCooldownTimer > 0) return;
        OnRollCall();
    }
    
    [ContextMenu("Obtain Roll Call Ability")]
    public void ObtainRollCall()
    {
        HasObtainedRollCall = true;
        ProcessObtainCheck();
    }

    [ContextMenu("Process Roll Call Check")]
    public void ProcessObtainCheck()
    {
        uiHelper.SetVisibility(HasObtainedRollCall, new List<IUI> { uiHelper.RollCall }, "catHelper");
        if (HasObtainedRollCall)
        {
            SubscribeToInput();
        }
        else
        {
            UnsubscribeToInput();
        }
    }


    [SerializeField] private Image rollCallImage;
    [SerializeField] private TextMeshProUGUI rollCallHotkeyText;
    [SerializeField] private Color disabledColor;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color activeColor;

    private void FixedUpdate()
    {
        if (!IsProcessing) return;

        if (rollCallCooldownTimer > 0)
        {
            rollCallCooldownTimer = Math.Max(0, rollCallCooldownTimer - Time.fixedDeltaTime);
        }
        
        if (!uiHelper.GetVisibility(uiHelper.RollCall)) return;
        
        if (_managerCat.FoundCats.Count == 0 || rollCallCooldownTimer > 0)
        {
            rollCallImage.color = disabledColor;
            rollCallHotkeyText.color = disabledColor;
            return;
        }
        
        Color color = _managerCat.IsRollCalling ? activeColor : defaultColor;
        rollCallImage.color = color;
        rollCallHotkeyText.color = color;
    }

    [SerializeField] private AudioClip rollCallWhistleCallSFX;
    [SerializeField] private AudioClip rollCallWhistleDismissSFX;
    private void OnRollCall()
    {
        rollCallCooldownTimer = rollCallCooldown;
        _managerCat.OnRollCall(!_managerCat.IsRollCalling);
        ManagerSFX.Instance.PlaySFX(_managerCat.IsRollCalling ? rollCallWhistleCallSFX : rollCallWhistleDismissSFX, transform.position, 0.01f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, parent: transform);
    }
    
}
