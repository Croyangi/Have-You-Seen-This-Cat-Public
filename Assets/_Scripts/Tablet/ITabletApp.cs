using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITabletApp
{
    public void OnShow();
    public void OnHide();
    public Vector3 GetCameraPos();
}
