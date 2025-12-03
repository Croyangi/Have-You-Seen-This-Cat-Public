using Shapes;
using UnityEngine;

public class CrosshairUI : MonoBehaviour, IUI
{
    [SerializeField] private GameObject ui;
    [SerializeField] private PlayerCrosshairHelper playerCrosshairHelper;
    
    public bool IsVisible { get; set; }

    public void OnVisibilityChanged()
    {
        ui.SetActive(IsVisible);
        
        if (IsVisible)
        {
            playerCrosshairHelper.PlayAnimation(playerCrosshairHelper.CurrentBaseLayerState, playerCrosshairHelper.BaseLayer, force: true);
        }
    }
}