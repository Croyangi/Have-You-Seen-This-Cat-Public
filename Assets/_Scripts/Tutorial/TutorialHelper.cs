using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TutorialHelper : MonoBehaviour, IDataPersistence
{
    [TextArea]
    [SerializeField] private string loginText;
    [SerializeField] private TextMeshProShapes loginTextMesh;
    
    [SerializeField] private GameObject logo;
    [SerializeField] private Transform[] screenTransforms;

    private GameObject _bootUpSFXObject;
    [SerializeField] private AudioClip bootUp;
    [SerializeField] private AudioClip completeBootUp;
    
    [SerializeField] private AudioClip logoReveal;
    [SerializeField] private AudioClip modemBeep;
    [SerializeField] private AudioClip complete;
    
    [SerializeField] private AudioClip lightHumAmbienceSFX;
    [SerializeField] private AudioClip computerHum;
    private GameObject _computerHumSFXObj;
    [SerializeField] private string currentName;
    [SerializeField] private AudioClip[] keyboardTypeSFXs;
    [SerializeField] private AudioClip keyboardBackspaceSFX;
    [SerializeField] private AudioClip errorSFX;
    [SerializeField] private AudioClip tvTurnOffSFX;
    [SerializeField] private AudioClip revealSFX;
    
    [SerializeField] private GameObject mainCam;

    
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
        zoomOutCam.GetComponent<CinemachineVirtualCamera>().m_Lens = zoomOutCamLensSettings;
        
        SetAllowedChars();
        foreach (TextMeshProUGUI text in currentCharsTextMesh)
        {
            text.text = "";
        }
    }

    private IEnumerator Start()
    {
        ManagerGame.Instance.InitializeGame();
        StartCoroutine(Initialize());
        yield return null;
        ManagerGame.Instance.StartGame();
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= OnKeyboardPerformed;
        _playerInput.Disable();
    }
    
    public void LoadData(GameData data)
    {
        _hasCompleted = data.hasCompletedTutorial;
    }

    private bool _hasCompleted;
    public void SaveData(ref GameData data)
    {
        data.player.name = currentName;
        data.hasCompletedTutorial = _hasCompleted;
    }
    
    private List<char> _allowedEyes = new List<char> { 'm', 'e', 'o', 'w', 'c', 'a', 't'};
    
    private List<char> _allowedMouths = new List<char> { 'm', 'e', 'o', 'w', 'c', 'a', 't'};
    private List<char> _allowedChars = new List<char>();
    [SerializeField] private List<TextMeshProUGUI> currentCharsTextMesh = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI availableCharsTextMesh;
    private void OnKeyboardPerformed(char input)
    {
        // Backspace is ASCII 8 ('\b')
        if (input == '\b' && currentName.Length > 0)
        {
            ManagerSFX.Instance.PlayRawSFX(keyboardBackspaceSFX, 0.2f, false, ManagerAudioMixer.Instance.AMGSFX, 0.1f);
            currentCharsTextMesh[currentName.Length - 1].text = "";
            currentName = currentName.Substring(0, currentName.Length - 1);
            SetAllowedChars();
            return;
        }
        
        input = char.ToLower(input);
        
        if (_allowedChars.Contains(input) && currentName.Length < 3)
        {
            string processedInput = input.ToString().ToUpper();
            currentName += processedInput;
            currentCharsTextMesh[currentName.Length - 1].text = processedInput;
            ManagerSFX.Instance.PlayRawSFX(keyboardTypeSFXs[Random.Range(0, 1)], 0.2f, false, ManagerAudioMixer.Instance.AMGSFX, 0.1f);
        }
        else
        {
            ManagerSFX.Instance.PlayRawSFX(errorSFX, 0.2f, false, ManagerAudioMixer.Instance.AMGSFX);
        }
        
        SetAllowedChars();
    }

    private void SetAllowedChars()
    {
        _allowedChars = (currentName.Length == 0 || currentName.Length == 2) ? _allowedEyes : _allowedMouths;
        availableCharsTextMesh.text = new string(_allowedChars.ToArray()).ToUpper();
    }

    private int _screenIndex = 0;
    private IEnumerator SetNextScreen(bool isBlank = false)
    {
        if (isBlank)
        {
            ManagerSFX.Instance.PlayRawSFX(modemBeep, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.8f, false);
            tutorialCam.transform.localPosition = Vector3.right * screenTransforms[0].localPosition.x;
            yield break;
        }
        
        ManagerSFX.Instance.PlayRawSFX(modemBeep, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.8f, false);
        tutorialCam.transform.localPosition = Vector3.right * screenTransforms[0].localPosition.x;
        yield return new WaitForSeconds(1f);
        ManagerSFX.Instance.PlayRawSFX(modemBeep, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 1.1f, false);
        
        _screenIndex++;
        tutorialCam.transform.localPosition = Vector3.right * screenTransforms[_screenIndex].localPosition.x;
    }

    [SerializeField] private Transform startingLookAt;
    private IEnumerator Initialize()
    {
        ManagerPlayer mp = ManagerPlayer.instance;
        mp.PlayerHead.transform.LookAt(startingLookAt.position);
        mp.PlayerFlashlightHelper.HasObtained = false;
        mp.PlayerFlashlightHelper.ProcessObtainCheck();
        mp.PlayerTabletHelper.HasObtained = false;
        mp.PlayerTabletHelper.ProcessObtainCheck();
        mp.PlayerCatHelper.HasObtainedRollCall = false;
        mp.PlayerCatHelper.ProcessObtainCheck();
        
        StartCoroutine(SetNextScreen());
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        
        mainCam.SetActive(false);
        tutorialCam.SetActive(true);
        zoomOutCam.SetActive(false);
        tutorialVolume.gameObject.SetActive(false);
        tutorialEndVolume.gameObject.SetActive(false);
        
        mp.PlayerInputHelper.SetProcessing(false, mp.PlayerInputHelper.All, "tutorial");
        PlayerUIHelper puh = ManagerPlayer.instance.PlayerUIHelper;
        puh.SetVisibility(false, puh.All, "tutorial");
        
        logo.SetActive(false);
        loginTextMesh.gameObject.SetActive(false);
        
        _computerHumSFXObj = ManagerSFX.Instance.PlayRawSFX(computerHum, 0.01f, true, ManagerAudioMixer.Instance.AMGSFX);
        loginTextMesh.SetText("");
        
        yield return new WaitForSeconds(2f);
        
        loginTextMesh.gameObject.SetActive(true);
        
        GameObject sfx = ManagerSFX.Instance.PlayRawSFX(bootUp, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX);
        _bootUpSFXObject = sfx;
        StartCoroutine(PrintBootLines(loginText, loginTextMesh, 8f));
    }

    private void OnNamingShow()
    {
        StartCoroutine(SetNextScreen());
        
        Keyboard.current.onTextInput += OnKeyboardPerformed;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void OnDonePressed()
    {
        if (currentName.Length <= 2)
        {
            ManagerSFX.Instance.PlayRawSFX(errorSFX, 0.2f, false, ManagerAudioMixer.Instance.AMGSFX);
            return;
        }
        
        Keyboard.current.onTextInput -= OnKeyboardPerformed;
        
        ManagerDataPersistence.Instance.GetGameData().player.name = currentName;
        ManagerDataPersistence.Instance.GetGameData().hasCompletedTutorial = _hasCompleted;
        ManagerDataPersistence.Instance.SaveGame();
        
        StartCoroutine(SetNextScreen());
        OnWaiverShow();
    }

    [SerializeField] private TextMeshProUGUI waiverNameTextMesh;
    private void OnWaiverShow()
    {
        waiverNameTextMesh.text = currentName;
    }

    public void OnAgreePressed()
    {
        StartCoroutine(SetNextScreen(true));
        StartCoroutine(OnExitTerminal());
    }
    
    public void OnContinueTutorial()
    {
        StartCoroutine(SetNextScreen());
    }

    [ContextMenu("On Tutorial Done")]

    public void CompleteTutorial()
    {
        StartCoroutine(HandleCompleteTutorial());
    }
    private IEnumerator HandleCompleteTutorial()
    {
        ManagerPlayer mp = ManagerPlayer.instance;
        mp.PlayerInputHelper.SetProcessing(false, mp.PlayerInputHelper.All, "tutorial");
        PlayerUIHelper puh = ManagerPlayer.instance.PlayerUIHelper;
        puh.SetVisibility(false, puh.All, "tutorial");
        
        tutorialCam.transform.localPosition = Vector3.right * screenTransforms[0].localPosition.x;
        ManagerSFX.Instance.PlayRawSFX(tvTurnOffSFX, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX);
        if (_computerHumSFXObj != null) Destroy(_computerHumSFXObj);
        ManagerSFX.Instance.StopAmbienceSFX();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        mainCam.SetActive(false);
        tutorialCam.SetActive(true);
        tutorialEndVolume.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        _hasCompleted = true;
        ManagerDataPersistence.Instance.SaveGame();
        yield return new WaitForSeconds(1f);
        SceneLoader.Load(SceneID.WaitingRoom);
    }

    [SerializeField] private GameObject tutorialCam;
    [SerializeField] private GameObject zoomOutCam;
    [SerializeField] private LensSettings zoomOutCamLensSettings = LensSettings.Default;
    [SerializeField] private Volume tutorialVolume;
    [SerializeField] private Volume tutorialEndVolume;
    [SerializeField] private TutorialWalkthrough tutorialWalkthrough;
    private IEnumerator OnExitTerminal()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        yield return new WaitForSeconds(1f);
        ManagerSFX.Instance.PlayRawSFX(tvTurnOffSFX, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX);
        if (_computerHumSFXObj != null) Destroy(_computerHumSFXObj);
        yield return new WaitForSeconds(4f);
        AudioSource lightHumAudioSource = ManagerSFX.Instance.PlayAmbienceSFX(lightHumAmbienceSFX, 0f);
        float volumeDuration = 2f;
        DOVirtual.Float(0f, 0.05f, volumeDuration, value =>
        {
            lightHumAudioSource.volume = value;
        });
        
        tutorialVolume.weight = 1f;
        tutorialVolume.priority = 999999f;
        
        mainCam.SetActive(true);
        tutorialCam.SetActive(false);
        zoomOutCam.SetActive(true);
        tutorialVolume.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(2f);
        float weightDuration = 2f;
        DOVirtual.Float(1f, 0f, weightDuration, value =>
        {
            tutorialVolume.weight = value;
        });
        

        float duration = 6f;
        
        zoomOutCam.transform.DOMove(ManagerPlayer.instance.PlayerHead.transform.position, duration).SetEase(Ease.InOutCubic);
        yield return new WaitForSeconds(duration);
        
        tutorialWalkthrough.StartTutorial();
        
        yield return new WaitForFixedUpdate();
        
        ManagerPlayer mp = ManagerPlayer.instance;
        mp.PlayerInputHelper.SetProcessing(true, mp.PlayerInputHelper.All, "tutorial");
        PlayerUIHelper puh = ManagerPlayer.instance.PlayerUIHelper;
        puh.SetVisibility(true, puh.All, "tutorial");
        
        zoomOutCam.SetActive(false);
        tutorialVolume.gameObject.SetActive(false);
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

    private readonly List<string> _currentDisplayText = new List<string>();
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
        if (_bootUpSFXObject != null) Destroy(_bootUpSFXObject);
        ManagerSFX.Instance.PlayRawSFX(modemBeep, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        yield return new WaitForSeconds(2.5f);
        ManagerSFX.Instance.PlayRawSFX(logoReveal, 0.4f, false, ManagerAudioMixer.Instance.AMGSFX);
        logo.SetActive(true);
        
        yield return new WaitForSeconds(4f);
        logo.SetActive(false);
        OnNamingShow();
    }
}
