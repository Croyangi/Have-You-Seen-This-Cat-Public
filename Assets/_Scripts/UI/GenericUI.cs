using UnityEngine;

public class GenericUI : MonoBehaviour, IUI
{
    [SerializeField] private GameObject ui;
    
    public bool IsVisible { get; set; }

    public void OnVisibilityChanged()
    {
        if (ui == null) return;
        ui.SetActive(IsVisible);
    }
}
