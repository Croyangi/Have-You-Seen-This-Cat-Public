using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCrosshairHelper : MonoBehaviour
{
    [Header("General References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Image crosshairImage;
    [SerializeField] private RectTransform crosshairRect;

    [Header("State References")]
    public string BaseLayer { get; private set; }  = "Base";

    [field: SerializeField] public string CurrentBaseLayerState { get; private set; }

    public string Default { get; private set; } = "CrosshairCat";
    public string Interact { get; private set; } = "CrosshairPointer";
    public string Inspect { get; private set; } = "CrosshairEye";
    public string Cross { get; private set; } = "CrosshairX";
    public string Grab { get; private set; } = "CrosshairGrab";
    
    [SerializeField] private Color crossColor;
    [SerializeField] private Color defaultColor;
    

    private void Awake()
    {
        var runtimeController = animator.runtimeAnimatorController;
        var newController = Instantiate(runtimeController);
        animator.runtimeAnimatorController = newController;
        PlayAnimation(Default, BaseLayer);
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += GameStart;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= GameStart;
    }

    private void GameStart()
    {
        PlayAnimation(Default, BaseLayer);
    }

    public void PlayAnimation(string state, string layer, float transitionTime = 0, bool force = false)
    {
        int index = animator.GetLayerIndex(layer);

        if (!animator.isActiveAndEnabled) return;

        if (state.Equals(CurrentBaseLayerState) && !force)
        {
            return;
        }

        if (state.Equals(Cross))
        {
            crosshairImage.color = crossColor;
        }
        else
        {
            crosshairImage.color = defaultColor;
        }
        
        if (transitionTime == 0)
        {
            animator.Play(state, index);
        }
        else
        {
            animator.CrossFade(state, transitionTime, index);
        }
        
        CurrentBaseLayerState = state;
        
        animator.Update(0f);
        ApplySpritePivotToUI();
    }

    private void ApplySpritePivotToUI()
    {
        Sprite sprite = crosshairImage.sprite;
        RectTransform rt = crosshairRect;
        
        Vector2 normalizedPivot = new Vector2(
            sprite.pivot.x / sprite.rect.width,
            sprite.pivot.y / sprite.rect.height
        );
        rt.pivot = normalizedPivot;
        
        rt.anchoredPosition = Vector2.zero;
    }
}
