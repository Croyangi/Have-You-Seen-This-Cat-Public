using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerGeometry : MonoBehaviour
{
    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private UnityEvent onTriggerExit;
    
    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        onTriggerExit?.Invoke();
    }
}
