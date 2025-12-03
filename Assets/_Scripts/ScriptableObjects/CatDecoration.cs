using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cat Decoration", menuName = "Croyangi's Script Objs/New Cat Decoration")]
public class CatDecoration : ScriptableObject
{
    [Header("Decoration")]
    [field: SerializeField] public GameObject decoration { get; private set; }
    
    [Header("Decoration Settings")]
    [field: SerializeField] public bool OccupiesHeadTop { get; private set; }
    [field: SerializeField] public bool OccupiesForehead { get; private set; }
    [field: SerializeField] public bool OccupiesEyes { get; private set; }
    [field: SerializeField] public bool OccupiesMouth { get; private set; }
    [field: SerializeField] public bool OccupiesChin { get; private set; }
    [field: SerializeField] public bool IsBodyDecoration { get; private set; }
    
    [Header("Decoration Offset Settings")]
    [field: SerializeField] public Vector3 PositionOffset { get; private set; }
    [field: SerializeField] public Vector3 RotationOffset { get; private set; }
    
    
}
