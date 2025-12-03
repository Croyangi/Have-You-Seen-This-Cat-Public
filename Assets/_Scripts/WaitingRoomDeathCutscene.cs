using System.Collections;
using System.IO;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class WaitingRoomDeathCutscene : MonoBehaviour, IDataPersistence
{
    [Header("Death Screen")]
    [SerializeField] private GameObject deathCam;
    [SerializeField] private GameObject textGroup;
    [SerializeField] private GameObject displayText;
    [SerializeField] private GameObject displayDeathImage;

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform lookAtPoint;

    [SerializeField] private AudioClip lightHumAmbienceSFX;
    [SerializeField] private AudioClip deathScreenBeepSFX;
    [SerializeField] private AudioClip deathScreenBuzzSFX;
    [SerializeField] private AudioClip deathScreenTurnOffSFX;
    private GameObject _deathScreenBuzzSFXObj;
    
    private void Awake()
    {
        deathCam.SetActive(false);
        _deathCamInitPos = deathCam.transform.localPosition;
        
        displayText.SetActive(true);
        displayDeathImage.SetActive(false);
    }
    
    private CinemachineBlendDefinition _oldBlend;
    [ContextMenu("Load Death Screen")]
    public void PlayerDeathWaitingRoomCutscene()
    {
        ManagerSFX.Instance.StopAmbienceSFX();
        AudioSource lightHumAudioSource = ManagerSFX.Instance.PlayAmbienceSFX(lightHumAmbienceSFX, 0f);
        float volumeDuration = 2f;
        DOVirtual.Float(0f, 0.05f, volumeDuration, value =>
        {
            lightHumAudioSource.volume = value;
        });
        
        ManagerPlayer mp = ManagerPlayer.instance;
        mp.PlayerObj.transform.position = respawnPoint.position;
        mp.PlayerObj.GetComponent<Rigidbody>().position = respawnPoint.position;
        
        mp.PlayerUIHelper.SetVisibility(false, mp.PlayerUIHelper.All, "death");
        
        displayText.SetActive(false);
        displayDeathImage.SetActive(true);
        
        _oldBlend = Camera.main.gameObject.GetComponent<CinemachineBrain>().m_DefaultBlend;
        
        CinemachineBlendDefinition cutBlend = new CinemachineBlendDefinition();
        cutBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        Camera.main.gameObject.GetComponent<CinemachineBrain>().m_DefaultBlend = cutBlend;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        
        deathCam.transform.localPosition = _deathCamInitPos;
        deathCam.SetActive(true);
        
        PlayerInputHelper pih = ManagerPlayer.instance.PlayerInputHelper;
        pih.SetProcessing(false, pih.All, "death");

        StartCoroutine(HandlePlayerDeathWaitingRoomCutscene());
    }
    
    private Vector3 _deathCamInitPos;
    private IEnumerator HandlePlayerDeathWaitingRoomCutscene()
    {
        _deathScreenBuzzSFXObj = ManagerSFX.Instance.PlaySFX(deathScreenBuzzSFX, lookAtPoint.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX).gameObject;
        
        StartCoroutine(FlickerText());
        
        yield return new WaitForSeconds(2f);
        
        Tween tween = deathCam.transform.DOLocalMoveZ(-2, 5f).SetEase(Ease.InOutCubic);
        
        yield return tween.WaitForCompletion();
        
        ManagerPlayer.instance.PlayerHead.transform.LookAt(lookAtPoint.position);
        Camera.main.gameObject.GetComponent<CinemachineBrain>().m_DefaultBlend = _oldBlend;
        deathCam.SetActive(false);
        
        ManagerPlayer mp = ManagerPlayer.instance;
        mp.PlayerUIHelper.SetVisibility(true, mp.PlayerUIHelper.All, "death");
        PlayerInputHelper pih = ManagerPlayer.instance.PlayerInputHelper;
        pih.SetProcessing(true, pih.All, "death");
        
        ManagerPlayer.instance.PlayerDialogueHelper.QueueDialogue(GetRandomStartingDialogue());
        
        yield return new WaitForSeconds(2f);
        Complete();
    }

    private void Complete()
    {
        displayText.SetActive(true);
        displayDeathImage.SetActive(false);
        if (_deathScreenBuzzSFXObj != null) Destroy(_deathScreenBuzzSFXObj);
        ManagerSFX.Instance.PlaySFX(deathScreenTurnOffSFX, lookAtPoint.position, 0.01f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
    }

    private IEnumerator FlickerText()
    {
        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(0.8f);
            textGroup.SetActive(!textGroup.activeSelf);
            if (textGroup.activeSelf) ManagerSFX.Instance.PlaySFX(deathScreenBeepSFX, lookAtPoint.position, 0.01f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        }
    }
    
    [SerializeField] private string textFileName;
    private string _nameIdentifier = "<NAME>";
    private string GetRandomStartingDialogue()
    {
        string path = Path.Combine(Application.streamingAssetsPath, textFileName);

        if (!File.Exists(path))
        {
            Debug.LogError("File not found at: " + path);
            return string.Empty;
        }

        string[] lines = File.ReadAllLines(path);

        if (lines.Length == 0) return string.Empty;

        string text = lines[Random.Range(0, lines.Length)];
        
        LoadData(ManagerDataPersistence.Instance.GetGameData());
        string formatted = _deaths.ToString("D3");
        
        text = text.Replace(_nameIdentifier, _playerName + "-" + formatted);
        
        return text;
    }
    
    private string _playerName;
    private int _deaths;
    public void LoadData(GameData data)
    {
        _playerName = data.player.name;
        _deaths = data.player.deaths;
    }

    public void SaveData(ref GameData data)
    {
    }
}
