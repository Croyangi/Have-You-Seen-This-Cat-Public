using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inspect Settings", menuName = "Croyangi's Script Objs/New Inspect Settings")]
public class InspectSettings : ScriptableObject
{
    public Vector3 posOffset = Vector3.zero;

    public Vector3 posRotationOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    public bool invertedControls = false;
    public float zoomMultiplier = 1f;
}
