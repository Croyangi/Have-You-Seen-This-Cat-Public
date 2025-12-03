using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVisibilityHelper : MonoBehaviour, IProcessable
{
    [SerializeField] private PlayerFlashlightHelper flashlightHelper;
    [SerializeField] private PlayerMovementHelper movementHelper;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerUIHelper uiHelper;

    [Header("Settings")]
    [field: SerializeField]
    public float Visibility { get; private set; }

    [SerializeField] private float baseVisibility;

    [SerializeField] private float flashlightVisibility;
    [SerializeField] private float movementVisibility;
    [SerializeField] private float crouchingVisibility;

    [SerializeField] private float closedVisibilityThreshold;
    [SerializeField] private float midVisibilityThreshold;
    [SerializeField] private float openVisibilityThreshold;
    [SerializeField] private Color spottedColor;
    [SerializeField] private Color aggressiveColor;
    [SerializeField] private Color defaultColor;

    [field: SerializeField] public bool IsProcessing { get; set; }

    private void Awake()
    {
        var runtimeController = animator.runtimeAnimatorController;
        var newController = Instantiate(runtimeController);
        animator.runtimeAnimatorController = newController;
    }
    
    private void Start()
    {
        _managerCopyCat = ManagerCopyCat.Instance;
        PlayAnimation(Closed);
        visibilityEyesImage.color = defaultColor;
        _hasBeenSpotted = false;
    }
    
    private void OnEnable()
    {
        ManagerGame.OnGameStart += OnGameStart;
        ManagerGame.OnGameEnd += OnGameEnd;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= OnGameStart;
        ManagerGame.OnGameEnd -= OnGameEnd;
    }

    private bool _isGameOngoing;
    private Coroutine _gameplayUpdateTick;
    private void OnGameStart()
    {
        _managerCopyCat = ManagerCopyCat.Instance;
        _isGameOngoing = true;
        
        if (_gameplayUpdateTick != null) StopCoroutine(_gameplayUpdateTick);
        _gameplayUpdateTick = StartCoroutine(GameplayUpdateTick());
    }

    private void OnGameEnd()
    {
        _isGameOngoing = false;
    }

    private ManagerCopyCat _managerCopyCat;
    private bool _hasBeenSpotted;
    private IEnumerator GameplayUpdateTick()
    {
        while (_isGameOngoing)
        {
            Visibility = baseVisibility;
        
            if (flashlightHelper.IsFlashlightOn)
            {
                Visibility += flashlightVisibility;
            }

            if (movementHelper.MovementSpeed > 0 && rb.linearVelocity.magnitude > 0.1f)
            {
                Visibility += movementVisibility;
            }

            if (movementHelper.IsCrouchPressed)
            {
                Visibility -= crouchingVisibility;
            }
        
            Visibility = Mathf.Max(0, Visibility);

            if (!uiHelper.GetVisibility(uiHelper.VisibilityEyes))
            {
                yield return null;
                continue; 
            }

            if (_managerCopyCat.CopyCat != null && _managerCopyCat.CopyCatPathfindingHelper.CanSeePlayer)
            {
                if (!_hasBeenSpotted)
                {
                    PlayAnimation(Spotted);
                    _hasBeenSpotted = true;
                    if (_jitterVisibilityEyes != null) StopCoroutine(_jitterVisibilityEyes);
                    _jitterVisibilityEyes = StartCoroutine(JitterVisibilityEyes());
                    visibilityEyesImage.color = spottedColor;
                }
            }
            else if (Visibility >= openVisibilityThreshold)
            {
                PlayAnimation(Open);
            } else if (Visibility >= midVisibilityThreshold)
            {
                PlayAnimation(Mid);
            } else
            {
                PlayAnimation(Closed);
            }
        
            if (_managerCopyCat.CopyCat != null && !_managerCopyCat.CopyCatPathfindingHelper.CanSeePlayer && _hasBeenSpotted)
            {
                visibilityEyesImage.color = aggressiveColor;
                _hasBeenSpotted = false;
            } else if (_managerCopyCat.CopyCat != null && !_managerCopyCat.CopyCatPathfindingHelper.IsFearless)
            {
                visibilityEyesImage.color = defaultColor;
            }

            yield return null;
        }
    }

    public float GetVisibilityMultiplier()
    {
        return Visibility;
    }
    
    [Header("Animator References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Image visibilityEyesImage;
    [SerializeField] private RectTransform visibilityEyesRect;

    [Header("State References")]
    [field: SerializeField] public string CurrentState { get; private set; }

    public string Closed { get; private set; } = "VisibilityEyes0";
    public string Mid { get; private set; } = "VisibilityEyes1";
    public string Open { get; private set; } = "VisibilityEyes2";
    public string Spotted { get; private set; } = "VisibilityEyes3";

    public void PlayAnimation(string state, bool force = false)
    {
        int index = animator.GetLayerIndex("Base");

        if (!animator.isActiveAndEnabled) return;

        if (state.Equals(CurrentState) && !force)
        {
            return;
        }
        
        animator.Play(state, index);
        
        CurrentState = state;
        
        animator.Update(0f);
    }

    private Vector3 _initVisibilityEyesPos;
    [SerializeField] private float visibilityEyesJitterStrength;
    private Coroutine _jitterVisibilityEyes;
    private IEnumerator JitterVisibilityEyes()
    {
        _initVisibilityEyesPos = visibilityEyesImage.transform.localPosition;
        
        float jitterAmount = visibilityEyesJitterStrength;
        
        while (ManagerCopyCat.Instance.CopyCatPathfindingHelper.CanSeePlayer)
        {
            Vector3 jitter = new Vector3(
                Random.Range(-jitterAmount, jitterAmount),
                Random.Range(-jitterAmount, jitterAmount),
                0f
            );

            visibilityEyesImage.transform.localPosition = _initVisibilityEyesPos + jitter;
            yield return new WaitForFixedUpdate();
        }
        
        visibilityEyesImage.transform.localPosition = _initVisibilityEyesPos;
    }
}
