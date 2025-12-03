using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] protected string goalText;
    [SerializeField] public Sprite goalIcon;
    
    public Action<Goal, string> OnUpdateText;
    public Action<Goal> OnCompleted;
    public Action<Goal> OnPing;
    public Action<Goal> OnFail;

    protected bool IsVisible;

    public virtual void OnShow()
    {
        IsVisible = true;
    }

    public virtual void OnHide()
    {
        IsVisible = false;
    }

    public virtual void OnComplete()
    {
        OnCompleted?.Invoke(this);
        Destroy(gameObject);
    }
}
