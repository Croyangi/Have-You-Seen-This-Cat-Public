using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shapes;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrashChuteMinigame : MonoBehaviour, ITerminalMinigame
{
    private PlayerInput _playerInput = null;
    [SerializeField] private TerminalMinigameBase terminalMinigameBase;

    [Header("Minigame")] 
    [SerializeField] private float unlockAnimationDuration;
    [SerializeField] private float transitionDuration;
    
    [SerializeField] private int index;
    [SerializeField] private float[] positions = {-0.12f, 0f, 0.12f};
    
    [SerializeField] private GameObject[] insides;
    [SerializeField] private Rectangle[] backings;
    [SerializeField] private bool[] unlockedLatches = new bool[3];
    [SerializeField] private Color unlockedColor;    
    [SerializeField] private GameObject activeArrow;
    [SerializeField] private int totalUnlocked;
    
    [SerializeField] private GameObject modemBeepSFXObject;
    [SerializeField] private AudioClip modemBeep;
    [SerializeField] private AudioClip click;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }
    
    private void OnActionPerformed(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();
        
        // Pressing right
        if (input.x > 0 && input.y == 0)
        {
            index++;
            index %= positions.Length;

            PlayHorizontalInputSFX();
            
        } else if (input.x < 0 && input.y == 0)
        {
            index--;
            if (index < 0) { index = positions.Length - 1; }

            PlayHorizontalInputSFX();
        } else if (input.y > 0 && !unlockedLatches[index])
        {
            StartCoroutine(UnlatchLock(index));
            unlockedLatches[index] = true;
            
            ManagerSFX.Instance.PlaySFX(click, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        }

        activeArrow.transform.localPosition = new Vector3(positions[index], activeArrow.transform.localPosition.y, activeArrow.transform.localPosition.z);
    }

    private void PlayHorizontalInputSFX()
    {
        ManagerSFX.Instance.PlaySFX(click, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }

    private IEnumerator UnlatchLock(int i)
    {
        float maxPosition = 0.025f;
        GameObject inside = insides[i];
        Rectangle backing = backings[i];
        
        inside.transform.DOLocalMoveY(maxPosition, unlockAnimationDuration).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(unlockAnimationDuration);

        backing.Color = unlockedColor;
        totalUnlocked++;
        if (totalUnlocked >= positions.Length)
        {
            ManagerSFX.Instance.PlaySFX(modemBeep, transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            StartCoroutine(TransitionDelay());
        }
    }

    private IEnumerator TransitionDelay()
    {
        yield return new WaitForSeconds(transitionDuration);
        terminalMinigameBase.OnMinigameEnd();
    }

    private void SubscribeToInput()
    {
        _playerInput.Enable();
        _playerInput.TerminalMinigames.UnlatchLocks.performed += OnActionPerformed;
    }
    
    private void UnsubscribeToInput()
    {
        _playerInput.TerminalMinigames.UnlatchLocks.performed -= OnActionPerformed;
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
