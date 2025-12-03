// using System;
// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.SceneManagement;
//
// [SelectionBase]
// public class MetalDoorTerminal : Terminal, ITerminal
// {
//     [SerializeField] private GameObject unlockedObjs;
//     
//     [Header("Lights")]
//     [SerializeField] private MeshRenderer redLightMesh;
//     [SerializeField] private MeshRenderer greenLightMesh;
//     [SerializeField] private GameObject redLight;
//     [SerializeField] private GameObject greenLight;
//     [SerializeField] private Material redLightMaterial;
//     [SerializeField] private Material greenLightMaterial;
//     [SerializeField] private Material offLightMaterial;
//
//     [SerializeField] private MetalDoor metalDoor;
//     
//     [SerializeField] private InteractableGeometry interactableGeometryScreen;
//
//     [field: SerializeField] public bool IsProcessing { get; set; } = true;
//
//     private void Awake()
//     {
//         SetLightOff();
//         unlockedObjs.SetActive(false);
//     }
//     
//     public void OnInteract()
//     {
//         if (!IsProcessing) return;
//         
//         if (isUnlocked)
//         {
//             OnTerminalUnlockedInteraction();
//         }
//         else if (!isTransitioning)
//         {
//             if (IsOn)
//             {
//                 OnTerminalLockedInteraction();
//             }
//             else
//             {
//                 OnTerminalBootStart();                
//             }
//         }
//     }
//
//     public void OnTerminalLockedInteraction()
//     {
//         terminalMinigameBase.ProcessFocus();
//         StopAllCoroutines();
//         
//         if (terminalMinigameBase.isFocused)
//         {
//             interactableGeometryScreen.OnHoverExit();
//             
//             StartCoroutine(AdjustPlayerToTerminal(0.5f));
//             isTransitioning = true;
//             ManagerPlayer.instance.PlayerInputHelper.SetProcessing(false, ManagerPlayer.instance.PlayerInputHelper.Terminal, "terminal");
//             terminalViewCamera.SetActive(true);
//         }
//         else
//         {
//             interactableGeometryScreen.OnHoverEnter();
//             
//             isTransitioning = true;
//             ManagerPlayer.instance.PlayerInputHelper.SetProcessing(true, ManagerPlayer.instance.PlayerInputHelper.Terminal, "terminal");
//             terminalViewCamera.SetActive(false);
//         }
//         
//         StartCoroutine(ProcessTransition(0.5f));
//     }
//
//     public void OnTerminalUnlockedInteraction()
//     {
//
//     }
//     
//     [ContextMenu("Set Off")]
//     private void SetLightOff()
//     {
//         redLightMesh.material = redLightMaterial;
//         greenLightMesh.material = offLightMaterial;
//         
//         redLight.SetActive(true);
//         greenLight.SetActive(false);
//     }
//     
//     [ContextMenu("Set On")]
//     private void SetLightOn()
//     {
//         redLightMesh.material = offLightMaterial;
//         greenLightMesh.material = greenLightMaterial;
//         
//         redLight.SetActive(false);
//         greenLight.SetActive(true);
//     }
//
//     public void OnTerminalUnlock()
//     {
//         if (metalDoor.IsOpen)
//         {
//             metalDoor.OnClose(this);
//         }
//         else
//         {
//             metalDoor.OnOpen(this);
//         }
//
//         if (terminalMinigameBase != null) terminalMinigameBase.isFocused = false;
//         
//         ManagerPlayer.instance.PlayerInputHelper.SetProcessing(true, ManagerPlayer.instance.PlayerInputHelper.Terminal, "terminal");
//         terminalViewCamera.SetActive(false);
//         
//         if (TerminalMinigame != null) TerminalMinigame.OnMinigameUnfocus();
//         isUnlocked = true;
//         SetLightOn();
//         
//         StopAllCoroutines();
//         StartCoroutine(ProcessTransition(0.5f));
//         
//         unlockedObjs.SetActive(true);
//     }
//
//     [ContextMenu("Lock")]
//     public void OnTerminalLock()
//     {
//         isUnlocked = false;
//         IsOn = false;
//         SetLightOff();
//         
//         unlockedObjs.SetActive(false);
//     }
// }
