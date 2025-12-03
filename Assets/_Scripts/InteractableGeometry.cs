using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableGeometry : InteractableObject
{
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private int index = -1;
    
    public override void OnInteract()
    {
        onInteract.Invoke();
    }

    public override void ShowOutline()
    {
        foreach (Renderer r in renderers)
        {
            if (index == -1)
            {
                r.GetPropertyBlock(mpb);
            }
            else
            {
                r.GetPropertyBlock(mpb, index);
            }
            mpb.SetColor("_OutlineColor", selectedOutlineColor); // Set for that material only
            r.SetPropertyBlock(mpb);
        }
    }

    public override void HideOutline()
    {
        foreach (Renderer r in renderers)
        {
            if (index == -1)
            {
                r.GetPropertyBlock(mpb);
            }
            else
            {
                r.GetPropertyBlock(mpb, index);
            }
            mpb.SetColor("_OutlineColor", originalOutlineColor); // Set for that material only
            r.SetPropertyBlock(mpb);
        }
    }
}
