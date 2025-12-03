using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;
using Random = UnityEngine.Random;

public class LocateSignalMinigame : MonoBehaviour, ITerminalMinigame
{
    private PlayerInput _playerInput = null;
    [SerializeField] private TerminalMinigameBase terminalMinigameBase;

    [Header("Minigame")] 
    [SerializeField] private float confirmStayTime;
    
    [SerializeField] private AudioClip modemBeep;
    [SerializeField] private AudioClip tabletClick;
    
    [SerializeField] private Triangle upArrow;
    [SerializeField] private Triangle downArrow;
    [SerializeField] private Triangle leftArrow;
    [SerializeField] private Triangle rightArrow;
    [SerializeField] private Color inactiveArrowColor;
    [SerializeField] private Color activeArrowColor;

    [SerializeField] private GameObject dot;
    [SerializeField] private float movementSpeed;
    [SerializeField] private Vector2 goal;
    [SerializeField] private float goalDistancePadding;
    private float boxBounds = 0.11f;
    private float cursorBounds = 0.12f;

    [SerializeField] private GameObject hintDotsParent;
    private List<GameObject> _hintDots = new List<GameObject>();

    [SerializeField] private GameObject verticalGuideLine;
    [SerializeField] private GameObject horizontalGuideLine;

    [SerializeField] private GameObject signalRingsParent;
    private List<Disc> _signalRings = new List<Disc>();
    
    private Vector2 _inputVector;
    public Vector2 dotPosition;
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
        
        goal = new Vector2(Random.Range(-boxBounds, boxBounds), Random.Range(-boxBounds, boxBounds));
        
        for (int i = 0; i < hintDotsParent.transform.childCount; i++)
        {
            _hintDots.Add(hintDotsParent.transform.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < signalRingsParent.transform.childCount; i++)
        {
            _signalRings.Add(signalRingsParent.transform.GetChild(i).gameObject.GetComponent<Disc>());
        }
            
        _hintDots[0].transform.localPosition = goal;
        for (int i = 1; i < _hintDots.Count; i++)
        {
            _hintDots[i].transform.localPosition = new Vector2(Random.Range(-boxBounds, boxBounds), Random.Range(-boxBounds, boxBounds));
        }

        StartCoroutine(AnimateSignal());
    }
    
    [SerializeField] private float signalAnimationOffsetTime;
    [SerializeField] private float signalAnimationPulseMultiplier;
    [SerializeField] private Color inactiveSignalColor;
    [SerializeField] private Color activeSignalColor;
    
    private IEnumerator AnimateSignal()
    {
        while (true)
        {
            for (int i = 0; i < _signalRings.Count; i++)
            {
                if (Mathf.InverseLerp(0.167f, 0f, Vector3.Distance(goal, dotPosition)) < (1f / _signalRings.Count) * i)
                {
                    _signalRings[i].Color = inactiveSignalColor;
                    continue;
                }
                
                Disc disc = _signalRings[i];
                float H, S, V;
                Color.RGBToHSV(activeSignalColor, out H, out S, out V);
                
                //Debug.Log(Mathf.InverseLerp(0.167f, 0f, Vector3.Distance(goal, dotPosition)));
                
                float amplitude = Mathf.InverseLerp(0.167f, 0f, Vector3.Distance(goal, dotPosition));
                float pulse = Mathf.Sin(Time.time * signalAnimationPulseMultiplier + signalAnimationOffsetTime * (_signalRings.Count - 1 - i)) * amplitude;
                
                float normalizedPulse = pulse * 0.5f + 0.5f;
                float minPulse = 0.1f;
                float animatedV = Mathf.Lerp(minPulse, V, normalizedPulse);
                disc.Color = Color.HSVToRGB(H, S, animatedV);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private bool _isCompleting;
    private void Update()
    {
        Vector2 move = Time.deltaTime * movementSpeed * _inputVector;
        Vector2 p = (Vector2)dot.transform.localPosition + move;

        Vector2 orig = p;

        p.x = Mathf.Clamp(p.x, -cursorBounds, cursorBounds);
        p.y = Mathf.Clamp(p.y, -cursorBounds, cursorBounds);
        
        if (move.magnitude > 0)
        {
            if (!_isMoving)
            {
                _isMoving = true;
                StartCoroutine(MoveSFX());
            }
        }
        else if (_isMoving)
        {
            _isMoving = false;
        }

        dot.transform.localPosition = p;
        dotPosition = p;

        verticalGuideLine.transform.localPosition = new Vector2(p.x, 0);
        horizontalGuideLine.transform.localPosition = new Vector2(0, p.y);
        
        if (Vector2.Distance(p, goal) < goalDistancePadding)
        {
            if (!_isCompleting)
            {
                _isCompleting = true;
                if (_confirmStandby != null) StopCoroutine(_confirmStandby);
                _confirmStandby = StartCoroutine(ConfirmStandby());
                ManagerSFX.Instance.PlaySFX(modemBeep, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            }
        }
        else if (_isCompleting)
        {
            _isCompleting = false;
            if (_confirmStandby != null) StopCoroutine(_confirmStandby);
        }
    }

    private bool _isMoving;
    private IEnumerator MoveSFX()
    {
        while (_isMoving)
        {
            ManagerSFX.Instance.PlaySFX(tabletClick, transform.position,0.05f, false, ManagerAudioMixer.Instance.AMGSFX);
            yield return new WaitForSeconds(0.07f);
        }
    }


    private void OnActionPerformed(InputAction.CallbackContext value)
    {
        _inputVector = value.ReadValue<Vector2>();
        
        SetArrowsUI();
    }

    private void OnActionCanceled(InputAction.CallbackContext value)
    {
        _inputVector = value.ReadValue<Vector2>();
        
        SetArrowsUI();
    }
    
    private void SetArrowsUI()
    {
        rightArrow.Color = _inputVector.x > 0 ? activeArrowColor : inactiveArrowColor;
        leftArrow.Color  = _inputVector.x < 0 ? activeArrowColor : inactiveArrowColor;
        upArrow.Color    = _inputVector.y > 0 ? activeArrowColor : inactiveArrowColor;
        downArrow.Color  = _inputVector.y < 0 ? activeArrowColor : inactiveArrowColor;
    }
    
    private Coroutine _confirmStandby;
    private IEnumerator ConfirmStandby()
    {
        StartCoroutine(ConfirmStandbySFX());
        yield return new WaitForSeconds(confirmStayTime);
        terminalMinigameBase.OnMinigameEnd();
    }
    
    private IEnumerator ConfirmStandbySFX()
    {
        float pitch = 0.5f;
        while (_isCompleting)
        {
            ManagerSFX.Instance.PlaySFX(tabletClick, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: pitch, isRandomPitch: false);
            pitch += 0.01f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    [ContextMenu("SubscribeToInput")]
    private void SubscribeToInput()
    {
        _playerInput.Enable();
        _playerInput.TerminalMinigames.DivertPower.performed += OnActionPerformed;
        _playerInput.TerminalMinigames.DivertPower.canceled += OnActionCanceled;
    }
    
    private void UnsubscribeToInput()
    {
        _playerInput.TerminalMinigames.DivertPower.performed -= OnActionPerformed;
        _playerInput.TerminalMinigames.DivertPower.canceled -= OnActionCanceled;
        _playerInput.Disable();
    }
    
    public void OnMinigameStart(TerminalMinigameBase minigameBase)
    {
        terminalMinigameBase = minigameBase;
    }
    
    public void OnMinigameEnd()
    {
        StopAllCoroutines();
        UnsubscribeToInput();
    }

    public void OnMinigameFocus()
    {
        SubscribeToInput();
    }
    
    public void OnMinigameUnfocus()
    {
        UnsubscribeToInput();
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
