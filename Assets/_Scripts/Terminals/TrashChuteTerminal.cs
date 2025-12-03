using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[SelectionBase]
public class TrashChuteTerminal : Terminal
{
    [SerializeField] private bool isOpen;
    [SerializeField] private GameObject trashChute;
    
    [SerializeField] private AudioClip hatchUnlock;
    [SerializeField] private AudioClip hatchOpen;
    [SerializeField] private AudioClip hatchClose;
    [SerializeField] private AudioClip hatchDeposit;
    [SerializeField] private Transform audioSpot;

    [SerializeField] private InteractableGeometry hatchInteractionGeometry;
    
    [Header("Lights")]
    [SerializeField] private MeshRenderer redLightMesh;
    [SerializeField] private MeshRenderer greenLightMesh;
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject greenLight;
    [SerializeField] private Material redLightMaterial;
    [SerializeField] private Material greenLightMaterial;
    [SerializeField] private Material offLightMaterial;

    private void Start()
    {
        SetHoverText();
    }
    
    public void InteractTerminal()
    {
        if (isTransitioning) return;
        if (isOpen) return;
        
        if (IsOn)
        {
            TerminalLockedInteraction();
        }
        else
        { 
            TerminalBootStart();                
        }
    }
    
    public void InteractTrashChute()
    {
        if (isOpen)
        {
            TerminalUnlockedInteraction();
        }
    }

    protected override void TerminalUnlockedInteraction()
    {
        base.TerminalUnlockedInteraction();
        
        PlayerInventoryHelper inventory = ManagerPlayer.instance.PlayerInventoryHelper;
        
        if (inventory.heldItem != null)
        {
            inventory.RemoveHeldItem();
            TerminalLock();
            
            ManagerSFX.Instance.PlaySFX(hatchDeposit, audioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        } else if (inventory.CurrentItem != null)
        {
            inventory.RemoveItem(inventory.CurrentItem);
            TerminalLock();
            
            ManagerSFX.Instance.PlaySFX(hatchDeposit, audioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        }
    }
    

    [ContextMenu("Open Chute")]
    private IEnumerator OpenChute()
    {
        ManagerSFX.Instance.PlaySFX(hatchUnlock, audioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        isOpen = true;
        SetLightOn();
        trashChute.transform.DOComplete();
        trashChute.transform.DOLocalRotate(new Vector3(-125f, 0f, 0f), 1f).SetEase(Ease.OutBounce).SetDelay(0.5f);
        
        yield return new WaitForSeconds(0.5f);
        
        ManagerSFX.Instance.PlaySFX(hatchOpen, audioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }
    
    [ContextMenu("Close Chute")]
    private IEnumerator CloseChute()
    {
        isOpen = false;
        SetLightOff();
        trashChute.transform.DOComplete();
        trashChute.transform.DOLocalRotate(new Vector3(-130f, 0f, 0f), 0.2f).SetEase(Ease.OutCubic);
        trashChute.transform.DOLocalRotate(new Vector3(-90f, 0f, 0f), 0.5f).SetEase(Ease.OutBounce).SetDelay(0.2f);
        
        ManagerSFX.Instance.PlaySFX(hatchClose, audioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);

        yield return null;
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
        
        SetHoverText();
        SetLightOff();
        StartCoroutine(CloseChute());
    }
    
    public override void TerminalUnlock()
    {
        base.TerminalUnlock();
        
        SetHoverText();
        SetLightOn();
        StartCoroutine(OpenChute());
    }

    private void SetHoverText()
    {
        if (isUnlocked)
        {
            hatchInteractionGeometry.HoverText = "trash chute";
        }
        else
        {
            hatchInteractionGeometry.HoverText = "[locked]";
        }
    }
}
