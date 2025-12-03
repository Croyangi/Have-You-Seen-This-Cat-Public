using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Floor", menuName = "Croyangi's Script Objs/New Floor")]
public class Floor : ScriptableObject
{
    public string name;
    public int depth;
    public Difficulty difficulty;
}