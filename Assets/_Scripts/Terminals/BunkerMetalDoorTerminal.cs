using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
public class BunkerMetalDoorTerminal : Terminal
{
    [SerializeField] private Bunker bunker;
    
    [Header("Lights")]
    [SerializeField] private MeshRenderer redLightMesh;
    [SerializeField] private MeshRenderer greenLightMesh;
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject greenLight;
    [SerializeField] private Material redLightMaterial;
    [SerializeField] private Material greenLightMaterial;
    [SerializeField] private Material offLightMaterial;

    protected override void Awake()
    {
        base.Awake();
        SetLightOff();
    }
    
    public void Interact()
    {
        if (isUnlocked)
        {
            TerminalUnlockedInteraction();
        }
        else if (!isTransitioning)
        {
            if (IsOn)
            {
                TerminalLockedInteraction();
            }
            else
            {
                TerminalBootStart();                
            }
        }
    }

    [SerializeField] private TerminalMinigame sendElevatorMinigame;
    [SerializeField] private TerminalMinigame bunkerStandbyMinigame;
    [SerializeField] private TerminalMinigame divertPowerMinigame;
    [SerializeField] private TerminalMinigame operateDoorMinigame;
    [SerializeField] private TerminalMinigame downloadCatDataMinigame;
    public override void TerminalBootStart()
    {
        if (!IsProcessing) return;
        IsOn = true;
        GameObject temp = Instantiate(terminalMinigameBasePrefab, transform);
        TerminalMinigameBase = temp.GetComponent<TerminalMinigameBase>();
        TerminalMinigameBase.Initialize(this,  gameObject.GetComponent<ITerminal>(), terminalPosition, Quaternion.identity);
        
        
        List<TerminalMinigame> minigames = new List<TerminalMinigame>(terminalMinigames);

        if (!bunker.HasReachedQuota)
        {
            if (bunker.HasPower)
            {
                minigames.Add(divertPowerMinigame);
                minigames.Add(bunkerStandbyMinigame);
                minigames.Add(sendElevatorMinigame);
                minigames.Add(downloadCatDataMinigame);
                minigames.Add(operateDoorMinigame);
            }
            else
            {
                minigames.Add(operateDoorMinigame);
                minigames.Add(divertPowerMinigame);
            }
        }
        else
        {
            minigames.Add(sendElevatorMinigame);
        }
        TerminalMinigameBase.SetMinigames(minigames);
        OnTerminalBootStart?.Invoke();
    }
    
    [ContextMenu("Set Off")]
    private void SetLightOff()
    {
        redLightMesh.material = redLightMaterial;
        greenLightMesh.material = offLightMaterial;
        
        redLight.SetActive(true);
        greenLight.SetActive(false);
    }
    
    [ContextMenu("Set On")]
    private void SetLightOn()
    {
        redLightMesh.material = offLightMaterial;
        greenLightMesh.material = greenLightMaterial;
        
        redLight.SetActive(false);
        greenLight.SetActive(true);
    }
    
    public override void TerminalLock()
    {
        base.TerminalLock();
        SetLightOff();
    }
    
    public override void TerminalUnlock()
    {
        base.TerminalUnlock();
        SetLightOn();
    }
}
