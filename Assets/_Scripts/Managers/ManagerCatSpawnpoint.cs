using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCatSpawnpoint : MonoBehaviour
{
    [SerializeField] private Tag tag_catSpawnpoint;
    [field: SerializeField] public List<GameObject> CatSpawnpoints { get; private set; }
    [field: SerializeField] public List<CatSpawnpoint> CatSpawnpointsScript { get; private set; }
    
    [SerializeField] private int catsAvailableCount;

    // Manager
    public static ManagerCatSpawnpoint instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Cat Manager in the scene.");
        }
        instance = this;
        
        // Heavy method, but only called once for reference
        FindAllCatSpawnpoints();
    }

    private void Start()
    {
        OnChangeCatAvailability();
    }

    private void OnEnable()
    {
        CatSpawnpoint.OnChangeAvailability += OnChangeCatAvailability;
    }
    
    private void OnDisable()
    {
        CatSpawnpoint.OnChangeAvailability -= OnChangeCatAvailability;
    }

    private void FindAllCatSpawnpoints()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        // Loop through each object
        foreach (GameObject obj in allObjects)
        {
            if (obj.TryGetComponent<Tags>(out var tags) && tags.SearchTag(tag_catSpawnpoint))
            {
                CatSpawnpoints.Add(obj);
                CatSpawnpointsScript.Add(obj.GetComponent<CatSpawnpoint>());
            }
        }
    }

    private void OnChangeCatAvailability()
    {
        catsAvailableCount = GetActualAvailableCatsCount();
        if (catsAvailableCount <= 0) ReduceLeastCooldownSpawnpoint();
    }

    private void ReduceLeastCooldownSpawnpoint()
    {
        float min = CatSpawnpointsScript[0].SpawnTimer;
        int index = 0;

        for (int i = 0; i < CatSpawnpointsScript.Count; i++)
        {
            CatSpawnpoint spawnpoint = CatSpawnpointsScript[i];
            if (spawnpoint.SpawnTimer < min && spawnpoint.IsSpawnpointUnoccupied())
            {
                min = CatSpawnpointsScript[i].SpawnTimer;
                index = i;
            }
        }
        CatSpawnpointsScript[index].ReduceSpawnTimer();
    }
    
    private int GetActualAvailableCatsCount()
    {
        int count = 0;
        foreach (var spawn in CatSpawnpointsScript)
        {
            if (spawn.IsOccupied && !spawn.IsOccupiedByCopyCat)
            {
                count++;
            }
        }
        return count;
    }

    [ContextMenu("Remove All Cats")]
    private void RemoveAllCats()
    {
        foreach (CatSpawnpoint catSpawnpoint in CatSpawnpointsScript)
        {
            catSpawnpoint.RemoveCat();           
        }
    }
}
