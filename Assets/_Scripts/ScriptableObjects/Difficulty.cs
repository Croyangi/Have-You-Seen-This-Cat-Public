using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Difficulty", menuName = "Croyangi's Script Objs/New Difficulty")]
public class Difficulty : ScriptableObject
{
    [Range(0, 999)]
    public int invalidCount;
    
    [Range(0, 1)]
    public float validSkipChance; // The lower, the harder
    
    [Range(0, 7)]
    public int maxValids = 7;
    
    [Range(0, 1)]
    public float invalidSkipChance; // The higher, the harder
    
    [Range(0, 99999)]
    public int collectionGoal;
    
    [Range(1, 99999)]
    public int timeLimit;

    [Range(0, 999)] 
    public int naturalCatSpawnCount;

    public GameObject floorDemo;
}
