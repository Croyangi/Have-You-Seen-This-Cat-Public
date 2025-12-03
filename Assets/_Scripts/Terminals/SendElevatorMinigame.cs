using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SendElevatorMinigame : MonoBehaviour, ITerminalMinigame
{
    private PlayerInput _playerInput = null;
    [SerializeField] private TerminalMinigameBase terminalMinigameBase;

    [Header("Minigame")] 
    [SerializeField] private float confirmStayTime;
    [SerializeField] private List<string> floorNames;
    [SerializeField] private List<string> currentFloorNames;
    [SerializeField] private int floorsAmount;
    [SerializeField] private int index;
    [SerializeField] private int correctFloorIndex;
    [SerializeField] private TextMeshProShapes currentFloorText;
    [SerializeField] private TextMeshProShapes correctFloorText;
    
    [SerializeField] private GameObject upActiveArrow;
    [SerializeField] private GameObject downActiveArrow;
    
    [SerializeField] private AudioClip modemBeep;
    [SerializeField] private GameObject computerClickSFXObject;
    [SerializeField] private AudioClip computerClick;

    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color activeColor;

    private void Awake()
    {
        _playerInput = new PlayerInput();

        currentFloorNames = GetCurrentFloorNames();
        
        index = 0;
        currentFloorText.text = currentFloorNames[index];
        
        correctFloorIndex = Random.Range(1, currentFloorNames.Count);
        correctFloorText.text = currentFloorNames[correctFloorIndex];

        StartCoroutine(AnimateArrows());
    }

    private List<string> GetCurrentFloorNames()
    {
        List<string> names = new List<string>(floorNames);
        List<string> returningList = new List<string>();
        returningList.Add("current floor");
        for (int i = 0; i < floorsAmount && names.Count > 0; i++)
        {
            int fi = Random.Range(0, names.Count);
            returningList.Add(names[fi]);
            names.RemoveAt(fi);
        }
        
        return returningList;
    }
    
    private void OnActionPerformed(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();
        int newInput = (int) Mathf.Sign(input.y);
        
        index += newInput;
        index = (index % currentFloorNames.Count + currentFloorNames.Count) % currentFloorNames.Count;

        if (newInput > 0)
        {
            upActiveArrow.SetActive(true);
            downActiveArrow.SetActive(false);
        } else if (newInput < 0)
        {
            downActiveArrow.SetActive(true);
            upActiveArrow.SetActive(false);
        }
        
        currentFloorText.text = currentFloorNames[index];

        PlayInputSFX();

        if (_testFloorStandby != null) StopCoroutine(_testFloorStandby);
        if (index == correctFloorIndex)
        {
            ManagerSFX.Instance.PlaySFX(modemBeep, transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            _testFloorStandby = StartCoroutine(TestFloorStandby());
        }
    }

    private void OnActionCanceled(InputAction.CallbackContext value)
    {
        upActiveArrow.SetActive(false);
        downActiveArrow.SetActive(false);
    }

    [SerializeField] private GameObject arrows;
    private int _arrowsIndex;
    [SerializeField] private float arrowAnimationTime;
    [SerializeField] private List<Triangle> triangles;
    
    private IEnumerator AnimateArrows()
    {
        foreach (Transform child in arrows.transform)
        {
            triangles.Add(child.gameObject.GetComponent<Triangle>());
        }
        
        while (true)
        {
            int prevIndex = _arrowsIndex;
            _arrowsIndex++;
            _arrowsIndex = (_arrowsIndex % triangles.Count + triangles.Count) % triangles.Count;
            triangles[_arrowsIndex].Color = activeColor;
            triangles[prevIndex].Color = inactiveColor;
            yield return new WaitForSeconds(arrowAnimationTime);
        }
    }

    private void PlayInputSFX()
    {
        if (computerClickSFXObject != null)
        {
            Destroy(computerClickSFXObject);
        }
        computerClickSFXObject = ManagerSFX.Instance.PlaySFX(computerClick, transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f).gameObject;
    }

    private Coroutine _testFloorStandby;
    private IEnumerator TestFloorStandby()
    {
        yield return new WaitForSeconds(confirmStayTime);
        terminalMinigameBase.OnMinigameEnd();
    }

    private void SubscribeToInput()
    {
        _playerInput.Enable();
        _playerInput.TerminalMinigames.SendElevator.performed += OnActionPerformed;
        _playerInput.TerminalMinigames.SendElevator.canceled += OnActionCanceled;
    }
    
    private void UnsubscribeToInput()
    {
        _playerInput.TerminalMinigames.SendElevator.performed -= OnActionPerformed;
        _playerInput.TerminalMinigames.SendElevator.canceled -= OnActionCanceled;
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
