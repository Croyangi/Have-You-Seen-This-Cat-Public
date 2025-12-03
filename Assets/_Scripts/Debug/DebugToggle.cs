using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebugToggle : MonoBehaviour
{
    private PlayerInput _playerInput;
    [SerializeField] private int buttonPressCount;
    [SerializeField] private bool isToggled;
    public static event Action togglePerformed;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }
    
    private void OnEnable()
    {
        //// Subscribes to Unity's input system
        _playerInput.Debug.ToggleD.performed += OnTogglePerformed;
        _playerInput.Debug.ToggleD.canceled += OnToggleCancelled;
        _playerInput.Debug.ToggleE.performed += OnTogglePerformed;
        _playerInput.Debug.ToggleE.canceled += OnToggleCancelled;
        _playerInput.Debug.ToggleV.performed += OnTogglePerformed;
        _playerInput.Debug.ToggleV.canceled += OnToggleCancelled;

        _playerInput.Enable();
    }

    private void OnDisable()
    {
        //// Unubscribes to Unity's input system
        _playerInput.Debug.ToggleD.performed -= OnTogglePerformed;
        _playerInput.Debug.ToggleD.canceled -= OnToggleCancelled;
        _playerInput.Debug.ToggleE.performed -= OnTogglePerformed;
        _playerInput.Debug.ToggleE.canceled -= OnToggleCancelled;
        _playerInput.Debug.ToggleV.performed -= OnTogglePerformed;
        _playerInput.Debug.ToggleV.canceled -= OnToggleCancelled;

        _playerInput.Disable();
    }

    private void OnTogglePerformed(InputAction.CallbackContext value)
    {
        buttonPressCount++;

        if (buttonPressCount >= 3)
        {
            togglePerformed?.Invoke();
            isToggled = !isToggled;
        }
    }

    private void OnToggleCancelled(InputAction.CallbackContext value)
    {
        buttonPressCount--;
    }
}
