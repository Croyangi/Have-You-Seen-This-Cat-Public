using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInspectingHelper : MonoBehaviour, IProcessable
{
    [Header("References")] 
    private PlayerInput _playerInput;
    [SerializeField] private Transform head;
    [SerializeField] private PlayerInventoryHelper inventoryHelper;
    [SerializeField] private PlayerInteractHelper interactHelper;
    [SerializeField] private PlayerFlashlightHelper flashlightHelper;
    [SerializeField] private Vector3 inspectionInput;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private AudioClip[] inspectSounds;
    
    [Header("Settings")]
    [field: SerializeField] public bool CanFreeMoveInspect { get; set; }

    [field: SerializeField] public GameObject InspectedObject { get; private set; }
    [SerializeField] private InteractableObject interactableObject;
    [SerializeField] private Vector3 originalRotation;
    [SerializeField] private Vector3 originalPosition;
    
    [SerializeField] private float pickUpTime;
    [SerializeField] private Vector3 parallaxScale;
    [SerializeField] private float lerpScale;
    [SerializeField] private Vector3 parallaxRotationScale;
    [SerializeField] private Vector3 currentRotation;
    [SerializeField] private float slerpScale;
    [SerializeField] private InspectSettings inspectSettings;
    
    [SerializeField] private TextMeshProUGUI inspectTextMesh;
    [SerializeField] private PlayerUIHelper uiHelper;
    
    [field: SerializeField] public bool IsInspecting { get; private set; }
    [field: SerializeField] public bool IsProcessing { get; set;}

    private void Awake()
    {
        // Instantiate new Unity's Input System
        _playerInput = new PlayerInput();
    }

    private void Start()
    {
        GameReset();

        if (ManagerGame.Instance != null)
        {
            ManagerGame.OnGameStart += GameReset;
        }
    }

    private void GameReset()
    {
        uiHelper.SetVisibility(false, new List<IUI> { uiHelper.InspectFlavorText }, "inspect");
    }

    private void OnEnable()
    {
        //// Subscribes to Unity's input system
        _playerInput.Player.Inspection.performed += OnInspectionPerformed;
        _playerInput.Player.Inspection.canceled += OnInspectionCancelled;
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        //// Unubscribes to Unity's input system
        _playerInput.Player.Inspection.performed -= OnInspectionPerformed;
        _playerInput.Player.Inspection.canceled -= OnInspectionCancelled;
        _playerInput.Disable();
    }

    private void OnInspectionPerformed(InputAction.CallbackContext value)
    {
        inspectionInput = value.ReadValue<Vector3>();
    }

    private void OnInspectionCancelled(InputAction.CallbackContext value)
    {
        // This never really gets called, unless external factors, like tabbing out, good for releasing sticky inputs
        inspectionInput = Vector3.zero; // Resets input
    }

    public void InspectingUpdate()
    {
        if (InspectedObject == null) return;
        InspectFollow(InspectedObject);
    }

    public void SetObject(GameObject obj, InteractableObject interactObj)
    {
        InspectedObject = obj;
        interactableObject = interactObj;
    }
    
    
    private void OnInteractPerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return;
        if (!IsInspecting) return;

        if (!CanFreeMoveInspect || interactHelper.GetInteractRaycastObj(interactHelper.raycastDistance, InspectedObject) == null)
        {
            OnEndInspection();
        }
    }
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private int _initLayerMask;
    [SerializeField] private LayerMask renderOnlyLayerMask;
    public void OnStartInspection()
    {
        _playerInput.Player.Interact.performed += OnInteractPerformed;
        
        if (InspectedObject == null) return;
        if (!InspectedObject.TryGetComponent<IInspectableObject>(out var iInspectObj)) return;

        IsInspecting = true;
        interactableObject.IsBeingInteracted = true;
        interactableObject.IsOnInteractOutlineCulled = true;
        
        ManagerSFX.Instance.PlayRawSFX(inspectSounds[0], 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        
        if (interactableObject.InspectText != "")
        {
            uiHelper.SetVisibility(true, new List<IUI> { uiHelper.InspectFlavorText}, "inspect");
            inspectTextMesh.text = interactableObject.InspectText;
        }

        _initLayerMask = InspectedObject.layer;
        if (CanFreeMoveInspect) SetLayerRecursively(InspectedObject, (int) Mathf.Log(renderOnlyLayerMask.value, 2));
        
        // Reset
        currentRotation = Vector3.zero;
        if (InspectedObject.TryGetComponent<InspectSettingsHolder>(out var holder))
        {
            inspectSettings = holder.InspectSettings;
        }
        else
        {
            inspectSettings = null;
        }

        originalPosition = InspectedObject.transform.position;
        originalRotation = InspectedObject.transform.eulerAngles;
        iInspectObj.OnStartInspect();
        
        flashlightHelper.SetForceNear(true);
    }

    public void OnEndInspection()
    {
        _playerInput.Player.Interact.performed -= OnInteractPerformed;
        
        if (InspectedObject != null && InspectedObject.TryGetComponent<IInspectableObject>(out var inspectObj))
        {
            ManagerSFX.Instance.PlayRawSFX(inspectSounds[1], 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
            
            uiHelper.SetVisibility(false, new List<IUI> { uiHelper.InspectFlavorText}, "inspect");
            
            SetLayerRecursively(InspectedObject, _initLayerMask);
            
            if (CanFreeMoveInspect)
            {
                StartCoroutine(PutDownModel(pickUpTime, inspectObj, originalRotation));
            }
            else
            {
                StartCoroutine(PutBackModel(pickUpTime, originalPosition, originalRotation, inspectObj));
            }
        }
        
        interactableObject.IsBeingInteracted = false;
        interactableObject.IsOnInteractOutlineCulled = false;
        IsInspecting = false;
        inventoryHelper.isForceHold = false;
        inventoryHelper.heldItem = null;
        InspectedObject = null;
        CanFreeMoveInspect = false;

        flashlightHelper.SetForceNear(false);
    }

    private IEnumerator PutBackModel(float time, Vector3 originalPos, Vector3 originalRot,
        IInspectableObject iInspectObj)
    {
        InspectedObject.transform.DORotate(originalRot, 0.2f).SetEase(Ease.OutQuad);
        InspectedObject.transform.DOMove(originalPos, time).SetEase(Ease.OutQuint);

        yield return new WaitForSeconds(time);
        iInspectObj.OnEndInspect();
    }

    // Called for any CanFreeMoveInspect models
    private IEnumerator PutDownModel(float time, IInspectableObject iInspectObj, Vector3 originalRot)
    {
        // Set raycast height level of head
        Vector3 origin = InspectedObject.transform.position;
        Vector3 direction = Vector3.down;

        // Set up
        int range = 10;
        Ray ray = new Ray(origin, direction);
        Vector3 returnPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, range, LayerUtility.Environment))
        {
            returnPoint = hit.point;
            Debug.DrawRay(origin, direction * range, Color.cyan, 5f); // MAGENTA, OPEN
        }
        else
        {
            Debug.LogWarning("No available ground found");
            returnPoint = origin;
        }

        InspectedObject.transform.DORotate(originalRot, 0.2f).SetEase(Ease.OutQuad);
        InspectedObject.transform.DOMove(returnPoint, time).SetEase(Ease.OutQuint);

        yield return new WaitForSeconds(time);
        iInspectObj.OnEndInspect();
    }

    /// <summary>
    /// Controls inspecting free-rotation
    /// </summary>
    private void InspectFollow(GameObject obj)
    {
        // Settings
        Vector3 posOffset = Vector3.zero;
        Vector3 rotOffset = Vector3.zero;
        float zoomMultiplier = 1f;
        int invertedControls = 1;
        if (inspectSettings != null)
        {
            posOffset = inspectSettings.posOffset;
            rotOffset = inspectSettings.rotationOffset;
            zoomMultiplier = inspectSettings.zoomMultiplier;
            if (inspectSettings.invertedControls)
            {
                invertedControls = -1;
            }
        }

        // Position
        Vector3 lerpPos = Camera.main.transform.position + Camera.main.transform.forward * zoomMultiplier;
        float x = Mathf.Lerp(obj.transform.position.x, lerpPos.x + posOffset.x, Time.deltaTime * lerpScale);
        float y = Mathf.Lerp(obj.transform.position.y, lerpPos.y + posOffset.y, Time.deltaTime * lerpScale);
        float z = Mathf.Lerp(obj.transform.position.z, lerpPos.z + posOffset.z, Time.deltaTime * lerpScale);
        obj.transform.position = new Vector3(x, y, z);

        // Rotation
        currentRotation.x += inspectionInput.z * invertedControls * Time.deltaTime * rotationSpeed; // Up/Down
        currentRotation.y += inspectionInput.x * invertedControls * -1 * Time.deltaTime * rotationSpeed; // Left/Right
        currentRotation.x %= 360f;
        currentRotation.y %= 360f;

        // Create the X and Y rotation quaternions
        Quaternion rotationX =
            Quaternion.AngleAxis(currentRotation.x * parallaxRotationScale.x, Camera.main.transform.right);
        Quaternion rotationY = Quaternion.AngleAxis(currentRotation.y * parallaxRotationScale.y, Vector3.up);

        // Combine rotations in correct order (Z -> X -> Y) // OR NOT?
        Quaternion combinedRotation = rotationX * rotationY;
        
        Quaternion finalRotation = Quaternion.Slerp(
            obj.transform.rotation,
            combinedRotation,
            slerpScale
        );

        obj.transform.rotation = finalRotation;
    }
}