using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCatAttackHelper : MonoBehaviour
{
    public static Action OnKillPlayer;
    public bool canKill;
    
    [SerializeField] private CopyCatStateMachine stateMachine;

    private void OnTriggerEnter(Collider other)
    {
        KillPlayer();
    }

    public void KillPlayer()
    {
        if (canKill)
        {
            OnKillPlayer?.Invoke();
        }
    }
}
