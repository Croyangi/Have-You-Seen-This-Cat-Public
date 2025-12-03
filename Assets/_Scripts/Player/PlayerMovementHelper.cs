using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Cinemachine.Utility;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovementHelper : MonoBehaviour, IProcessable
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject[] hitboxes;
    [SerializeField] private GameObject hitboxTop;
    
    [SerializeField] private PlayerInputHelper inputHelper;
    [SerializeField] private RectTransform staminaBarRectTransform; 
    [SerializeField] private Image staminaBarImage;
    [field: SerializeField] public bool IsExhausted { get; private set; }

    [Header("Settings")]
    [field: SerializeField] public bool IsCrouching { get; private set; }
    [field: SerializeField] public bool IsStanding { get; private set; }
    [SerializeField] private bool isMidCrouch;
    [SerializeField] private Vector3 hitboxMidStandCenter;
    [SerializeField] private Vector3 hitboxTopStandCenter;
    [SerializeField] private Vector3 hitboxCrouchHeadPosition;
    [SerializeField] private Vector3 hitboxStandHeadPosition;
    [SerializeField] private float hitboxStandRaycastDistance;
    [field: SerializeField] public float MinimumRunningStamina { get; private set; }
    [SerializeField] private float staminaBarWidth;

    [SerializeField] private Color staminaBarExhaustedColor;
    [SerializeField] private float staminaBarPulseFrequency;
    [SerializeField] private float staminaBarExhaustedMultiplier;
    [SerializeField] private float staminaBarFadeMultiplier;
    [Range(0, 1)] [SerializeField] private float staminaBarRestingAlpha;
    [Range(0, 1)] [SerializeField] private float staminaBarActiveAlpha;
    
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float velocityPower;
    [field: SerializeField] public float MovementSpeed { get;  set; }

    [field: SerializeField] public float Stamina { get; set; }
    [field: SerializeField] public float StaminaMax { get; private set; }
    [field: SerializeField] public float StaminaGain { get; set; }

    [SerializeField] private bool canMove;
    
    [SerializeField] private float swayIdleAmplitude;
    [SerializeField] private float swayIdleSpeed;
    
    [SerializeField] private Vector3 swayAmplitude;
    [SerializeField] private Vector3 swayAmplitudeVelocityScale;
    [SerializeField] private Vector3 swaySpeed;
    [SerializeField] private Vector3 swaySpeedVelocityScale;
    [SerializeField] private float swaySmoothTime;

    [field: SerializeField] public bool IsProcessing { get; set;}
    private PlayerInput _playerInput;
    
    [field: SerializeField] public bool IsRunningPressed { get; private set; }
    [field: SerializeField] public bool IsRunning { get; private set; }
    [SerializeField] private Vector3 rawInputMovement;
    [field: SerializeField] public Vector3 ProcessedInputMovement { get; private set; }
    [field: SerializeField] public bool IsCrouchPressed { get; private set; }
    
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private GameObject feet;

    [SerializeField] private float footstepDelayTimer;
    [SerializeField] private float footstepDelayTimerSet;

    private void Awake()
    {
        // Instantiate new Unity's Input System
        _playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        //// Subscribes to Unity's input system
        _playerInput.Player.Movement.performed += OnMovementPerformed;
        _playerInput.Player.Movement.canceled += OnMovementCancelled;

        _playerInput.Player.Run.performed += OnRunPerformed;
        _playerInput.Player.Run.canceled += OnRunCancelled;
        
        _playerInput.Player.Crouch.performed += OnCrouchPerformed;
        _playerInput.Player.Crouch.canceled += OnCrouchCancelled;

        _playerInput.Enable();
    }

    private void OnDisable()
    {
        //// Unubscribes to Unity's input system
        _playerInput.Player.Movement.performed -= OnMovementPerformed;
        _playerInput.Player.Movement.canceled -= OnMovementCancelled;

        _playerInput.Player.Run.performed -= OnRunPerformed;
        _playerInput.Player.Run.canceled -= OnRunCancelled;
        
        _playerInput.Player.Crouch.performed -= OnCrouchPerformed;
        _playerInput.Player.Crouch.canceled -= OnCrouchCancelled;

        _playerInput.Disable();
    }
    
    public void OnProcessingChanged()
    {
        if (IsProcessing) return;
        rawInputMovement = Vector3.zero; // Resets input
        ProcessedInputMovement = Vector3.zero;
        IsCrouchPressed = false;
        IsRunningPressed = false;
    }
    
    public bool InputMovementCheck()
    {
        // If no input, return false
        if (ProcessedInputMovement.magnitude > 0)
        {
            return true;
        }

        return false;
    }

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        rawInputMovement = value.ReadValue<Vector3>();

        if (IsProcessing)
        {
            ProcessedInputMovement = rawInputMovement;
        } else
        {
            rawInputMovement = Vector3.zero; // Resets input
            ProcessedInputMovement = rawInputMovement;
        }
    }

    private void OnMovementCancelled(InputAction.CallbackContext value)
    {
        // This never really gets called, unless external factors, like tabbing out, good for releasing sticky inputs
        rawInputMovement = Vector3.zero; // Resets input
        ProcessedInputMovement = Vector3.zero;
    }
    
    private void OnCrouchPerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return;
        IsCrouchPressed = true;
    }
    
    private void OnCrouchCancelled(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return;
        IsCrouchPressed = false;
    }

    private void OnRunPerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return; 
        IsRunningPressed = true;
    }

    private void OnRunCancelled(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return; 
        IsRunningPressed = false;
    }

    private void FixedUpdate()
    {
        IsRunning = IsRunningPressed && !IsExhausted;
        
        DirectionalWalking(ProcessedInputMovement);

        if (Stamina <= 0)
        {
            IsExhausted = true;
        } else if (Stamina > MinimumRunningStamina)
        {
            IsExhausted = false;
        }
        
        StaminaRegeneration(StaminaGain);
        StaminaBarUI();

        if ((IsCrouching || isMidCrouch) && !IsCrouchPressed && CanStand())
        {
            StartCoroutine(OnStand());
        }
    }

    private void Update()
    {
        if (!IsStanding) return;
        
        if (MovementSpeed > 0 && rb.linearVelocity.magnitude > 0.1f)
        {
            HeadSway();
        }
        else
        {
            IdleHeadSway();
        }
    }

    private float _lastSinValue;
    private void HeadSway()
    {
        float speed = MovementSpeed;
        
        float speedX = speed * swaySpeedVelocityScale.x;
        float speedY = speed * swaySpeedVelocityScale.y;
        float ampX = speed * swayAmplitudeVelocityScale.x;
        float ampY = speed * swayAmplitudeVelocityScale.y;

        float offsetX = Mathf.Cos(Time.time * (swaySpeed.x + speedX)) * (swayAmplitude.x + ampX);
        float offsetY = Mathf.Sin(Time.time * (swaySpeed.y + speedY)) * (swayAmplitude.y + ampY);

        Vector3 offset =
            head.transform.right * offsetX +
            head.transform.up * Mathf.Abs(offsetY);
        
        if (footstepDelayTimer > 0) footstepDelayTimer = Mathf.Max(0, footstepDelayTimer -= Time.fixedDeltaTime);
        if (Mathf.Sign(offsetY) != Mathf.Sign(_lastSinValue) && footstepDelayTimer <= 0) Footstep();
        _lastSinValue = offsetY;

        
        head.transform.localPosition = Vector3.Lerp(head.transform.localPosition, hitboxStandHeadPosition + offset, Time.deltaTime / swaySmoothTime);
    }

    private void Footstep()
    {
        footstepDelayTimer = footstepDelayTimerSet;
        float volume = IsRunning ? 0.07f : 0.03f;

        AudioClip footstepSFX = GetFootstepSFX();
        if (footstepSFX != null)  ManagerSFX.Instance.PlaySFX(footstepSFX, feet.transform.position, volume, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }

    [SerializeField] private float footstepRaycastDistance;
    private AudioClip GetFootstepSFX()
    {
        Ray ray = new Ray(head.transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, footstepRaycastDistance, LayerUtility.Environment))
        {
            if (hit.transform.gameObject.TryGetComponent(out GroundMaterialHolder gmh))
            {
                return gmh.groundMaterial.sfxs[Random.Range(0, gmh.groundMaterial.sfxs.Length - 1)];
            }
            return footstepSounds[Random.Range(0, footstepSounds.Length - 1)];
        }

        return null;
    }

    private void IdleHeadSway()
    {
        float idleOffsetY = Mathf.Sin(Time.time * swayIdleSpeed) * swayIdleAmplitude; // breathing
        Vector3 offset = head.transform.up * idleOffsetY;
        head.transform.localPosition = Vector3.Lerp(head.transform.localPosition, hitboxStandHeadPosition + offset, Time.deltaTime / swaySmoothTime);
    }


    /// <summary>
    /// Method for basic directional movement.
    /// </summary>
    private void DirectionalWalking(Vector3 movementInputs)
    {
        float inputX = movementInputs.x;
        float inputZ = movementInputs.z;

        // Create a direction vector relative to the camera's rotation
        Vector3 direction = (head.transform.forward.ProjectOntoPlane(Vector3.up) * inputZ + head.transform.right * inputX).normalized;

        // Zero out the Y component to prevent upward movement
        direction.y = 0;

        float speed = 0;
        if (canMove) { speed = MovementSpeed; }
        Vector3 targetVelocity = direction * speed;
        Vector3 velocityDifference = targetVelocity - rb.linearVelocity;

        // Apply acceleration or deceleration based on the input
        float movementForceX;
        float movementForceZ;

        // Acceleration
        float accelRate;
        if (Mathf.Abs(targetVelocity.magnitude) > 0.01f)
        {
            accelRate = acceleration;
        }
        else
        {
            accelRate = deceleration;
        }

        movementForceX = Mathf.Pow(Mathf.Abs(velocityDifference.x) * accelRate, velocityPower) * Mathf.Sign(velocityDifference.x);
        movementForceZ = Mathf.Pow(Mathf.Abs(velocityDifference.z) * accelRate, velocityPower) * Mathf.Sign(velocityDifference.z);
        Vector3 dirSpeed = new Vector3(movementForceX, 0f, movementForceZ);

        // Apply force to the Rigidbody
        rb.AddForce(dirSpeed);
    }

    /// <summary>
    /// Gains or loses Stamina  based off of Stamina  gain. 
    /// </summary>
    private void StaminaRegeneration(float gain)
    {
        Stamina = Mathf.Clamp(Stamina + (Time.fixedDeltaTime * gain), 0f, StaminaMax);
    }

    public void SetMovementSpeed(float speed)
    {
        this.MovementSpeed = speed;
    }

    public void ToggleMovement(bool state)
    {
        canMove = state;
    }

    private void StaminaBarUI()
    {
        // Resize the Stamina  bar based on current Stamina 
        float width = staminaBarWidth * (Stamina  / StaminaMax);
        staminaBarRectTransform.sizeDelta = new Vector2(width, staminaBarRectTransform.sizeDelta.y);

        // Fade alpha based on Stamina  state
        float targetAlpha = (Stamina  >= StaminaMax) ? staminaBarRestingAlpha : staminaBarActiveAlpha;
        Color currentColor = staminaBarImage.color;
        currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * staminaBarFadeMultiplier);
        staminaBarImage.color = currentColor;

        // Apply exhausted tint or return to white
        Color exhaustedTargetColor = (Stamina < MinimumRunningStamina && IsExhausted)
            ? staminaBarExhaustedColor
            : Color.white;

        exhaustedTargetColor.a = staminaBarImage.color.a; // Preserve alpha
        staminaBarImage.color = Color.Lerp(staminaBarImage.color, exhaustedTargetColor, Time.deltaTime * staminaBarExhaustedMultiplier);

        // Pulse effect while regenerating Stamina 
        if (StaminaGain > 0 && Stamina  < StaminaMax)
        {
            Color.RGBToHSV(staminaBarImage.color, out float h, out float s, out float v);
            float pulse = (Mathf.Sin(Time.time * staminaBarPulseFrequency) * 0.5f) + 0.5f; // 0–1 range
            float pulsedV = Mathf.Clamp01(0.6f + 0.4f * pulse); // Brightness from 60% to 100%
            Color pulsedColor = Color.HSVToRGB(h, s, pulsedV);
            pulsedColor.a = staminaBarImage.color.a;
            staminaBarImage.color = pulsedColor;
        }
    }

    
    private Vector3 _midCrouchVelocity;
    private Vector3 _topCrouchVelocity;
    private Vector3 _crouchHeadVelocity;

    public IEnumerator OnCrouch(float smoothTime = 0.2f)
    {
        isMidCrouch = true;
        IsCrouching = false;
        IsStanding = false;
        CapsuleCollider bot = hitboxes[0].GetComponent<CapsuleCollider>();
        CapsuleCollider mid = hitboxes[1].GetComponent<CapsuleCollider>();
        CapsuleCollider top = hitboxes[2].GetComponent<CapsuleCollider>();
        
        
        while (IsCrouchPressed && isMidCrouch)
        {
            IsCrouching = false;
            IsStanding = false;

            mid.center = Vector3.SmoothDamp(
                mid.center,
                bot.center,
                ref _midCrouchVelocity,
                smoothTime
            );
            
            top.center = Vector3.SmoothDamp(
                top.center,
                bot.center,
                ref _topCrouchVelocity,
                smoothTime
            );
            
            head.transform.localPosition = Vector3.SmoothDamp(
                head.transform.localPosition,
                hitboxCrouchHeadPosition,
                ref _crouchHeadVelocity,
                smoothTime
            );

            if ((Vector3.Distance(head.transform.localPosition, hitboxCrouchHeadPosition) < 0.01f)
                && (Vector3.Distance(mid.center, bot.center) < 0.01f))
            {
                head.transform.localPosition = hitboxCrouchHeadPosition;
                mid.center = bot.center;
                top.center = bot.center;
                
                IsCrouching = true;
                isMidCrouch = false;
            }

            yield return null;
        }
    }

    private IEnumerator OnStand(float smoothTime = 0.2f)
    {
        isMidCrouch = true;
        IsCrouching = false;
        IsStanding = false;
        CapsuleCollider bot = hitboxes[0].GetComponent<CapsuleCollider>();
        CapsuleCollider mid = hitboxes[1].GetComponent<CapsuleCollider>();
        CapsuleCollider top = hitboxes[2].GetComponent<CapsuleCollider>();
        
        while (!IsCrouchPressed && isMidCrouch)
        {
            IsCrouching = false;
            IsStanding = false;
            
            mid.center = Vector3.SmoothDamp(
                mid.center,
                hitboxMidStandCenter,
                ref _midCrouchVelocity,
                smoothTime
            );
            
            top.center = Vector3.SmoothDamp(
                top.center,
                hitboxTopStandCenter,
                ref _topCrouchVelocity,
                smoothTime
            );

            head.transform.localPosition = Vector3.SmoothDamp(
                head.transform.localPosition,
                hitboxStandHeadPosition,
                ref _crouchHeadVelocity,
                smoothTime
            );
            
            if ((Vector3.Distance(head.transform.localPosition, hitboxStandHeadPosition) < 0.01f) 
                && (Vector3.Distance(mid.center, hitboxMidStandCenter) < 0.01f))
            {
                head.transform.localPosition = hitboxStandHeadPosition;
                mid.center = hitboxMidStandCenter;
                top.center = hitboxTopStandCenter;
                
                isMidCrouch = false;
                IsStanding = true;
            }

            yield return null;
        }
    }

    private bool CanStand()
    {
        CapsuleCollider top = hitboxes[0].GetComponent<CapsuleCollider>();
        Vector3 pos = hitboxTop.transform.position;
        
        if (IsDetecting(pos + Vector3.forward * top.radius)) return false;
        if (IsDetecting(pos - Vector3.forward * top.radius)) return false;
        if (IsDetecting(pos + Vector3.right * top.radius)) return false;
        if (IsDetecting(pos - Vector3.right * top.radius)) return false;

        return true;

        bool IsDetecting(Vector3 pos)
        {
            Ray ray = new Ray(pos, hitboxTop.transform.up);

            if (Physics.Raycast(ray, out RaycastHit hit, hitboxStandRaycastDistance, LayerUtility.Environment))
            {
                Debug.DrawRay(hitboxTop.transform.position, hitboxTop.transform.up * hit.distance, Color.magenta);
                return true;
            }
            Debug.DrawRay(hitboxTop.transform.position, hitboxTop.transform.up * hitboxStandRaycastDistance, Color.magenta);

            return false;
        }
    }

}
