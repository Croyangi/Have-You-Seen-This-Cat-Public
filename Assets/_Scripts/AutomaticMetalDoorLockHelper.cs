using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;

public class AutomaticMetalDoorLockHelper : MonoBehaviour
{
    [SerializeField] private UnityEvent onTriggerExit;
    [SerializeField] private bool isInversed;
    
    private void OnTriggerExit(Collider other)
    {
        Vector3 exitDir = (other.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, exitDir);
        
        if ((angle < 90f && !isInversed) || (angle > 90f && isInversed))
        {
            onTriggerExit?.Invoke();
        }
    }

}
