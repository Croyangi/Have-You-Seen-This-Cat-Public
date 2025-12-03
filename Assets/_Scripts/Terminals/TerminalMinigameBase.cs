using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class TerminalMinigameBase : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string loginText;
    [SerializeField] private TextMeshProShapes loginTextMesh;
    [SerializeField] private GameObject baseMinigameTemplate;
    [SerializeField] private float maxLoginTime;
    [SerializeField] private TextMeshProShapes goalText;
    [SerializeField] private TextMeshProShapes progressText;
    [SerializeField] private GameObject transitions;
    [SerializeField] private float transitionDuration;
    [SerializeField] private Line transitionBar;

    public bool isFocused;
    [SerializeField] private int progress;
    [SerializeField] private List<TerminalMinigame> terminalMinigames = new List<TerminalMinigame>();
    [SerializeField] private TerminalMinigame loginMinigame;

    private Terminal _terminal;
    private ITerminal _iTerminal;
    private ITerminalMinigame _currentITerminalMinigame;
    [field: SerializeField] public TerminalMinigame CurrentMinigame { get; private set; }
    [field: SerializeField] public GameObject CurrentMinigameObj { get; private set; }

    private readonly List<string> _currentDisplayText = new List<string>();

    [SerializeField] private GameObject bootUpSFXObject;
    [SerializeField] private AudioClip bootUp;
    [SerializeField] private AudioClip completeBootUp;

    [SerializeField] private GameObject transitionSFXObject;
    [SerializeField] private AudioClip transition;
    [SerializeField] private AudioClip modemBeep;
    [SerializeField] private AudioClip complete;
    
    private PlayerInput _playerInput;

    public Action<TerminalMinigame> OnTerminalMinigameEnd;
    public Action<ITerminalMinigameResult> OnTerminalMinigameGetResult;
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void OnDisable()
    {
        _playerInput.Player.BackOut.performed -= OnBackOutPerformed;
        _playerInput.Disable();
    }

    public void Initialize(Terminal terminal, ITerminal iTerminal, Vector3 position, Quaternion rotation)
    {
        _terminal = terminal;
        _iTerminal = iTerminal;
        transform.localPosition = position;
        transform.localRotation = rotation;
    }
    
    private void OnBackOutPerformed(InputAction.CallbackContext value)
    {
        _terminal.TerminalLockedInteraction();
    }

    public void OnMinigameEnd()
    {
        ITerminalMinigameResult result = null;
        if (CurrentMinigameObj.TryGetComponent<ITerminalMinigame>(out var iTerminalMinigame))
        {
            iTerminalMinigame.OnMinigameEnd();
            result = iTerminalMinigame.GetResult();
        }
        Destroy(CurrentMinigameObj);
        OnTerminalMinigameEnd?.Invoke(CurrentMinigame);
        if (result != null) OnTerminalMinigameGetResult?.Invoke(result);
        CurrentMinigame = null;
        OnTransitionStart();
    }
    
    public void OnMinigameSkip()
    {
        if (CurrentMinigameObj.TryGetComponent<ITerminalMinigame>(out var iTerminalMinigame))
        {
            iTerminalMinigame.OnMinigameEnd();
        }
        Destroy(CurrentMinigameObj);
        CurrentMinigame = null;
        OnTransitionStart();
    }
    
    private void OnTransitionStart()
    {
        transitions.SetActive(true);
        baseMinigameTemplate.SetActive(false);
        transitionBar.End = Vector3.zero;
        
        SFXObject sfx = ManagerSFX.Instance.PlaySFX(transition, transform.position, 0.2f, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        transitionSFXObject = sfx.gameObject;
        
        StartCoroutine(TransitionTimer());
    }

    private IEnumerator TransitionTimer()
    {
        float elapsedTime = 0f;
        float maxEndLength = 0.47f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float length = Mathf.Min(maxEndLength * (elapsedTime / transitionDuration), maxEndLength);
            transitionBar.End = new Vector3(length, transitionBar.End.y, transitionBar.End.z);

            yield return new WaitForFixedUpdate();
        }

        OnTransitionEnd();
    }
    
    private void OnTransitionEnd()
    {
        if (transitionSFXObject != null)
        {
            Destroy(transitionSFXObject);
        }
        
        transitions.SetActive(false);
        baseMinigameTemplate.SetActive(true);
        OnMinigameNext();
    }

    private void OnMinigameNext()
    {
        if (progress >= terminalMinigames.Count)
        {
            ManagerSFX.Instance.PlaySFX(complete, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            
            _terminal.TerminalUnlock();
            Destroy(gameObject); /////////////////////////////////////////////////////////
            return;
        }
        
        ManagerSFX.Instance.PlaySFX(modemBeep, transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        CurrentMinigameObj = Instantiate(terminalMinigames[progress].terminalMinigame, transform);
        CurrentMinigame = terminalMinigames[progress];
        _currentITerminalMinigame = null;
        if (CurrentMinigameObj.TryGetComponent<ITerminalMinigame>(out var iTerminalMinigame))
        {
            _currentITerminalMinigame = iTerminalMinigame;
            _currentITerminalMinigame.OnMinigameStart(gameObject.GetComponent<TerminalMinigameBase>());
            if (isFocused) _currentITerminalMinigame.OnMinigameFocus();
        }
        
        progressText.text = progress + 1 + "/" + terminalMinigames.Count;
        goalText.text = terminalMinigames[progress].goalText.ToUpper();
        progress++;
    }

    public void ProcessFocus()
    {
        isFocused = !isFocused;
        
        if (_currentITerminalMinigame == null) return;
        if (isFocused) 
        {
            _currentITerminalMinigame.OnMinigameFocus();
            _playerInput.Player.BackOut.performed += OnBackOutPerformed;
            _playerInput.Enable();
        }
        else
        {
            _currentITerminalMinigame.OnMinigameUnfocus();
            _playerInput.Player.BackOut.performed -= OnBackOutPerformed;
            _playerInput.Disable();
        }
    }

    public void SetMinigames(List<TerminalMinigame> minigames)
    {
        terminalMinigames.Clear();
        terminalMinigames.Add(loginMinigame);
        terminalMinigames.AddRange(minigames);
    }

    private void Start()
    {
        loginTextMesh.gameObject.SetActive(true);
        baseMinigameTemplate.SetActive(false);
        
        SFXObject sfx = ManagerSFX.Instance.PlaySFX(bootUp, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        bootUpSFXObject = sfx.gameObject;
        
        StartCoroutine(PrintBootLines(loginText, loginTextMesh, maxLoginTime));
    }
    
    
    private IEnumerator PrintBootLines(string bootText, TextMeshProShapes displayText, float remainingTime)
    {
        displayText.SetText("");
        string[] allLines = bootText.Split('\n');
        int currentLineIndex = 0;

        // Define your printing stages (linesToPrint, waitBeforeStart)
        var printStages = new List<(int linesToPrint, float waitBefore)>
        {
            (2, 0.1f),
            (11, 0.3f),
            (23, 0.1f),
            (28, 0.1f),
            (44, 0.1f),
            (48, 0.1f),
            (50, 0.1f),
            (999, 0.1f)
        };

        foreach (var (linesToPrint, waitBefore) in printStages)
        {
            yield return new WaitForSeconds(waitBefore);
            int linesToAdd = Mathf.Min(linesToPrint, allLines.Length - currentLineIndex);
            for (int i = 0; i < linesToAdd; i++)
            {
                AppendLine(displayText, allLines[currentLineIndex++]);

                if (remainingTime > 0)
                {
                    float delay = Random.Range(0f, 0.05f);
                    remainingTime = Mathf.Max(0, remainingTime - delay);
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        
        StartCoroutine(FinishSequence());
    }

    private void AppendLine(TextMeshProShapes displayText, string line)
    {
        _currentDisplayText.Add(line);
        displayText.text += line + "\n";

        if (displayText.isTextOverflowing)
        {
            _currentDisplayText.RemoveAt(0);
            displayText.text = string.Join("\n", _currentDisplayText) + "\n";
        }
    }

    private IEnumerator FinishSequence()
    {
        loginTextMesh.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        
        if (bootUpSFXObject != null)
        {
            Destroy(bootUpSFXObject);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        ManagerSFX.Instance.PlaySFX(completeBootUp, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        baseMinigameTemplate.SetActive(true);
        OnMinigameNext();
        _terminal.TerminalBootEnd();
    }
}
