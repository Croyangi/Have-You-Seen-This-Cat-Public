using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerGoal : Goal
{
    private ManagerGameplay _managerGameplay;
    private Stack<float> _timerPings = new Stack<float>();
    
    private void OnEnable()
    {
        _managerGameplay = ManagerGameplay.Instance;
        float timer = ManagerGame.Instance.Difficulty.timeLimit;
        
        _timerPings.Clear();
        _timerPings.Push(timer * 0.1f);
        _timerPings.Push(timer * 0.25f);
        _timerPings.Push(timer * 0.50f);
        _timerPings.Push(timer * 0.75f);
        
        SetText();
        
        CatCollectionTerminal.OnQuotaReached += OnComplete;
        ManagerGameplay.OnTimerEnd += Fail;
    }

    private void OnDisable()
    {
        CatCollectionTerminal.OnQuotaReached -= OnComplete;
        ManagerGameplay.OnTimerEnd -= Fail;
    }

    private void Fail()
    {
        OnFail?.Invoke(this);
        Destroy(gameObject);
    }

    public override void OnShow()
    {
        base.OnShow();
        StartCoroutine(TextUpdateTick());
    }

    private IEnumerator TextUpdateTick()
    {
        while (IsVisible)
        {
            SetText();
            yield return null;
        }
    }

    private void SetText()
    {
        float timer = _managerGameplay.Timer;
        int minutes = Mathf.FloorToInt(timer / 60f);
        float seconds = timer % 60f;
        
        string formattedTime = $"{minutes:00}:{seconds:00.00}";
        string text = $"{goalText} - {formattedTime}";
        
        OnUpdateText?.Invoke(this, text);
    }

    private void FixedUpdate()
    {
        while (_timerPings.Count > 0 && _managerGameplay.Timer <= _timerPings.Peek())
        {
            _timerPings.Pop();
            OnPing?.Invoke(this);
        }
        
        if (_timerPings.Count == 0 && _managerGameplay.Timer < 60)
        {
            OnPing?.Invoke(this);
        }
    }
}
