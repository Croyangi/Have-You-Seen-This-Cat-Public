using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [field: SerializeField] public bool IsInteractable { get; set; }
    [field: SerializeField] public bool IsInspectable { get; set; }
    [field: SerializeField] public bool IsBeingInteracted { get; set; }
    [field: SerializeField] public bool IsFreeMoveInspectable { get; set; }
    [field: SerializeField] public bool IsOnInteractOutlineCulled { get; set; }
    
    [field: SerializeField] public ObjFlavorText ObjFlavorText { get; private set; }
    [field: SerializeField] public string HoverText { get; set; }
    [field: SerializeField] public string InspectText { get; set; }

    protected MaterialPropertyBlock mpb;
    [SerializeField] protected Color originalOutlineColor;
    [SerializeField] protected Color selectedOutlineColor = Color.white;
    
    public abstract void OnInteract();
    public abstract void ShowOutline();
    public abstract void HideOutline();

    protected virtual void Awake()
    {
        mpb = new MaterialPropertyBlock();
        if (ObjFlavorText != null)
        {
            HoverText = ObjFlavorText.hoverText;
            InspectText = ObjFlavorText.inspectText;
        }
    }
    
    protected virtual void Start()
    {
        HideOutline();
    }
}
