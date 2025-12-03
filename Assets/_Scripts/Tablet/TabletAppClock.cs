using System.Collections;
using TMPro;
using UnityEngine;

public class TabletAppClock : MonoBehaviour, ITabletApp
{
    [SerializeField] private GameObject cameraBlock;
    [field: SerializeField] public bool IsShowing { get; private set; }
    [SerializeField] private ManagerGame managerGame;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private float clockSpeedMin;
    [SerializeField] private float clockSpeedMax;
    [SerializeField] private float clockSpeed;
    
    [SerializeField] private GameObject countdownSFXObj;
    [SerializeField] private AudioClip countdownSFX;
    
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        
        clockSpeed = Random.Range(clockSpeedMin, clockSpeedMax);
    }

    private void Start()
    {
        managerGame = ManagerGame.Instance;
    }

    public Vector3 GetCameraPos()
    {
        return cameraBlock.transform.localPosition;
    }
    
    public void OnShow()
    {
        IsShowing = true;
        StartCoroutine(ToggledUpdate());
        
        Vector3 pos = ManagerPlayer.instance.PlayerTabletHelper.Tablet.transform.position;
        countdownSFXObj = ManagerSFX.Instance.PlaySFX(countdownSFX, pos, 0.1f, true, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f).gameObject;
    }
    
    public void OnHide()
    {
        IsShowing = false;
        if (countdownSFXObj != null) Destroy(countdownSFXObj);
    }

    private void OnEnable()
    {
        _playerInput.Enable();
    }
    
    private void OnDisable()
    {
        _playerInput.Disable();
    }

    private IEnumerator ToggledUpdate()
    {
        while (IsShowing)
        {
            //timeText.text = ((managerGame.Timer * clockSpeed).ToString("F0").PadLeft(5, 'X'));
            yield return null;
        }
    }
}
