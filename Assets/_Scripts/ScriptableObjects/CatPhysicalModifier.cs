using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cat Physical Modifier", menuName = "Croyangi's Script Objs/New Cat Physical Modifier")]
public class CatPhysicalModifier : ScriptableObject
{
    [Header("Physical Modifier")]
    [field: SerializeField]
    public Mesh PhysicalModifier { get; set; }

    [field: SerializeField] public List<CatPhysicalModifier> Dependencies { get; private set; }

    // Applies to any dependencies, meaning it will not be included, preventing any duplicates
    public bool isDependency;
    public CatPhysicalModifier dependencyParent;

    [Header("Decoration Settings")]
    [field: SerializeField]
    public OccupationFlags OccupationFlags { get; set; }
}