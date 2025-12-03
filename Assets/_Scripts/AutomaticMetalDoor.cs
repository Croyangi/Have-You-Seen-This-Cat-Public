using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class AutomaticMetalDoor : MonoBehaviour
{
    [field: SerializeField] public bool IsLocked { get; private set; }
    [field: SerializeField] public MetalDoor MetalDoor { get; private set; }

    [ContextMenu("Lock")]
    public void OnLock()
    {
        if (IsLocked) return;
        MetalDoor.Close();
        IsLocked = true;
    }
}
