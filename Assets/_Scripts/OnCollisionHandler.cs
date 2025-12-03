using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionHandler : MonoBehaviour
{
    public Action<Collision> OnCollisionEnterAction;
    public Action<Collision> OnCollisionExitAction;
    public Action<Collision> OnCollisionStayAction;

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterAction?.Invoke(collision);
    }
    
    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExitAction?.Invoke(collision);
    }
    
    private void OnCollisionStay(Collision collision)
    {
        OnCollisionStayAction?.Invoke(collision);
    }
}
