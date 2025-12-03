using System;
using UnityEngine;

public class RetrieveCatsGoal : Goal
{
    private void OnEnable()
    {
        CatCollectionTerminal.OnCatSuccessfulCollection += OnCatSuccessfulCollection;
        CatCollectionTerminal.OnQuotaReached += OnComplete;
        
        SetText();
    }

    private void OnDisable()
    {
        CatCollectionTerminal.OnCatSuccessfulCollection -= OnCatSuccessfulCollection;
        CatCollectionTerminal.OnQuotaReached -= OnComplete;
    }

    private void OnCatSuccessfulCollection()
    {
        SetText();
        OnPing?.Invoke(this);
    }

    public override void OnShow()
    {
        base.OnShow();
        SetText();
    }

    private void SetText()
    {
        string text = goalText;
        text += " - " + CatCollectionTerminal.CatCount + "/" + ManagerGame.Instance.Difficulty.collectionGoal;
        OnUpdateText?.Invoke(this, text);
    }
}
