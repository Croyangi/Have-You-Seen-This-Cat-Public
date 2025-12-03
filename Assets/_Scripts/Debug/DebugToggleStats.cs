using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugToggleStats : MonoBehaviour
{
    [SerializeField] private GameObject ui;
    [SerializeField] private bool isToggled;
    
    private void OnEnable()
    {
        //Id = "show_stats";
        //Description = "Toggles stats view.";
        //Format = "show_stats";
    }
    
    private void OnToggle()
    {
        isToggled = !isToggled;
        ui.SetActive(isToggled);
    }
}
