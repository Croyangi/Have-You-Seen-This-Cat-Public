using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatScatteringState : State<CatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private GameObject cat;
    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatStateHelper stateHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CatAnimationHelper animationHelper;
    [SerializeField] private GameObject playerHead;
    [SerializeField] private Seeker seeker;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;

    [SerializeField] private bool isSearchInProgress;
    [SerializeField] private Vector3 targetPosition;

    [SerializeField] private AudioClip[] hisses;
    
    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    
    [SerializeField] private int wanderSearchScore;
    [SerializeField] private int wanderSearchScoreSpread;

    public override void EnterState()
    {
        playerHead = ManagerPlayer.instance.PlayerHead;
        
        movementHelper.SetMovementSpeed(movementSpeed);
        //animationHelper.PlayAnimation(animationHelper.WaddleAnim, animationHelper.BaseLayer, 1f);
        
        aiDestinationSetter.enabled = false;

        GetScatterPath();
        ManagerCat.instance.RemoveCat(cat);
        ManagerCat.instance.ResetCatChainAiTarget();
        ManagerSFX.Instance.PlaySFX(hisses[Random.Range(0, hisses.Length)], transform.position, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }

    public override void ExitState()
    {
        aiDestinationSetter.enabled = true;
    }

    public override void FixedUpdateTick()
    {
        if (isSearchInProgress) return;
        if (followerEntity.reachedEndOfPath)
        {
            if (!IsInLOS(playerHead.transform.position, transform.position))
            {
                Destroy(cat);
                return;
            }
            
            ChangeState(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.Lost]);
        }
    }
    
    private bool IsInLOS(Vector3 origin, Vector3 end)
    {
        Vector3 direction = (end - origin).normalized;
        float range = Vector3.Distance(origin, end);
        Ray ray = new Ray(origin, direction);
    
        if (Physics.Raycast(ray, out RaycastHit hit, range, LayerUtility.BlocksLOS))
        {
            return false; // Obstacle detected
        }
    
        return true; // Clear line of sight
    }
    
    [ContextMenu("Search")]
    private void GetScatterPath()
    {
        isSearchInProgress = true;
        RandomPath path = RandomPath.Construct(transform.position, wanderSearchScore);
        path.spread = wanderSearchScoreSpread;
        seeker.StartPath(path, OnRandomPathComplete);
    }

    private void OnRandomPathComplete(Path p)
    {
        if (p.error)
        {
            Debug.LogError("Path calculation failed: " + p.errorLog);
            // Handle error: perhaps transition to roaming or try a new search
            isSearchInProgress = false; // Allow state to proceed even with error, or implement specific error handling.
            return;
        }
        Vector3 pos = (Vector3)p.path[p.path.Count - 1].position;
        
        followerEntity.destination = pos;
        followerEntity.SearchPath();
        
        isSearchInProgress = false; // Path is now calculated and ready to be used.
    }
}
