using System.Collections.Generic;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI textMesh;
    public string originalText;
    public TextAnimator_TMP textAnimator;
    public Image backing;
    public MainMenuButton mainMenuButton;
    public int id;
    
    [SerializeField] private MainMenuHelper mainMenuHelper;
    public bool isRestricted;


    private GameObject _parentObj;
    private void Awake()
    {
        _parentObj = gameObject;
        
        if (_parentObj.GetComponentInChildren<Image>() != null)
        {
            backing = _parentObj.GetComponentInChildren<Image>();
        }
                
        if (_parentObj.GetComponentInChildren<TextMeshProUGUI>() != null)
        {
            textMesh = _parentObj.GetComponentInChildren<TextMeshProUGUI>();
        }
            
        if (_parentObj.GetComponentInChildren<MainMenuButton>() != null)
        {
            mainMenuButton = _parentObj.GetComponentInChildren<MainMenuButton>();
        }
            
        if (_parentObj.transform.parent.GetComponentInChildren<TextAnimator_TMP>() != null)
        {
            textAnimator = _parentObj.GetComponentInChildren<TextAnimator_TMP>();
        }
        
        originalText = textMesh.text;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        mainMenuHelper.PressButton(id);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mainMenuHelper.PointerEnterButton(id);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mainMenuHelper.PointerExitButton(id);
    }
}
