using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[SelectionBase]
public class GenericTerminal : Terminal
{
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
