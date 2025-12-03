using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Terminal : MonoBehaviour
{
    [SerializeField] private UnityEvent onUnlocked;
    [SerializeField] private UnityEvent onLocked;
    
    [field: SerializeField] public bool IsProcessing { get; set; }
    [field: SerializeField] public bool IsOn { get; set; }
    [SerializeField] protected GameObject terminalMinigameBasePrefab;
    [field: SerializeField] public TerminalMinigameBase TerminalMinigameBase { get; protected set; }
    [SerializeField] protected List<TerminalMinigame> terminalMinigames;
    private ITerminalMinigame _terminalMinigame;
    [SerializeField] protected Vector3 terminalPosition;
    [SerializeField] protected GameObject focusTarget;
    [SerializeField] protected bool isUnlocked;
    [SerializeField] protected GameObject terminalViewCamera;
    [SerializeField] protected bool isTransitioning;
    [SerializeField] protected InteractableObject interactableObject;
    [field: SerializeField] public ITerminal ITerminal { get; private set; }

    public Action OnTerminalBootStart;
    public Action OnTerminalUnlocked;

    protected virtual void Awake()
    {
        IsProcessing = true;
        ITerminal = gameObject.GetComponent<ITerminal>();
    }
    
    public virtual void TerminalBootStart()
    {
        if (!IsProcessing) return;
        IsOn = true;
        GameObject temp = Instantiate(terminalMinigameBasePrefab, transform);
        TerminalMinigameBase = temp.GetComponent<TerminalMinigameBase>();
        TerminalMinigameBase.Initialize(this,  ITerminal, terminalPosition, Quaternion.identity);
        TerminalMinigameBase.SetMinigames(terminalMinigames);
        
        OnTerminalBootStart?.Invoke();
    }

    public void TerminalBootEnd()
    {
        _terminalMinigame = TerminalMinigameBase.CurrentMinigameObj.GetComponent<ITerminalMinigame>();
    }

    private IEnumerator AdjustPlayerToTerminal(float time)
    {
        ManagerPlayer mp = ManagerPlayer.instance;
        Rigidbody rb = mp.PlayerObj.GetComponent<Rigidbody>();

        while (TerminalMinigameBase != null && TerminalMinigameBase.isFocused)
        {
            Vector3 pos = mp.PlayerObj.transform.position;
            Quaternion rot = mp.PlayerHead.transform.localRotation;
            
            mp.PlayerObj.transform.position = Vector3.Lerp(pos, focusTarget.transform.position, Time.deltaTime / time); 
            rb.position = Vector3.Lerp(pos, focusTarget.transform.position, Time.deltaTime / time);
            mp.PlayerHead.transform.localRotation = Quaternion.Lerp(rot, Quaternion.LookRotation(TerminalMinigameBase.transform.position - mp.PlayerHead.transform.position), Time.deltaTime / time);
            yield return new WaitForFixedUpdate();
        }
    }
    
    private IEnumerator ProcessTransition(float time)
    {
        yield return new WaitForSeconds(time);
        isTransitioning = false;
    }
    
    public void TerminalLockedInteraction()
    {
        TerminalMinigameBase.ProcessFocus();
        StopAllCoroutines();
        
        if (TerminalMinigameBase.isFocused)
        {
            StartCoroutine(AdjustPlayerToTerminal(0.5f));
            isTransitioning = true;
            
            ManagerPlayer mp = ManagerPlayer.instance;
            mp.PlayerInputHelper.SetProcessing(false, mp.PlayerInputHelper.Terminal, "terminal");
            mp.PlayerUIHelper.SetVisibility(false, mp.PlayerUIHelper.Terminal, "terminal");
            
            terminalViewCamera.SetActive(true);
            interactableObject.IsOnInteractOutlineCulled = true;
        }
        else
        {
            isTransitioning = true;
            
            ManagerPlayer mp = ManagerPlayer.instance;
            mp.PlayerInputHelper.SetProcessing(true, mp.PlayerInputHelper.Terminal, "terminal");
            mp.PlayerUIHelper.SetVisibility(true, mp.PlayerUIHelper.Terminal, "terminal");
            
            terminalViewCamera.SetActive(false);
            interactableObject.IsOnInteractOutlineCulled = false;
        }
        
        StartCoroutine(ProcessTransition(0.5f));
    }

    protected virtual void TerminalUnlockedInteraction()
    {
    }
    
    public virtual void TerminalLock()
    {
        onLocked?.Invoke();
        isUnlocked = false;
        IsOn = false;
        interactableObject.IsOnInteractOutlineCulled = false;
        if (TerminalMinigameBase != null) Destroy(TerminalMinigameBase.gameObject);
    }
    
    public virtual void TerminalUnlock()
    {
        onUnlocked?.Invoke();
        OnTerminalUnlocked?.Invoke();
        TerminalMinigameBase.isFocused = false;
        
        ManagerPlayer mp = ManagerPlayer.instance;
        mp.PlayerInputHelper.SetProcessing(true, mp.PlayerInputHelper.Terminal, "terminal");
        mp.PlayerUIHelper.SetVisibility(true, mp.PlayerUIHelper.Terminal, "terminal");
        
        terminalViewCamera.SetActive(false);
        _terminalMinigame.OnMinigameUnfocus();
        isUnlocked = true;
        
        StopAllCoroutines();
        StartCoroutine(ProcessTransition(0.5f));
    }
}
