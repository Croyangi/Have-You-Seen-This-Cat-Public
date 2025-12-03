using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerTabletHelper : MonoBehaviour, IProcessable
{
    [Header("References")]
    [SerializeField] private PlayerInputHelper inputHelper;
    [SerializeField] private PlayerFlashlightHelper flashlightHelper;
    [field: SerializeField] public GameObject Tablet { get; private set; }
    [SerializeField] private GameObject[] tabletHiders;

    [field: SerializeField] public bool IsFullyHidden { get; private set; }
    [SerializeField] private GameObject playerHead;
    [field: SerializeField] public bool IsProcessing { get; set; }
    [field: SerializeField] public bool HasObtained { get; set; }
    
    
    [Header("Settings")]
    [SerializeField] private Vector3 shownPosOffset;
    [SerializeField] private Vector3 hiddenPosOffset;
    [SerializeField] private Vector3 rotOffset;
    [SerializeField] private float slerpScale;
    [SerializeField] private float lerpScale;
    
    [SerializeField] private Vector3 swayMultiplier;
    [SerializeField] private Vector3 swaySpeed;
    
    
    [SerializeField] private GameObject computerHumSfxObject;
    [SerializeField] private AudioClip computerHum;
    [SerializeField] private AudioClip[] tabletToggleShows;
    
    [SerializeField] private PlayerUIHelper uiHelper;
    
    private ManagerTablet _managerTablet;
    private PlayerInput _playerInput;

    private bool _isGameOngoing;
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
        ObtainTablet();
    }

    private void OnGameStart()
    {
        _isGameOngoing = true;
        _managerTablet = ManagerTablet.Instance;
        ProcessObtainCheck();
        ApplyTablet();
    }

    private void OnGameEnd()
    {
        _isGameOngoing = false;
    }
    
    private void Start()
    {
        ProcessObtainCheck();
        IsFullyHidden = true;
        ApplyTablet();
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += OnGameStart;
        ManagerGame.OnGameEnd += OnGameEnd;
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= OnGameStart;
        ManagerGame.OnGameEnd -= OnGameEnd;
        UnsubscribeToInput();
        _playerInput.Disable();
    }

    private void SubscribeToInput()
    {
        _playerInput.Tablet.Toggle.performed += OnTabletTogglePerformed;
    }
    
    private void UnsubscribeToInput()
    {
        _playerInput.Tablet.Toggle.performed -= OnTabletTogglePerformed;
    }

    private void OnTabletTogglePerformed(InputAction.CallbackContext value)
    {
        if (!IsProcessing) return;
        _managerTablet.OnToggleTablet();
        ApplyTablet();
    }
    
    [ContextMenu("Obtain Tablet")]
    public void ObtainTablet()
    {
        HasObtained = true;
        ProcessObtainCheck();
    }

    [ContextMenu("Process Obtain Check")]
    public void ProcessObtainCheck()
    {
        Tablet.SetActive(HasObtained);
        if (HasObtained)
        {
            uiHelper.SetVisibility(true, new List<IUI> { uiHelper.Tablet}, "tablet");
            SubscribeToInput();
        }
        else
        {
            uiHelper.SetVisibility(false, new List<IUI> { uiHelper.Tablet}, "tablet");
            UnsubscribeToInput();
        }
    }

    
    [SerializeField] private Image pawPadImage;
    [SerializeField] private TextMeshProUGUI pawPadHotkeyText;
    [SerializeField] private Color disabledColor;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color activeColor;
    private void FixedUpdate()
    {
        if (!HasObtained) return;
        if (!IsProcessing)
        {
            pawPadImage.color = disabledColor;
            pawPadHotkeyText.color = disabledColor;
            return;
        }
        
        Color color = IsFullyHidden ? defaultColor : activeColor;
        pawPadImage.color = color;
        pawPadHotkeyText.color = color;
    }

    private void ApplyTablet()
    {
        if (_managerTablet == null) return;
        
        StopAllCoroutines();
        if (_managerTablet.IsShowing)
        {
            if (computerHumSfxObject != null) Destroy(computerHumSfxObject);
            computerHumSfxObject = ManagerSFX.Instance.PlaySFX(computerHum, Tablet.transform.position, 0.01f, true, ManagerAudioMixer.Instance.AMGSFX, Tablet.transform).gameObject;
            ManagerSFX.Instance.PlaySFX(tabletToggleShows[0], Tablet.transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, Tablet.transform);

            foreach (GameObject obj in tabletHiders)
            {
                obj.SetActive(true);
            }
            
            inputHelper.SetProcessing(false, inputHelper.Tablet, "tablet");
            IsFullyHidden = false;
            flashlightHelper.SetForceNear(true);
        }
        else
        {
            ManagerSFX.Instance.PlaySFX(tabletToggleShows[1], Tablet.transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, Tablet.transform);
            
            inputHelper.SetProcessing(true, inputHelper.Tablet, "tablet");
            StartCoroutine(FullyHiddenTimer());
            flashlightHelper.SetForceNear(false);
        }
    }
    
    private void Update()
    {
        if (!IsProcessing || _managerTablet == null)
        {
            TabletFollow(Tablet, false);
            return;
        }
        TabletFollow(Tablet, _managerTablet.IsShowing);
    }

    private IEnumerator FullyHiddenTimer()
    {
        yield return new WaitForSeconds(1 / lerpScale);
        IsFullyHidden = true;
        
        foreach (GameObject obj in tabletHiders)
        {
            obj.SetActive(false);
        }
        
        if (computerHumSfxObject != null)
        {
            Destroy(computerHumSfxObject);
        }
    }
    
    /// <summary>
    /// Controls inspecting free-rotation
    /// </summary>
    private void TabletFollow(GameObject obj, bool doShow)
    {
        // Position offset in camera's local space
        Vector3 offset = doShow ? shownPosOffset : hiddenPosOffset;
        
        Vector3 localOffset = offset; // e.g., new Vector3(0, -0.5f, 1f);
        Vector3 targetPos = playerHead.transform.TransformPoint(localOffset);

        // Smooth position follow
        if (IsFullyHidden)
        {
            obj.transform.position = targetPos;
        }
        else
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, Time.deltaTime * lerpScale);
        }

        // Rotation offset in camera's local space
        Quaternion sway = Quaternion.Euler(
            Mathf.Sin(Time.time * swaySpeed.x) * swayMultiplier.x,  // slight X sway
            Mathf.Cos(Time.time * swaySpeed.y) * swayMultiplier.y,  // slight Y sway
            0
        );
        Quaternion targetRot = playerHead.transform.rotation * sway * Quaternion.Euler(rotOffset);

        // Smooth rotation follow
        obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRot, Time.deltaTime * slerpScale);
    }


}
