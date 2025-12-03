using System;
using System.Collections;
using DG.Tweening;
using NUnit.Framework.Interfaces;
using UnityEngine;

public class BunkerMetalDoor : MonoBehaviour
{
    [field: SerializeField] public MetalDoor MetalDoor { get; private set; }
    [SerializeField] private Terminal[] terminals;

    public void ToggleDoor()
    {
        if (MetalDoor.IsOpen)
        {
            MetalDoor.Close();
        }
        else
        {
            MetalDoor.Open();
        }
    }

    public void ResetTerminals()
    {
        StartCoroutine(HandleResetTerminals());
    }

    private IEnumerator HandleResetTerminals()
    {
        yield return new WaitForSeconds(1f);
        foreach (Terminal terminal in terminals)
        {
            terminal.TerminalLock();
        }
    }
}
