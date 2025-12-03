using UnityEngine;

public class VisibilityEyesUI : MonoBehaviour, IUI
{
    [SerializeField] private GameObject ui;
    [SerializeField] private PlayerVisibilityHelper visibilityHelper;
    
    [field: SerializeField] public bool IsVisible { get; set; }

    public void OnVisibilityChanged()
    {
        ui.SetActive(IsVisible);
        
        if (IsVisible)
        {
            visibilityHelper.PlayAnimation(visibilityHelper.CurrentState, force: true);
        }
    }
}
