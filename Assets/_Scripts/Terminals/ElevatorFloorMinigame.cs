using System;
using System.Collections;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;

public class ElevatorFloorMinigame : MonoBehaviour, ITerminalMinigame, IDataPersistence
{
    private PlayerInput _playerInput = null;
    [SerializeField] private float loginTimer;
    [SerializeField] private float loginSet;
    [SerializeField] private bool isLoggingIn;
    [SerializeField] private bool isFocused;
    [SerializeField] private Line progressBar;
    [SerializeField] private Color progressBarColor;
    [SerializeField] private TerminalMinigameBase terminalMinigameBase;
    [SerializeField] private AudioClip loginSFX;
    [SerializeField] private AudioClip clickSFX;
    [SerializeField] private AudioClip errorSFX;
    
    [SerializeField] private GameObject hasSaveFileUI;
    [SerializeField] private GameObject noSaveFileUI;

    [SerializeField] private Triangle arrowLeft;
    [SerializeField] private Triangle arrowRight;
    [SerializeField] private Color arrowActiveColor;
    [SerializeField] private Color arrowInactiveColor;

    [SerializeField] private Rectangle newExpeditionOutline;
    [SerializeField] private Rectangle saveFileOutline;
    [SerializeField] private Line newExpeditionBacking;
    [SerializeField] private Line saveFileBacking;
    
    [SerializeField] private Color backingActiveColor;
    [SerializeField] private Color backingInactiveColor;
    [SerializeField] private Color outlineActiveColor;
    [SerializeField] private Color outlineInactiveColor;
    
    [SerializeField] private Color outlineDisabledColor;
    [SerializeField] private Color backingDisabledColor;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        
        loginTimer = 0;
        progressBarColor = progressBar.Color;
    }

    private void Start()
    {
        LoadData(ManagerDataPersistence.Instance.GetGameData());
        SetFilesUI(-1);
    }
    
    [SerializeField] private TextMeshProShapes floorTextMesh;
    [SerializeField] private TextMeshProShapes timeTextMesh;
    [SerializeField] private TextMeshProShapes catsCollectedTextMesh;
    private bool _isOngoing;

    public void LoadData(GameData data)
    {
        _isOngoing = data.expedition.isOngoing;
        if (data.expedition.isOngoing)
        {
            hasSaveFileUI.SetActive(true);
            noSaveFileUI.SetActive(false);

            floorTextMesh.text = data.expedition.floor + "";
            catsCollectedTextMesh.text = data.expedition.catsCollected + "";

            int currentTime = data.expedition.time;
            TimeSpan timeElapsed = TimeSpan.FromSeconds(currentTime);
            timeTextMesh.text = timeElapsed.Minutes.ToString("00") + ":" + timeElapsed.Seconds.ToString("00");
        }
        else
        {
            hasSaveFileUI.SetActive(false);
            noSaveFileUI.SetActive(true);
        }
    }
    
    public void SaveData(ref GameData data)
    {
    }
    
    private float _end = 0.28f;
    private void FixedUpdate()
    {
        float t = Mathf.Clamp01(loginTimer / (loginSet - (loginSet / 4))); // normalize time (0 to 1)
        float eased = 1 - Mathf.Pow( 1 - t, 3); // ease-in curve
        float progress = Mathf.Lerp(0f, _end, eased);
        progressBar.End = new Vector3(progress, 0f, 0f);

        if (loginTimer >= loginSet - 1)
        {
            float H, S, V;
            Color.RGBToHSV(progressBarColor, out H, out S, out V);
            float pulse = (Mathf.Sin(Time.time * 15f) * 0.5f) + 0.5f; // value from 0 to 1
            float animatedV = Mathf.Clamp01(V * (0.6f + 0.4f * pulse)); // modulate brightness between 70%–100%
            progressBar.Color = Color.HSVToRGB(H, S, animatedV);
        }
        else
        {
            progressBar.Color = progressBarColor;
        }
        
        if (isLoggingIn)
        {
            loginTimer = Mathf.Min(loginSet, loginTimer += Time.fixedDeltaTime);
            if (loginTimer >= loginSet)
            {
                terminalMinigameBase.OnMinigameEnd();
            }
        }
    }
    
    private IEnumerator PlayLoginSFX()
    {
        while (isLoggingIn)
        {
            ManagerSFX.Instance.PlaySFX(loginSFX, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.6f + loginTimer / 10f, isRandomPitch: false);
            yield return new WaitForSeconds(0.07f);
        }
    }
    
    private void OnLoginPerformed(InputAction.CallbackContext value)
    {
        isLoggingIn = true;
        StartCoroutine(PlayLoginSFX());
    }
    
    private void OnLoginCancelled(InputAction.CallbackContext value)
    {
        CancelLogin();
    }

    private bool _isLoadingSave;
    private void OnActionPerformed(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();

        if (!_isOngoing && input.x > 0)
        {
            input.x = 0;
            ManagerSFX.Instance.PlaySFX(errorSFX, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            return;
        }

        _isLoadingSave = input.x > 0;

        SetFilesUI(input.x);
        SetArrowsUI(input.x);
        ManagerSFX.Instance.PlaySFX(clickSFX, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }
    
    private void OnActionCancelled(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();
        SetArrowsUI(input.x);
    }

    private void SetFilesUI(float x)
    {
        newExpeditionOutline.Color = x < 0 ? outlineActiveColor : outlineInactiveColor;
        newExpeditionBacking.Color = x < 0 ? backingActiveColor : backingInactiveColor;
        
        if (_isOngoing)
        {
            saveFileOutline.Color = x > 0 ? outlineActiveColor : outlineInactiveColor;
            saveFileBacking.Color = x > 0 ? backingActiveColor : backingInactiveColor;
        }
        else
        {
            saveFileOutline.Color = outlineDisabledColor;
            saveFileBacking.Color = backingDisabledColor;
        }
    }

    private void SetArrowsUI(float x)
    {
        arrowLeft.Color = x < 0 ? arrowActiveColor : arrowInactiveColor;
        arrowRight.Color = x > 0 ? arrowActiveColor : arrowInactiveColor;
    }

    private void CancelLogin()
    {
        loginTimer = 0;
        isLoggingIn = false;
    }

    private void SubscribeToInput()
    {
        _playerInput.Enable();
        _playerInput.TerminalMinigames.Login.performed += OnLoginPerformed;
        _playerInput.TerminalMinigames.Login.canceled += OnLoginCancelled;
        _playerInput.TerminalMinigames.ElevatorFloor.performed += OnActionPerformed;
        _playerInput.TerminalMinigames.ElevatorFloor.canceled += OnActionCancelled;
    }
    
    private void UnsubscribeToInput()
    {
        _playerInput.TerminalMinigames.Login.performed -= OnLoginPerformed;
        _playerInput.TerminalMinigames.Login.canceled -= OnLoginCancelled;
        _playerInput.TerminalMinigames.ElevatorFloor.performed -= OnActionPerformed;
        _playerInput.TerminalMinigames.ElevatorFloor.canceled -= OnActionCancelled;
        _playerInput.Disable();
    }
    
    public class Result : ITerminalMinigameResult
    {
        public bool IsLoadingSave { get; }

        // Constructor
        public Result(bool isLoadingSave)
        {
            IsLoadingSave = isLoadingSave;
        }
    }

    public ITerminalMinigameResult GetResult()
    {
        return new Result(_isLoadingSave);
    }
    
    public void OnMinigameStart(TerminalMinigameBase minigameBase)
    {
        terminalMinigameBase = minigameBase;
    }
    
    public void OnMinigameEnd()
    {
        UnsubscribeToInput();
    }

    public void OnMinigameFocus()
    {
        SubscribeToInput();
    }
    
    public void OnMinigameUnfocus()
    {
        UnsubscribeToInput();
        CancelLogin();
    }

    private void OnDisable()
    {
        UnsubscribeToInput();
    }
    
    private void OnDestroy()
    {
        UnsubscribeToInput();
    }
}
