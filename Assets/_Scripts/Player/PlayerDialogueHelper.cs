using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerDialogueHelper : MonoBehaviour
{
    [SerializeField] private PlayerUIHelper uiHelper;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Febucci.UI.Core.TAnimCore textAnimator;
    [SerializeField] private Febucci.UI.Core.TypewriterCore typewriter;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private GameObject skipButton;

    [field: SerializeField] public bool IsTextShowed { get; private set; }
    [SerializeField] private bool hasTextStarted;
    [SerializeField] private bool canSkip;
    [SerializeField] private bool canCloseEarly;
    [SerializeField] private bool isAnimating;
    [field: SerializeField] public bool IsOngoing { get; private set; }
    [field: SerializeField] public bool IsContinuous { get; private set; }
    
    
    private PlayerInput _playerInput;
    private ManagerSFX _managerSFX;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void Start()
    {
        _managerSFX = ManagerSFX.Instance;
        dialogueBox.SetActive(false);
        uiHelper.SetVisibility(false, new List<IUI>() { uiHelper.Dialogue }, "dialogueHelper");
        ClearQueue();
    }

    private void OnEnable()
    {
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.SkipDialogue.performed -= OnSkip;
        _playerInput.Disable();
    }
    
    public void SetDialogue(string dialogue)
    {
        if (IsOngoing) return;
        
        if (IsContinuous)
        {
            StartCoroutine(OnDialogueStart(dialogue));
            return;
        }
        
        IsOngoing = true;
        StartCoroutine(OnDialogueStart(dialogue));
    }

    private Queue<string> _dialogueQueue = new Queue<string>();
    public void QueueDialogue(string dialogue)
    {
        if (!IsOngoing)
        {
            SetDialogue(dialogue);
        }
        else
        {
            _dialogueQueue.Enqueue(dialogue);
        }
    }

    public void ClearQueue()
    {
        _dialogueQueue.Clear();
    }

    public void Interrupt()
    {
        _dialogueQueue.Clear();
        if (_processingDialogue != null) StopCoroutine(_processingDialogue);
        if (IsOngoing) OnDialogueEnd();
    }

    private Coroutine _boxExpand;
    private IEnumerator DialogueBoxExpand()
    {
        if (_boxCollapse != null) StopCoroutine(_boxCollapse);
        
        isAnimating = true;
        
        DOTween.Complete(dialogueBox);
        dialogueBox.transform.DOScale(Vector3.zero, 0f);
        DOTween.Complete(dialogueBox);
        
        dialogueBox.SetActive(true);
        uiHelper.SetVisibility(true, new List<IUI>() { uiHelper.Dialogue }, "dialogueHelper");
        
        float duration = 0.4f;
        dialogueBox.transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
        
        yield return new WaitForSeconds(duration);
        isAnimating = false;
    }

    private Coroutine _boxCollapse;
    private IEnumerator DialogueBoxCollapse()
    {
        if (_boxExpand != null) StopCoroutine(_boxExpand);
        
        isAnimating = true;

        DOTween.Complete(dialogueBox);
        dialogueBox.transform.DOScale(Vector3.one, 0f);
        
        float duration = 0.3f;
        dialogueBox.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        yield return new WaitForSeconds(duration);
        dialogueBox.SetActive(false);
        uiHelper.SetVisibility(false, new List<IUI>() { uiHelper.Dialogue }, "dialogueHelper");
        isAnimating = false;
    }

    private Coroutine _processingDialogue;
    private IEnumerator OnDialogueStart(string dialogue)
    {
        dialogueText.text = "";
        
        if (!IsContinuous) _boxExpand = StartCoroutine(DialogueBoxExpand());
        
        while (isAnimating)
        {
            yield return null;
        }
        
        _playerInput.Player.SkipDialogue.performed += OnSkip;
        canSkip = true;
        
        StartCoroutine(TickDelay());
        
        typewriter.ShowText(dialogue);
        _processingDialogue = StartCoroutine(ProcessingDialogue());
    }

    private IEnumerator TickDelay()
    {
        hasTextStarted = true;
        yield return null;
        hasTextStarted = false;
    }

    private Coroutine _skipButtonAnimation;
    private IEnumerator SkipButtonAnimation()
    {
        while (canCloseEarly)
        {
            float duration0 = 0.6f;
            skipButton.transform.DOScale(Vector3.one * 1.1f, duration0).SetEase(Ease.InOutCubic);
            
            float duration1 = 0.6f;
            skipButton.transform.DOScale(Vector3.one * 0.9f, duration1).SetDelay(duration0).SetEase(Ease.InOutCubic);
            
            yield return new WaitForSeconds(duration0 + duration1);
        }
    }
    
    public void OnTextShowed()
    {
        if (hasTextStarted) return;
        
        IsTextShowed = true;
        canCloseEarly = true;
        _skipButtonAnimation = StartCoroutine(SkipButtonAnimation());
    }


    [SerializeField] private List<AudioClip> dialogueSFXs;
    [SerializeField] private float dialogueSFXTimer;
    [SerializeField] private float dialogueSFXTimerSet;
    public void OnCharacterShowed()
    {
        if (dialogueSFXTimer <= 0)
        {
            dialogueSFXTimer = dialogueSFXTimerSet;
            _managerSFX.PlayRawSFX(dialogueSFXs[Random.Range(0, dialogueSFXs.Count)], volume: 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        }
    }

    private void FixedUpdate()
    {
        if (dialogueSFXTimer > 0)
        {
            dialogueSFXTimer = Mathf.Max(0f, dialogueSFXTimer - Time.fixedDeltaTime);
        }
    }

    [SerializeField] private AudioClip skipSFX;
    private void OnSkip(InputAction.CallbackContext value)
    {
        if (!canSkip) return;
        
        _managerSFX.PlayRawSFX(skipSFX, volume: 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.2f);

        if (canCloseEarly)
        {
            if (_processingDialogue != null) StopCoroutine(_processingDialogue);
            OnDialogueEnd();
        }
        else
        {
            typewriter.SkipTypewriter();
            OnTextShowed();
        }
    }

    private IEnumerator ProcessingDialogue()
    {
        while (!IsTextShowed)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(10f);
        OnDialogueEnd();
    }

    private void OnDialogueEnd()
    {
        if (_skipButtonAnimation != null) StopCoroutine(_skipButtonAnimation);
        if (_processingDialogue != null) StopCoroutine(_processingDialogue);
        _playerInput.Player.SkipDialogue.performed -= OnSkip;
        
        IsTextShowed = false;
        canSkip = false;
        canCloseEarly = false;
        hasTextStarted = false;
        IsOngoing = false;
        
        if (_dialogueQueue.Count > 0)
        {
            IsContinuous = true;
            SetDialogue(_dialogueQueue.Dequeue());
            return;
        }
        
        IsContinuous = false;
        
        if (_boxCollapse != null) StopCoroutine(_boxCollapse);
        _boxCollapse = StartCoroutine(DialogueBoxCollapse());
    }
}
