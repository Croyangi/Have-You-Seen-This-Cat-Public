using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatSearchingState : State<CopyCatStateMachine>
{
    
    [Header("Building Block References")]
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private Seeker seeker;
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private CopyCatAttackHelper attackHelper;
    [SerializeField] private CopyCat copyCat;

    [Header("References")]
    [SerializeField] private GameObject copyCatObj;
    private GameObject _playerHead;

    private GameObject _ambienceSfxObject;
    [SerializeField] private AudioClip[] ambienceSfxs;
    [SerializeField] private List<AudioClip> availableAmbienceSfxs;
    [SerializeField] private bool canPlayAmbience;
    [SerializeField] private float ambienceSfxTimer;

    [Header("Settings")] 
    [SerializeField] private float baseMovementSpeed;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float aggressionSpeedMultiplier;
    
    [SerializeField] private float stopDistance;

    [SerializeField] private float wanderOffTimer;
    [SerializeField] private float wanderOffTimerSet;

    [SerializeField] private int wanderSearchScore;
    [SerializeField] private int wanderSearchScoreSpread;

    [SerializeField] private Vector3 playerSearchPosition;
    [SerializeField] private Vector3 searchPosition;
    
    [SerializeField] private bool hasBegunMoving;

    private void OnDisable()
    {
        CopyCatAttackHelper.OnKillPlayer -= OnKillPlayer;
    }

    private void OnKillPlayer()
    {
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Jumpscare]);
    }
    
    public override void EnterState()
    {
        CopyCatAttackHelper.OnKillPlayer += OnKillPlayer;
        attackHelper.canKill = true;

        _playerHead = ManagerPlayer.instance.PlayerHead;

        followerEntity.stopDistance = stopDistance;
        movementSpeed = baseMovementSpeed + copyCat.aggression * aggressionSpeedMultiplier;
        
        SearchAreaForPlayer();

        playerSearchPosition = _playerHead.transform.position;
        if (!pathfindingHelper.IsPathPossible(_playerHead.transform.position) && stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting] is CopyCatHuntingState huntingState)
        {
            playerSearchPosition = huntingState.lastSeenPlayerPosition;
        }
        
        wanderOffTimer = wanderOffTimerSet;
        
        availableAmbienceSfxs = new List<AudioClip>(ambienceSfxs);
        canPlayAmbience = true;
    }

    public override void ExitState()
    {
        CopyCatAttackHelper.OnKillPlayer -= OnKillPlayer;
        attackHelper.canKill = false;
        
        pathfindingHelper.IsFearless = false;

        animationHelper.SetRotationWithVelocityCheck(false);
        animationHelper.SetCanRotate(true);

        if (_ambienceSfxObject != null)
        {
            Destroy(_ambienceSfxObject);
        }
    }
    
    private void HuntingExit()
    {
        animationHelper.SetRotationWithVelocityCheck(false);
        animationHelper.SetCanRotate(true);

        if (_ambienceSfxObject != null)
        {
            Destroy(_ambienceSfxObject);
        }
    }

    [ContextMenu("Search")]
    private void SearchAreaForPlayer()
    {
        hasBegunMoving = false;
        RandomPath path = RandomPath.Construct(playerSearchPosition, wanderSearchScore);
        path.spread = wanderSearchScoreSpread;
        seeker.StartPath(path, OnRandomPathComplete);
    }

    private void OnRandomPathComplete(Path p)
    {
        if (p.error)
        {
            Debug.LogError("Path calculation failed: " + p.errorLog);
            return;
        }
        Vector3 pos = (Vector3)p.path[p.path.Count - 1].position;
        searchPosition = pos;
    }

    private void WanderOffTimerUpdate()
    {
        wanderOffTimer = Mathf.Max(0f, wanderOffTimer - Time.deltaTime);
    }

    public override void UpdateTick()
    {
        WanderOffTimerUpdate();
        pathfindingHelper.CanSeePlayerUpdate();
    }
    
    public override void FixedUpdateTick()
    {
        float normalized = 1f - (wanderOffTimer / wanderOffTimerSet); 
        float time = Mathf.PI * normalized;
        float speedMultiplier = Mathf.Pow(Mathf.Abs(Mathf.Sin(time)), 0.5f);
        followerEntity.maxSpeed = movementSpeed * speedMultiplier;

        if (pathfindingHelper.CanSeePlayer)
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting], HuntingExit);
            return;
        }
        
        if (wanderOffTimer <= 0)
        {
            copyCat.aggression++;
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming]);
            return;
        }

        pathfindingHelper.SetAIDestinationWithPosition(searchPosition);
        if (followerEntity.reachedEndOfPath && !followerEntity.isStopped && hasBegunMoving)
        {
            SearchAreaForPlayer();
            return;
        }
        
        // Brief delay between transitions
        hasBegunMoving = true;

        animationHelper.PlayAnimation(animationHelper.Chase, animationHelper.BaseLayer);
        
        // Ambient sounds
        if (canPlayAmbience && _ambienceSfxObject == null)
        {
            AudioClip clip = availableAmbienceSfxs[Random.Range(0, availableAmbienceSfxs.Count)];
            SFXObject temp = ManagerSFX.Instance.PlaySFX(clip, transform.position,
                 0.3f, false, ManagerAudioMixer.Instance.AMGSFX, copyCatObj.transform, maxDistance: 75f);
            ManagerSFX.Instance.ApplyLowPassFilter(temp);
            
            _ambienceSfxObject = temp.gameObject;
            
            availableAmbienceSfxs.Remove(clip);
            if (availableAmbienceSfxs.Count == 0)
            {
                availableAmbienceSfxs = new List<AudioClip>(ambienceSfxs);
            }

            canPlayAmbience = false;
            StartCoroutine(AmbienceSfxTimer(clip));
        }
    }

    private IEnumerator AmbienceSfxTimer(AudioClip clip)
    {
        yield return new WaitForSeconds(ambienceSfxTimer + clip.length);
        canPlayAmbience = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Optional: Draw debug visuals here
        }
    }
}
