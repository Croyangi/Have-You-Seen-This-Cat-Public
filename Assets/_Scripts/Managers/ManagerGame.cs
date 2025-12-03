using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Path = System.IO.Path;
using Random = UnityEngine.Random;

public class ManagerGame : MonoBehaviour
{ 
    [field: SerializeField] public bool IsOngoing { get; private set; }
    [field: SerializeField] public Difficulty Difficulty { get; set; }
    
    public static ManagerGame Instance { get; private set; }

    public static Action OnGameInitialize;
    public static Action OnGameStart;
    public static Action OnGameEnd;
    

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Game Manager in the scene.");
        }
        Instance = this;
    }

    [ContextMenu("Initialize")]
    public void InitializeGame()
    {
        OnGameInitialize?.Invoke();
    }
    
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        if (IsOngoing) return;
        IsOngoing = true;
        OnGameStart?.Invoke();
    }
    
    [ContextMenu("Start Tutorial")]
    public void StartTutorial()
    {
        IsOngoing = true;
        OnGameStart?.Invoke();
    }

    [ContextMenu("Game End")]
    public void GameEnd()
    {
        StopAllCoroutines();
        IsOngoing = false;
        OnGameEnd?.Invoke();
    }
}
