using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ManagerCopyCat : MonoBehaviour
{
    [SerializeField] private GameObject copyCatPrefab;
    [field: SerializeField] public GameObject CopyCatObj { get; private set; }
    [field: SerializeField] public GameObject MimicCat { get; private set; }
    [field: SerializeField] public CopyCat CopyCat { get; private set; }
    [field: SerializeField] public CopyCatStateMachine CopyCatStateMachine { get; private set; }
    [field: SerializeField] public CopyCatPathfindingHelper CopyCatPathfindingHelper { get; private set; }
    
    [SerializeField] private Tag tag_copyCat;

    // Manager
    public static ManagerCopyCat Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one CopyCat Manager in the scene.");
        }

        Instance = this;

        // Heavy method, but only called once for reference
        //CopyCat = FindCopyCat();
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += GameStart;
        ManagerGameplay.OnTimerEnd += EnterSuddenDeath;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= GameStart;
        ManagerGameplay.OnTimerEnd -= EnterSuddenDeath;
    }
    
    private void GameStart()
    {
        InitializeCopyCat();
    }

    private void OnDestroy()
    {
        Destroy(CopyCat);
        CopyCatStateMachine = null;
        MimicCat = null;
    }

    private void InitializeCopyCat()
    {
        CopyCatObj = Instantiate(copyCatPrefab).gameObject;
        CopyCat = CopyCatObj.GetComponent<CopyCat>();
        CopyCatStateMachine = CopyCat.GetComponentInChildren<CopyCatStateMachine>();
        CopyCatPathfindingHelper = CopyCat.GetComponentInChildren<CopyCatPathfindingHelper>();
        CopyCatStateMachine.ForceState(CopyCatStateMachine.CopyCatStates.Respawn);
    }

    public void SetMimicCat(GameObject mimicCat)
    {
        MimicCat = mimicCat;
    }

    public void ForceTransformCopyCat()
    {
        if (CopyCatStateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Copied] is CopyCatCopiedState copiedState)
        {
            CopyCatStateMachine.RequestStateChange(copiedState);
            copiedState.patienceTimer = 0;
        }
    }

    private void EnterSuddenDeath()
    {
        CopyCat.aggression = 10f;
        CopyCatHuntingState huntingState = CopyCatStateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting] as CopyCatHuntingState;
        huntingState.isSuddenDeath = true;
        
        if (CopyCatStateMachine.CurrentState == CopyCatStateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.LuringToCopied] &&
                   CopyCatStateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.LuringToCopied] is CopyCatLuringToCopiedState luringToCopiedState)
        {
            CopyCatStateMachine.RequestStateChange(CopyCatStateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming], luringToCopiedState.CleanUpTarget);
        }
        

        CopyCatStateMachine.RequestStateChange(CopyCatStateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting]);
    }
}