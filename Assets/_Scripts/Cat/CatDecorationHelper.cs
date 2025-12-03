using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatDecorationHelper : MonoBehaviour
{
    [field: SerializeField] public bool IsHeadTopOccupied { get; set; }
    [field: SerializeField] public bool IsForeheadOccupied { get; set; }
    [field: SerializeField] public bool IsEyesOccupied { get; set; }
    [field: SerializeField] public bool IsMouthOccupied { get; set; }
    [field: SerializeField] public bool IsChinOccupied { get; set; }
    
    [field: SerializeField] public GameObject head { get; private set; }
    [field: SerializeField] public GameObject body { get; private set; }
    [field: SerializeField] public GameObject cat { get; private set; }

    [ContextMenu("TEST")]
    public void GenerateDecorations()
    {
        //ManagerCatModifier.instance.GenerateDecorations(this, 100);
    }
}
