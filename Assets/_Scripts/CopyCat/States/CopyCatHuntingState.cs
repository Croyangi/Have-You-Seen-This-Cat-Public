using System.Collections;
using Pathfinding;
using Pathfinding.Drawing;
using UnityEngine;

public class CopyCatHuntingState : State<CopyCatStateMachine>
{
    
    [Header("Building Block References")]
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private Seeker seeker;
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private CopyCatAttackHelper attackHelper;
    [SerializeField] private OnCollisionHandler onCollisionHandler;
    [SerializeField] private CopyCat copyCat;

    [Header("References")]
    [SerializeField] private GameObject copyCatObj;
    private GameObject _playerHead;
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private Animator animator;
    
    public Vector3 lastSeenPlayerPosition;
    public bool isFromCharging;
    public bool isSuddenDeath;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float aggressionSpeedMultiplier;
    
    [SerializeField] private float omniscientTimer;
    [SerializeField] private float omniscientSet;
    
    [SerializeField] private float searchSpotThreshold;
    [SerializeField] private float stopDistance;
    [SerializeField] private bool isInAnimation;
    
    
    private GameObject _ambienceSfxObject;
    [SerializeField] private AudioClip[] ambienceSfxs;
    [SerializeField] private bool canPlayAmbience;
    [SerializeField] private float ambienceSfxTime;

    private ManagerPlayer _managerPlayer;

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

        _managerPlayer = ManagerPlayer.instance;
        
        _playerHead = _managerPlayer.PlayerHead;
        
        followerEntity.stopDistance = stopDistance;
        followerEntity.maxSpeed = movementSpeed + copyCat.aggression * aggressionSpeedMultiplier;
        
        followerEntity.isStopped = false;
        followerEntity.velocity = rb.linearVelocity;
        rb.linearVelocity = Vector3.zero;
        attackHelper.canKill = true;
        
        canPlayAmbience = true;

        pathfindingHelper.IsFearless = true;
        
        lastSeenPlayerPosition = _playerHead.transform.position;
        
        animationHelper.PlayAnimation(animationHelper.Chase, animationHelper.BaseLayer);
        animationHelper.SetRotationWithVelocityCheck(false);
        animationHelper.SetCanRotate(true);

        if (isFromCharging)
        {
            isFromCharging = false;
            return;
        }
        
        omniscientTimer = omniscientSet;
        isInAnimation = true;
        StartCoroutine(PlayRoarAnimation());
    }

    private IEnumerator PlayRoarAnimation()
    {
        followerEntity.isStopped = true;
        animationHelper.SetCanRotate(false);
        
        Vector3 direction = Vector3.ProjectOnPlane((copyCatObj.transform.position - _managerPlayer.PlayerHead.transform.position), Vector3.up);
        copyCatObj.transform.rotation = Quaternion.LookRotation(direction);
        animationHelper.PlayAnimation(animationHelper.Roar, animationHelper.BaseLayer, forcePlay: true);
        yield return new WaitForSeconds(animationHelper.GetCurrentTime(animationHelper.BaseLayer));
        isInAnimation = false;
        followerEntity.isStopped = false;
        animationHelper.SetCanRotate(true);
    }

    public override void ExitState()
    {
        CopyCatAttackHelper.OnKillPlayer -= OnKillPlayer;
        
        omniscientTimer = omniscientSet;
        
        attackHelper.canKill = false;
        pathfindingHelper.IsFearless = false;
        
        animationHelper.SetRotationWithVelocityCheck(false);
        animationHelper.SetCanRotate(true);
    }
    
    private void AggressiveExit()
    {
        animationHelper.SetRotationWithVelocityCheck(false);
        animationHelper.SetCanRotate(true);
    }

    public bool HasClearChargePath(Vector3 to)
    {
        GraphMask traversableGraphMask = GraphMask.FromGraphName(PathfindingGraphUtility.CopyCatPathfindingGraph);
        
        if (AstarPath.active == null || AstarPath.active.data == null) return false;

        var graphs = AstarPath.active.data.graphs;
        if (graphs == null) return false;
            
        for (int gi = 0; gi < graphs.Length; gi++)
        {
            if (!traversableGraphMask.Contains((uint) gi)) continue;

            var g = graphs[gi];
            if (g == null) continue;
                
            var raycastable = g as IRaycastableGraph;
            if (raycastable == null) continue;

            GraphHitInfo hit;
            bool blocked = raycastable.Linecast(transform.position, to, out hit);
            Debug.DrawRay(transform.position, to, Color.red);

            if (!blocked) return true; // if clear, we can charge
        }
        
        return false;
    }

    public override void UpdateTick()
    {
        OmniscientTimer();
        if (omniscientTimer > 0 || isSuddenDeath || pathfindingHelper.GetCopyCatPartialLOS())
        {
            pathfindingHelper.CanSeePlayerUpdate(true);
        }
        else
        {
            pathfindingHelper.CanSeePlayerUpdate();
        }
    }
    
    public override void FixedUpdateTick()
    {
        if (isInAnimation) return;
        animationHelper.PlayAnimation(animationHelper.Chase, animationHelper.BaseLayer);
        
        // Separate from canSeePlayer to avoid clashing omniscience
        // Needs very clear LOS
        if (!isSuddenDeath && pathfindingHelper.CanSeePlayer && pathfindingHelper.GetCopyCatCompleteLOS() && HasClearChargePath(_playerHead.transform.position))
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Charging], AggressiveExit);
            return;
        }

        ////
        if (pathfindingHelper.CanSeePlayer && !followerEntity.reachedEndOfPath)
        {
            lastSeenPlayerPosition = _playerHead.transform.position;
            animationHelper.PlayAnimation(animationHelper.Chase, animationHelper.BaseLayer);
        } else if (!followerEntity.isStopped && Vector3.Distance(lastSeenPlayerPosition, transform.position) <= stopDistance + searchSpotThreshold)
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Searching], AggressiveExit);
            return;
        }

        if (!pathfindingHelper.IsPathPossible(lastSeenPlayerPosition))
        {
            lastSeenPlayerPosition = transform.position;
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Searching], AggressiveExit);
            return;
        }

        pathfindingHelper.SetAIDestinationWithPosition(lastSeenPlayerPosition);
        AmbientSfxUpdate();
    }

    private void AmbientSfxUpdate()
    {
        // Ambient sounds
        if (canPlayAmbience && _ambienceSfxObject == null)
        {
            AudioClip clip = ambienceSfxs[Random.Range(0, ambienceSfxs.Length)];
            SFXObject temp = ManagerSFX.Instance.PlaySFX(clip, transform.position,
                0.1f, false, ManagerAudioMixer.Instance.AMGSFX, copyCatObj.transform, maxDistance: 75f);
            ManagerSFX.Instance.ApplyLowPassFilter(temp);
            
            _ambienceSfxObject = temp.gameObject;

            canPlayAmbience = false;
            StartCoroutine(AmbienceSfxTimer(clip));
        }
    }

    private IEnumerator AmbienceSfxTimer(AudioClip clip)
    {
        yield return new WaitForSeconds(ambienceSfxTime + clip.length);
        canPlayAmbience = true;
    }

    public void OmniscientTimer()
    {
        if (omniscientTimer > 0)
        {
            omniscientTimer = Mathf.Max(omniscientTimer -= Time.deltaTime, 0f);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (pathfindingHelper.CanSeePlayer)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawSphere(lastSeenPlayerPosition, 0.4f);
            
            Gizmos.DrawLine(transform.position, lastSeenPlayerPosition);
        }
    }
}
