using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class GeneralUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private GameObject button;
    [SerializeField] private bool isMultipleObjects;

    [SerializeField] private Color unpressedColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f);

    [SerializeField] private Vector3 unpressedScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 pressedScale = new Vector3(0.95f, 0.95f, 0.95f);

    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private Color unhoveredColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color hoveredColor = new Color(1f, 1f, 1f);
    [SerializeField] private float scaleSpeed = 0.1f;

    [SerializeField] private AudioClip generalUIHover;

    private void Awake()
    {
        button.transform.DOScale(unpressedScale, scaleSpeed).SetUpdate(true);
        ChangeButtonColor(unhoveredColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ChangeButtonColor(pressedColor);
        button.transform.DOScale(pressedScale, scaleSpeed).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ChangeButtonColor(unpressedColor);
        button.transform.DOScale(unpressedScale, scaleSpeed).SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (generalUIHover != null && ManagerSFX.Instance != null) 
        {
            ManagerSFX.Instance.PlayRawSFX(generalUIHover, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, 0.1f);
        }
        button.transform.DOScale(hoverScale, scaleSpeed).SetUpdate(true);
        ChangeButtonColor(hoveredColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        button.transform.DOScale(unpressedScale, scaleSpeed).SetUpdate(true);
        ChangeButtonColor(unhoveredColor);
    }

    private void ChangeButtonColor(Color color)
    {
        if (isMultipleObjects)
        {
            Image[] images = button.GetComponentsInChildren<Image>();
            foreach (Image image in images)
            {
                image.color = color;
            }
        }
        else
        {
            button.GetComponent<Image>().color = color;
        }
    }
}
