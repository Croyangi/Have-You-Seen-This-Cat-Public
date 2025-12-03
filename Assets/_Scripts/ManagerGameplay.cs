using System;
using System.Collections;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerGameplay : MonoBehaviour
{
    [SerializeField] private GameObject retrieveCatsGoal;
    [SerializeField] private GameObject timerGoal;
    [field: SerializeField] public float Timer { get; private set; }
    
    [SerializeField] private string startingGameDialogueFile = "StartingGameDialogue.txt";
    
    public static ManagerGameplay Instance { get; private set; }

    public static Action OnTimerEnd;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Manager Tablet in the scene.");
        }
        Instance = this;
    }

    private void OnEnable()
    {
        ManagerGame.OnGameInitialize += OnGameInitialize;
        ManagerGame.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameInitialize -= OnGameInitialize;
        ManagerGame.OnGameStart -= OnGameStart;
    }

    private void Start()
    {
        if (!Application.isEditor) ManagerPlayer.instance.PlayerObj.transform.position = new Vector3(100, 0, 0);
        ManagerGame.Instance.Difficulty = ManagerElevator.Instance.ElevatorHelper.GetFloor().difficulty;
        Debug.Log(ManagerGame.Instance.Difficulty);
        ManagerGame.Instance.InitializeGame();
    }

    private void OnGameInitialize()
    {
        Timer = ManagerGame.Instance.Difficulty.timeLimit;
        GenerateLevelDemo();
    }

    private void OnGameStart()
    {
        StartCoroutine(TimerUpdate());
        
        ManagerPlayer.instance.PlayerGoalHelper.AddGoal(Instantiate(retrieveCatsGoal).GetComponent<RetrieveCatsGoal>());
        ManagerPlayer.instance.PlayerGoalHelper.AddGoal(Instantiate(timerGoal).GetComponent<TimerGoal>());
        ManagerPlayer.instance.PlayerGoalHelper.PingUI(10f);
        
        ManagerPlayer.instance.PlayerDialogueHelper.QueueDialogue(TextFileReader.GetRandomLine(Application.streamingAssetsPath, startingGameDialogueFile));
    }
    
    private IEnumerator TimerUpdate()
    {
        while (Timer > 0)
        {
            Timer = Mathf.Max(0f, Timer - Time.deltaTime);
            yield return null;
        }
        
        OnTimerEnd?.Invoke();
    }
    
    [ContextMenu("Generate")]
    public void GenerateLevelDemo()
    {
        GameObject floorLayout = Instantiate(ManagerGame.Instance.Difficulty.floorDemo);
        Bunker bunker = FindObjectOfType<Bunker>();
        LevelLayoutDemo levelLayoutDemo = floorLayout.GetComponent<LevelLayoutDemo>();

        Vector3 offset = bunker.ConnectionPoint.position - levelLayoutDemo.ConnectionPoint.position;
        levelLayoutDemo.transform.position += offset;
        
        foreach (var graph in AstarPath.active.data.graphs)
        {
            if (graph is RecastGraph recast)
            {
                recast.SnapBoundsToScene();
                recast.Scan();
            }
        }
    }
}