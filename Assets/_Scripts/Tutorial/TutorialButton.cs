using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialButton: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private AudioClip generalUIHoverSFX;
    private GameObject _generalUIHoverSFXObj;
    [SerializeField] private Shapes.Rectangle doneOutline;
    [SerializeField] private TextMeshProUGUI doneText;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color unhoverColor;

    private void OnEnable()
    {
        OnPointerExit(null);
    }

    private void Awake()
    {
        OnPointerExit(null);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_generalUIHoverSFXObj != null) Destroy(_generalUIHoverSFXObj);
        _generalUIHoverSFXObj = ManagerSFX.Instance.PlayRawSFX(generalUIHoverSFX, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        doneOutline.Color = unhoverColor;
        doneText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        doneOutline.Color = hoverColor;
        doneText.color = unhoverColor;
    }
}
