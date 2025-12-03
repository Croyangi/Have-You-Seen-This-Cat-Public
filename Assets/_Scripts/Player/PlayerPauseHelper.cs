using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerPauseHelper : MonoBehaviour, IProcessable
{
    private PlayerInput _playerInput;
    
    [SerializeField] private PlayerUIHelper uiHelper;
    [field: SerializeField] public bool IsProcessing { get; set;}
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
    }
    
    private void OnEnable()
    {
        _playerInput.Enable();
        SubscribeToInput();
    }

    private void OnDisable()
    {
        UnsubscribeToInput();
        _playerInput.Disable();
    }

    private void SubscribeToInput()
    {
        _playerInput.Player.Pause.performed += OnPausePerformed;
    }

    private void UnsubscribeToInput()
    {
        _playerInput.Player.Pause.performed -= OnPausePerformed;
    }
    private void OnPausePerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return;
        
        ManagerPlayer.instance.PauseMenu.OnPausePerformed();
    }
}
