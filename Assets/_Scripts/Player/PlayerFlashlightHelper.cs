using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFlashlightHelper : MonoBehaviour, IProcessable
{
    [Header("References")]
    private PlayerInput _playerInput;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private GameObject lightObj;
    [SerializeField] private AudioClip[] flashlightSFX;

    [field: SerializeField] public bool IsProcessing { get; set;}
    [field: SerializeField] public bool HasObtained { get; set; }
    [field: SerializeField] public bool IsFlashlightOn { get; private set; }
    [SerializeField] private float flashlightRaycastDistance;
    [SerializeField] private float headRaycastDistance;
    [SerializeField] private Vector3 nearestPos;
    [SerializeField] private Vector3 furthestPos;
    
    [SerializeField] private float lerpScale;
    [SerializeField] private float slerpScale;
    [SerializeField] private GameObject playerHead;
    [SerializeField] private Vector3 targetPosition;
    
    [SerializeField] private Vector3 swayMultiplier;
    [SerializeField] private Vector3 swaySpeed;
    
    [SerializeField] private bool isForceNear;

    private void Awake()
    {
        // Instantiate new Unity's Input System
        _playerInput = new PlayerInput();
        ObtainFlashlight();
    }
    
    private void Start()
    {
        flashlight.SetActive(HasObtained);
        IsFlashlightOn = false;
        SetFlashlight(IsFlashlightOn);
    }

    private void OnEnable()
    {
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        UnsubscribeToInput();
        _playerInput.Disable();
    }

    private void SubscribeToInput()
    {
        _playerInput.Player.Flashlight.performed += OnFlashlightPerformed;
    }

    private void UnsubscribeToInput()
    {
        _playerInput.Player.Flashlight.performed -= OnFlashlightPerformed;
    }

    private void OnFlashlightPerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return;
        
        IsFlashlightOn = !IsFlashlightOn;
        SetFlashlight(IsFlashlightOn);
        AudioClip sfx = IsFlashlightOn ? flashlightSFX[0] : flashlightSFX[1];
        
        ManagerSFX.Instance.PlaySFX(sfx, transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, parent: transform);
    }

    [ContextMenu("Obtain Flashlight")]
    public void ObtainFlashlight()
    {
        HasObtained = true;
        ProcessObtainCheck();
    }

    [ContextMenu("Process Obtain Check")]
    public void ProcessObtainCheck()
    {
        flashlight.SetActive(HasObtained);
        if (HasObtained)
        {
            SetForceNear(false);
            SubscribeToInput();
        }
        else
        {
            SetForceNear(true);
            UnsubscribeToInput();
        }
    }

    private int _forceNearStateStack;
    public void SetForceNear(bool state)
    {
        int change = state ? 1 : -1;
        _forceNearStateStack = Mathf.Max(0, _forceNearStateStack + change);
        isForceNear = _forceNearStateStack > 0;
    }

    public void SetFlashlight(bool state)
    {
        lightObj.SetActive(state);
    }

    private void FixedUpdate()
    {
        ProcessFlashlightWithdraw();
    }
    
    private void Update()
    {
        FlashlightFollow();
    }

    [SerializeField] private float flashlightCheckRadius = 0.05f;

    private float GetRaycastDistance(Vector3 pos, Vector3 dir, float dist)
    {
        float radius = flashlightCheckRadius;

        Ray ray = new Ray(pos, dir);

        if (Physics.SphereCast(ray, radius, out RaycastHit hit, dist, LayerUtility.Environment))
        {
            Debug.DrawRay(pos, dir * hit.distance, Color.red);
            return hit.distance;
        }

        return dist;
    }


    [SerializeField] private Transform flashlightBack;
    private void ProcessFlashlightWithdraw()
    {
        float a = GetRaycastDistance(flashlightBack.position, flashlightBack.forward, flashlightRaycastDistance);
        float b = GetRaycastDistance(playerHead.transform.position, playerHead.transform.forward, headRaycastDistance);
        a /= flashlightRaycastDistance;
        b /= headRaycastDistance;
        
        float t = a < b ? a : b;
        
        Vector3 targetPos = isForceNear ? nearestPos : Vector3.Lerp(nearestPos, furthestPos, t);
        targetPosition = targetPos;
    }
    
    /// <summary>
    /// Controls inspecting free-rotation
    /// </summary>
    private void FlashlightFollow()
    {
        // Position offset in camera's local space
        Vector3 localOffset = targetPosition; // e.g., new Vector3(0, -0.5f, 1f);
        Vector3 targetPos = playerHead.transform.TransformPoint(localOffset);
        
        Vector3 offset = new Vector3(
            Mathf.Sin(Time.time * swaySpeed.x) * swayMultiplier.x,
            Mathf.Cos(Time.time * swaySpeed.y) * swayMultiplier.y,
            0
        );
        
        flashlight.transform.position = Vector3.Lerp(flashlight.transform.position, targetPos, Time.deltaTime * lerpScale) + offset;
        
        // Smooth rotation follow
        Quaternion targetRot = playerHead.transform.rotation;
        flashlight.transform.rotation = Quaternion.Slerp(flashlight.transform.rotation, targetRot, Time.deltaTime * slerpScale);
    }
}
