using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatStateHelper : MonoBehaviour
{
    [Header("References")]

    [field: SerializeField] public bool IsWithPlayer { get; private set; }

    public void SetWithPlayer(bool state)
    {
        IsWithPlayer = state;
    }
}
