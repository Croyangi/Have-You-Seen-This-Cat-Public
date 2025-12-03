using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGoalHelper : MonoBehaviour
{
    [SerializeField] private PlayerUIHelper uiHelper;
    
    [SerializeField] private Image backingUI;
    private float _packingXShowWidth = 600f;
    private float _packingXHideWidth = 0f;
    private float _packingYExpandHeight = 100f;

    [SerializeField] private CanvasGroup canvasGroup;
    
    private Dictionary<Goal, GoalUI> _goalDictionary = new Dictionary<Goal, GoalUI>();
    [SerializeField] private GameObject goalPrefab;

    [SerializeField] private float defaultShowTime;
    private float _showTimer;

    private ManagerGame _managerGame;

    private const string SHOW_TWEEN_ID = "PlayerGoalHelper_ShowGroup";
    private const string HIDE_TWEEN_ID = "PlayerGoalHelper_HideGroup";

    private class GoalUI
    {
        public TextMeshProUGUI TextMesh;
        public Image Image;
    }

    // --- UPDATED ---
    private void Awake()
    {
        canvasGroup.DOFade(0, 0f).SetId(HIDE_TWEEN_ID);

        DOTween.To(
            () => backingUI.rectTransform.sizeDelta.x,
            x =>
            {
                var size = backingUI.rectTransform.sizeDelta;
                size.x = x;
                backingUI.rectTransform.sizeDelta = size;
            },
            _packingXHideWidth,
            0f
        ).SetId(HIDE_TWEEN_ID);
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= OnGameStart;
    }

    private void OnGameStart()
    {
        _managerGame = ManagerGame.Instance;
    }

    [ContextMenu("Ping Goal")]
    public void PingUI(Goal goal = null)
    {
        if (goal != null) goal.OnShow();            
        if (_showTimer < defaultShowTime) _showTimer = defaultShowTime;
        if (!_isUIVisibilityTick) StartCoroutine(UIVisibilityTick());
    }
    
    [ContextMenu("Ping Goal")]
    public void PingUI(float time)
    {
        _showTimer = time;
        if (!_isUIVisibilityTick) StartCoroutine(UIVisibilityTick());
    }

    private bool _isUIVisibilityTick;
    private IEnumerator UIVisibilityTick()
    {
        _isUIVisibilityTick = true;
        
        ShowUI();
        while (_showTimer > 0)
        {
            _showTimer -= Time.deltaTime;
            yield return null;
        }
        
        HideUI();
        _isUIVisibilityTick = false;
    }

    private void OnUpdateUI()
    {
        backingUI.rectTransform.sizeDelta = new Vector2(
            backingUI.rectTransform.sizeDelta.x, 
            _goalDictionary.Count * _packingYExpandHeight
        );
    }
    
    public void AddGoal(Goal goal)
    {
        GameObject gp = Instantiate(goalPrefab, canvasGroup.transform);
        
        GoalUI goalUI = new GoalUI();
        goalUI.Image = gp.GetComponentInChildren<Image>();
        goalUI.TextMesh = gp.transform.GetComponentInChildren<TextMeshProUGUI>();
        goalUI.Image.sprite = goal.goalIcon;
        
        _goalDictionary.Add(goal, goalUI);

        SubscribeToGoal(goal);
        
        OnUpdateUI();
        PingUI(goal);
    }

    private void OnUpdateGoalText(Goal goal, string text)
    {
        _goalDictionary[goal].TextMesh.text = text;
    }

    [SerializeField] private Color goalCompleteColor;
    private void OnGoalReached(Goal goal)
    {
        PingUI();
        _goalDictionary[goal].TextMesh.color = goalCompleteColor;
        _goalDictionary[goal].Image.color = goalCompleteColor;
        UnsubscribeFromGoal(goal);
    }
    
    [SerializeField] private Color goalFailColor;
    private void OnFail(Goal goal)
    {
        PingUI();
        _goalDictionary[goal].TextMesh.color = goalFailColor;
        _goalDictionary[goal].Image.color = goalFailColor;
        UnsubscribeFromGoal(goal);
    }
    
    [ContextMenu("Show goals")]
    public void ShowUI()
    {
        DOTween.Kill(SHOW_TWEEN_ID);
        DOTween.Kill(HIDE_TWEEN_ID);

        uiHelper.SetVisibility(false, new List<IUI>{uiHelper.CameraOverlayExtras}, "playerGoalHelper");
        
        foreach (var kvp in _goalDictionary)
            if (kvp.Key != null) kvp.Key.OnShow();
        
        float showDuration = 0.6f;
        DOTween.To(
            () => backingUI.rectTransform.sizeDelta.x,
            x =>
            {
                var size = backingUI.rectTransform.sizeDelta;
                size.x = x;
                backingUI.rectTransform.sizeDelta = size;
            },
            _packingXShowWidth,
            showDuration
        )
        .SetEase(Ease.OutExpo)
        .SetId(SHOW_TWEEN_ID);

        float fadeDelay = 0.1f;
        float fadeDuration = 0.5f;
        canvasGroup
            .DOFade(1, fadeDuration)
            .SetDelay(fadeDelay)
            .SetId(SHOW_TWEEN_ID);
    }
    
    [ContextMenu("Hide goals")]
    public void HideUI()
    {
        DOTween.Kill(SHOW_TWEEN_ID);
        DOTween.Kill(HIDE_TWEEN_ID);

        uiHelper.SetVisibility(true, new List<IUI>{uiHelper.CameraOverlayExtras}, "playerGoalHelper");
        
        foreach (var kvp in _goalDictionary)
            kvp.Key.OnHide();
        
        float fadeDuration = 0.15f;
        canvasGroup
            .DOFade(0, fadeDuration)
            .SetId(HIDE_TWEEN_ID);

        float hideDuration = 0.9f;
        float hideDelay = 0.1f;
        DOTween.To(
            () => backingUI.rectTransform.sizeDelta.x,
            x =>
            {
                var size = backingUI.rectTransform.sizeDelta;
                size.x = x;
                backingUI.rectTransform.sizeDelta = size;
            },
            _packingXHideWidth,
            hideDuration
        )
        .SetEase(Ease.OutExpo)
        .SetDelay(hideDelay)
        .SetId(HIDE_TWEEN_ID);
    }

    private void SubscribeToGoal(Goal goal)
    {
        goal.OnUpdateText += OnUpdateGoalText;
        goal.OnCompleted += OnGoalReached;
        goal.OnPing += PingUI;
        goal.OnFail += OnFail;
    }

    private void UnsubscribeFromGoal(Goal goal)
    {
        goal.OnUpdateText -= OnUpdateGoalText;
        goal.OnCompleted -= OnGoalReached;
        goal.OnPing -= PingUI;
        goal.OnFail -= OnFail;
    }
    private void OnDestroy()
    {
        foreach (var kvp in _goalDictionary)
        {
            var goal = kvp.Key;
            UnsubscribeFromGoal(goal);
        }

        DOTween.Kill(SHOW_TWEEN_ID);
        DOTween.Kill(HIDE_TWEEN_ID);
    }
}
