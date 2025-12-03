using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using Random = UnityEngine.Random;

[SelectionBase]
public class CatSpawnpoint : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private GameObject cat;
    [SerializeField] private CatStateMachine catStateMachine;
    [SerializeField] private Transform catTree;
    
    [SerializeField] private Transform[] spawnpoints;
    [field: SerializeField] public bool IsOccupied { get; private set; }
    [field: SerializeField] public bool IsOccupiedByCopyCat { get; private set; }
    
    public static Action OnChangeAvailability;
    [SerializeField] private bool isOn;


    [Header("Settings")]
    [field: SerializeField] public float SpawnTimer { get; private set; }

    [SerializeField] private float spawnSet;
    [SerializeField] private float spawnTimerRandomOffset;


    private void Awake()
    {
        CleanUp();
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += GameStart;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= GameStart;
    }

    private void GameStart()
    {
        isOn = true;
    }

    private void CleanUp()
    {
        isOn = false;
        IsOccupied = false;
        IsOccupiedByCopyCat = false;
        _wasOccupied = false;
        SpawnTimer = spawnSet;
        SpawnTimer = Mathf.Clamp(SpawnTimer -= Random.Range(0, spawnTimerRandomOffset), 0, 9999f);
    }

    private void FixedUpdate()
    {
        if (!isOn) return;
        
        if (IsSpawnpointUnoccupied())
        {
            SpawnTimer = Mathf.Clamp(SpawnTimer -= Time.fixedDeltaTime, 0f, spawnSet);
            if (SpawnTimer <= 0f)
            {
                SpawnCat();
            }
        } else if (IsSpawnpointOccupied() && cat != null && !(catStateMachine.CurrentState == catStateMachine.CatStatesDictionary[CatStateMachine.CatStates.Lost]))
        {
            CleanUpCat();
            UpdateAvailableCatsCount();
        }
        
        // Fail-safe
        if (IsSpawnpointOccupied() && cat == null)
        {
            CleanUpCat();
            UpdateAvailableCatsCount();
        }
    }

    private void SpawnCat()
    {
        IsOccupied = true;
        SpawnTimer = spawnSet;
        cat = Instantiate(catPrefab, spawnpoints[Random.Range(0, spawnpoints.Length)].position, catTree.transform.rotation);
        catStateMachine = cat.GetComponentInChildren<CatStateMachine>();
        UpdateAvailableCatsCount();
    }

    private void UpdateAvailableCatsCount()
    {
        OnChangeAvailability?.Invoke();
    }

    private bool IsSpawnpointOccupied()
    {
        return IsOccupied || IsOccupiedByCopyCat;
    }
    
    public bool IsSpawnpointUnoccupied()
    {
        return !IsOccupied && !IsOccupiedByCopyCat;
    }

    public void ReduceSpawnTimer()
    {
        SpawnTimer = 0;
    }

    private bool _wasOccupied;
    public void ReplaceWithMimicCat(GameObject mimicCat)
    {
        if (cat != null)
        {
            _wasOccupied = true;
            RemoveCat();
        }

        cat = mimicCat;
        catStateMachine = mimicCat.GetComponentInChildren<CatStateMachine>();
        IsOccupied = true;
        IsOccupiedByCopyCat = true;
        mimicCat.transform.position = spawnpoints[Random.Range(0, spawnpoints.Length)].position;
        mimicCat.transform.rotation = catTree.transform.rotation;
    }

    public void CleanUpCat()
    {
        cat = null;
        catStateMachine = null;
        IsOccupied = false;
        IsOccupiedByCopyCat = false;

        if (_wasOccupied)
        {
            SpawnCat();
        }
        
        _wasOccupied = false;
    }

    public void RemoveCat()
    {
        ManagerCat.instance.RemoveCat(cat);
        Destroy(cat);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (IsSpawnpointUnoccupied())
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            DrawStringGizmo.DrawString(SpawnTimer.ToString("F2"), transform.position + new Vector3(0, 1f, 0), Gizmos.color, new Vector2(0.5f, 0.5f), 10f);
        }
    }
}
