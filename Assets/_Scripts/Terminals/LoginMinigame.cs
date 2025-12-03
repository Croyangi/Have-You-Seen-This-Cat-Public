using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shapes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoginMinigame : MonoBehaviour, ITerminalMinigame
{
    private PlayerInput _playerInput = null;
    [SerializeField] private float loginTimer;
    [SerializeField] private float loginSet;
    [SerializeField] private bool isLoggingIn;
    [SerializeField] private Line progressBar;
    [SerializeField] private Color progressBarColor;
    [SerializeField] private TerminalMinigameBase terminalMinigameBase;
    [SerializeField] private AudioClip loginSFX;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        
        loginTimer = 0;
        progressBarColor = progressBar.Color;
    }

    private void FixedUpdate()
    {
        float t = Mathf.Clamp01(loginTimer / (loginSet - (loginSet / 4))); // normalize time (0 to 1)
        float eased = 1 - Mathf.Pow( 1 - t, 3); // ease-in curve
        float progress = Mathf.Lerp(0f, 0.28f, eased);
        progressBar.End = new Vector3(progress, 0f, 0f);

        if (loginTimer >= loginSet - 1)
        {
            float H, S, V;
            Color.RGBToHSV(progressBarColor, out H, out S, out V);
            float pulse = (Mathf.Sin(Time.time * 15f) * 0.5f) + 0.5f; // value from 0 to 1
            float animatedV = Mathf.Clamp01(V * (0.6f + 0.4f * pulse)); // modulate brightness between 70%–100%
            progressBar.Color = Color.HSVToRGB(H, S, animatedV);
        }
        else
        {
            progressBar.Color = progressBarColor;
        }
        
        if (isLoggingIn)
        {
            loginTimer = Mathf.Min(loginSet, loginTimer += Time.fixedDeltaTime);
            if (loginTimer >= loginSet)
            {
                terminalMinigameBase.OnMinigameEnd();
            }
        }
    }
    
    private IEnumerator PlayLoginSFX()
    {
        while (isLoggingIn)
        {
            ManagerSFX.Instance.PlaySFX(loginSFX, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.6f + loginTimer / 10f, isRandomPitch: false);
            yield return new WaitForSeconds(0.07f);
        }
    }
    
    private void OnLoginPerformed(InputAction.CallbackContext value)
    {
        isLoggingIn = true;
        StartCoroutine(PlayLoginSFX());
    }
    
    private void OnLoginCancelled(InputAction.CallbackContext value)
    {
        CancelLogin();
    }

    private void CancelLogin()
    {
        loginTimer = 0;
        isLoggingIn = false;
    }

    private void SubscribeToInput()
    {
        _playerInput.Enable();
        _playerInput.TerminalMinigames.Login.performed += OnLoginPerformed;
        _playerInput.TerminalMinigames.Login.canceled += OnLoginCancelled;
    }
    
    private void UnsubscribeToInput()
    {
        _playerInput.TerminalMinigames.Login.performed -= OnLoginPerformed;
        _playerInput.TerminalMinigames.Login.canceled -= OnLoginCancelled;
        _playerInput.Disable();
    }
    
    public void OnMinigameStart(TerminalMinigameBase minigameBase)
    {
        terminalMinigameBase = minigameBase;
    }
    
    public void OnMinigameEnd()
    {
        UnsubscribeToInput();
    }

    public void OnMinigameFocus()
    {
        SubscribeToInput();
    }
    
    public void OnMinigameUnfocus()
    {
        UnsubscribeToInput();
        CancelLogin();
    }

    private void OnDisable()
    {
        UnsubscribeToInput();
    }
    
    private void OnDestroy()
    {
        UnsubscribeToInput();
    }
}
