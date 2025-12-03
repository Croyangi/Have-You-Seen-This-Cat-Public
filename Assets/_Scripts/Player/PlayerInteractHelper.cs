using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteractHelper : MonoBehaviour, IProcessable
{
    [Header("References")]
    private PlayerInput _playerInput;

    [SerializeField] private GameObject playerHead;

    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private PlayerInspectingHelper inspectingHelper;
    [SerializeField] private PlayerMovementHelper movementHelper;
    [SerializeField] private PlayerInventoryHelper inventoryHelper;
    [SerializeField] private PlayerCrosshairHelper crosshairHelper;

    [SerializeField] private PlayerUIHelper uiHelper;
    [SerializeField] private TextMeshProUGUI hoverTextMesh;
    
    [Header("Settings")]
    public float raycastDistance;
    [SerializeField] private LayerMask blocksRaycastLayer;

    [Header("Getters")]

    [field: SerializeField] public bool IsProcessing { get; set;}


    private void Awake()
    {
        _playerInput = new PlayerInput();
        uiHelper.SetVisibility(false, new List<IUI> { uiHelper.HoverFlavorText}, "interact");
    }

    private void OnEnable()
    {
        _playerInput.Player.Interact.performed += OnInteractPerformed;
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.Interact.performed -= OnInteractPerformed;
        _playerInput.Disable();
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return;
        
        ProcessInteraction(); 
    }

    private void FixedUpdate()
    {
        InteractionCheckUpdate();
    }

    private GameObject _prevRaycastObj;
    private bool _prevOutlineCulled;

    private void InteractionCheckUpdate()
    {
        if (!IsProcessing)
        {
            crosshairHelper.PlayAnimation(crosshairHelper.Cross, crosshairHelper.BaseLayer);
            uiHelper.SetVisibility(false, new List<IUI> { uiHelper.HoverFlavorText }, "interact");
            return;
        }

        // ALL Inspectables are Interactable, but NOT ALL Interactables are Inspectables
        GameObject raycastObj = GetInteractRaycastObj(raycastDistance, inspectingHelper.InspectedObject);
        InteractableObject interactableObj = null;
        if (raycastObj != null && raycastObj.TryGetComponent<InteractableObject>(out var interactObj))
        {
            interactableObj = interactObj;
            hoverTextMesh.text = interactableObj.HoverText;
        }

        InteractableObject prevInteractObj = null;
        if (_prevRaycastObj != null && _prevRaycastObj.TryGetComponent<InteractableObject>(out var obj)) prevInteractObj = obj;

        if (inspectingHelper.IsInspecting)
        {
            crosshairHelper.PlayAnimation(crosshairHelper.Grab, crosshairHelper.BaseLayer);
            if (inspectingHelper.InspectedObject != null && inspectingHelper.InspectedObject.TryGetComponent<InteractableObject>(out var iObj))
            {
                iObj.HideOutline();
            }
            return;
        }

        if (raycastObj == null)
        {
            if (prevInteractObj != null)
            {
                prevInteractObj.HideOutline();
                _prevRaycastObj = null;
            }
            
            uiHelper.SetVisibility(false, new List<IUI> { uiHelper.HoverFlavorText }, "interact");
            crosshairHelper.PlayAnimation(crosshairHelper.Default, crosshairHelper.BaseLayer);
            return;
        }

        // || prevInteractObj.IsBeingInteracted)
        if (prevInteractObj != null && _prevRaycastObj != raycastObj)
        {
            prevInteractObj.HideOutline();
            _prevRaycastObj = null;
            uiHelper.SetVisibility(false, new List<IUI> { uiHelper.HoverFlavorText }, "interact");
        }

        if (interactableObj == null || !interactableObj.IsInteractable) return;

        if (_prevRaycastObj != raycastObj || _prevOutlineCulled != interactableObj.IsOnInteractOutlineCulled)
        {
            _prevRaycastObj = raycastObj;
            _prevOutlineCulled = interactableObj.IsOnInteractOutlineCulled;

            if (interactableObj.IsOnInteractOutlineCulled)
            {
                interactableObj.HideOutline();
                uiHelper.SetVisibility(false, new List<IUI> { uiHelper.HoverFlavorText }, "interact");
            }
            else
            {
                interactableObj.ShowOutline();
                if (interactableObj.HoverText != "")
                {
                    uiHelper.SetVisibility(true, new List<IUI> { uiHelper.HoverFlavorText }, "interact");
                }
            }
        }

        crosshairHelper.PlayAnimation(
            interactableObj.IsInspectable ? crosshairHelper.Inspect : crosshairHelper.Interact,
            crosshairHelper.BaseLayer);
    }

    private void ProcessInteraction()
    {
        // ALL Inspectables are Interactable, but NOT ALL Interactables are Inspectables
        GameObject raycastObj = GetInteractRaycastObj(raycastDistance, inspectingHelper.InspectedObject);
        if (raycastObj == null) return;
        
        // Call interface method to interact
        if (raycastObj.TryGetComponent<InteractableObject>(out var interactObj) && interactObj.IsInteractable && !interactObj.IsBeingInteracted)
        {
            if (!inspectingHelper.IsInspecting)
            {
                OnInspection(raycastObj, interactObj);
            }
            interactObj.OnInteract();
        }
    }

    private void OnInspection(GameObject raycastObj, InteractableObject interactObj)
    {
        if (!interactObj.IsInspectable) return;
        if (!inventoryHelper.IsAvailableSpace()) return;

        inventoryHelper.isForceHold = true;
        inventoryHelper.heldItem = raycastObj;
        
        // FIRST, set inspectedObj for inspecting helper
        inspectingHelper.SetObject(raycastObj, interactObj);

        // Whether you can move during inspection
        inspectingHelper.CanFreeMoveInspect = interactObj.IsFreeMoveInspectable;
        
        // Force state into inspecting state
        stateMachine.RequestStateChange(stateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Inspecting]);
    }
    
    private bool OnInteractRaycast(float distance, GameObject inspectingObject)
    {
        Ray ray = new Ray(playerHead.transform.position, playerHead.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.Equals(inspectingObject)) continue;
            
            if (hit.collider.gameObject.TryGetComponent<InteractableObject>(out var interactableObj))
            {
                return true;
            }
        }

        return false;
    }

    public GameObject GetInteractRaycastObj(float distance, GameObject inspectingObject)
    {
        Ray ray = new Ray(playerHead.transform.position, playerHead.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance);

        hits = hits.OrderBy(hit => hit.distance).ToArray();
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == inspectingObject) continue;
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("InteractBlocker")) return null;
            
            if (hit.collider.gameObject.TryGetComponent<InteractableObject>(out var interactableObj)) return hit.collider.gameObject;
        }

        return null;
    }
}
