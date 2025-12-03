using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUI
{
    public bool IsVisible { get; set; }
    
    public void OnVisibilityChanged();
}