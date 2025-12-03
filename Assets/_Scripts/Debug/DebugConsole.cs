using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DebugConsole : MonoBehaviour
{
    public string input;
    public List<string> inputHistory;
    public int inputHistoryIndex;
    public bool doShow;

    private List<DebugCommand> _commands;
    private PlayerInput _playerInput;
    
    private void OnEnable()
    {
        DebugToggle.togglePerformed += OnTogglePerformed;
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        DebugToggle.togglePerformed -= OnTogglePerformed;
        _playerInput.Disable();
    }

    private void OnTogglePerformed()
    {
        doShow = !doShow;

        if (doShow)
        {
            ManagerPlayer.instance.PlayerInputHelper.SetProcessing(false, ManagerPlayer.instance.PlayerInputHelper.All, "debug");
            
            _playerInput.Debug.Enter.performed += OnEnterPerformed; 
            _playerInput.Debug.CheckHistory.performed += OnCheckHistoryPerformed; 
            input = "";
            inputHistoryIndex = -1;
            
        }
        else
        {
            ManagerPlayer.instance.PlayerInputHelper.SetProcessing(true, ManagerPlayer.instance.PlayerInputHelper.All, "debug");
            
            _playerInput.Debug.Enter.performed -= OnEnterPerformed; 
            _playerInput.Debug.CheckHistory.performed -= OnCheckHistoryPerformed; 
        }
    }
    private void OnGUI()
    {
        if (!doShow) { return; }

        // Set the background color for the box (a visible color)
        GUI.backgroundColor = Color.white;

        // Create a box for the input field
        float y = 0f;
        //GUI.Box(new Rect(10f, y, Screen.width / 2f, 30f), ""); // Background for input field

        // Create the actual text field for input
        GUI.SetNextControlName("inputField");
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width / 2f, 20f), input);
        GUI.FocusControl("inputField");
    }
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
        
        _commands = new List<DebugCommand>();

        IDebugCommandSource[] idcs = GetComponentsInChildren<IDebugCommandSource>();
        foreach (IDebugCommandSource dc in idcs)
        {
            _commands.AddRange(dc.GetCommands());
        }
    }


    private void OnEnterPerformed(InputAction.CallbackContext value)
    {
        if (doShow)
        {
            HandleInput();
            input = "";
            inputHistoryIndex = -1;
        }
    }

    private void OnCheckHistoryPerformed(InputAction.CallbackContext value)
    {
        float state = value.ReadValue<Vector2>().y;

        if (inputHistory.Count <= 0) return;
        
        inputHistoryIndex += -((int) Mathf.Sign(state));
        inputHistoryIndex = Mathf.Clamp(inputHistoryIndex, 0, inputHistory.Count - 1);

        input = inputHistory[inputHistoryIndex];
    }
    
    private void HandleInput()
    {
        string[] splitInput = input.Trim().Split(' ');
        if (splitInput.Length == 0) return;

        string enteredCommand = splitInput[0];
        string[] args = splitInput.Skip(1).ToArray();

        foreach (var command in _commands)
        {
            if (enteredCommand.Equals(command.Id, StringComparison.OrdinalIgnoreCase))
            {
                command.Execute(args);
                inputHistory.Add(input);
                return;
            }
        }

        Debug.LogWarning("Unknown command: " + input);
    }

}