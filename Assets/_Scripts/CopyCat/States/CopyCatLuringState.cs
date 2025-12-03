using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using Pathfinding;
using UnityEngine.AI;

public class CopyCatLuringState : State<CopyCatStateMachine>
{

    [Header("References")]
    [SerializeField] protected CopyCatAnimationHelper animationHelper;
    [SerializeField] protected CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] protected FollowerEntity followerEntity;
    [SerializeField] protected AIDestinationSetter aiDestinationSetter;
    [SerializeField] private CopyCatSpawnPointHelper spawnpointHelper;
    
    private GameObject _playerHead;

    [Header("Settings")] 
    [SerializeField] private float stopDistancePadding;
    [SerializeField] private float stopDistance;
    [SerializeField] private float movementSpeed;

    [SerializeField] private GameObject currentTarget;
    [SerializeField] private GameObject previousTarget;
    
    [SerializeField] private float catNearbyOmissionDistance;
    

    private void Start()
    {
        _playerHead = ManagerPlayer.instance.PlayerHead;
    }

    public override void EnterState()
    {
        followerEntity.maxSpeed = movementSpeed;
        followerEntity.stopDistance = stopDistance;
        
        animationHelper.PlayAnimation(animationHelper.Walk, animationHelper.BaseLayer);
        
        AssignCat();
    }
    
    
    public List<GameObject> GetValidCatsAroundPlayer()
    {
        List<GameObject> lostCats = new List<GameObject>(ManagerCat.instance.LostCats);
        List<GameObject> validTargets = new List<GameObject>();

        foreach (GameObject cat in lostCats)
        {
            CatStateMachine csm = cat.GetComponent<Cat>().StateMachine;
            if (csm == null && csm.CurrentState != csm.CatStatesDictionary[CatStateMachine.CatStates.Lost]) continue;
            //if (Vector3.Distance(cat.transform.position, transform.position) > catSearchDistance) continue;
            if (Vector3.Distance(cat.transform.position, transform.position) < catNearbyOmissionDistance) continue;
            validTargets.Add(cat);
        }
        
        validTargets.AddRange(GetCatSpawnPoints());

        return validTargets;
    }
    
    private Collider[] _spawnPointColliders = new Collider[64];
    [SerializeField] private LayerMask catSpawnPointLayerMask;
    public List<GameObject> GetCatSpawnPoints()
    {
        List<GameObject> spawnPoints = new List<GameObject>();
        
        int count = Physics.OverlapSphereNonAlloc(
            _playerHead.transform.position, 
            999f, 
            _spawnPointColliders, 
            catSpawnPointLayerMask
        );
        
        
        for (int i = 0; i < count; i++)
        {
            GameObject spawnpoint = _spawnPointColliders[i].gameObject;
            if (spawnpoint == null) continue; // safety check
            if (!pathfindingHelper.IsPathPossible(spawnpoint.transform.position)) continue; // path possible?
            spawnPoints.Add(spawnpoint);
        }
        
        return spawnPoints;
    }

    private void AssignCat()
    {
        if (currentTarget != null) previousTarget = currentTarget;
        currentTarget = GetWeightedValidCat(GetValidCatsAroundPlayer());

        if (currentTarget != null)
        {
            aiDestinationSetter.target = currentTarget.transform;
            return;
        }
        else if (ManagerCat.instance.FoundCats.Count > 0)
        {
            Debug.Log("Defaulting to Stalking");
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Stalking]);
            return;
        }
        else if (previousTarget != null && pathfindingHelper.IsPathPossible(previousTarget.transform.position))
        {
            Debug.LogWarning("No valid spawnpoints found, reusing old spawnpoint.");
            aiDestinationSetter.target = previousTarget.transform;
            currentTarget = previousTarget;
            return;
        }
        
        // This should never reach here.
        StartCoroutine(ExitToRoaming());
    }

    private IEnumerator ExitToRoaming()
    {
        Debug.LogWarning("Exited to Roaming");
        yield return new WaitForSeconds(0.5f);
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming]);
    }
    
    /// <summary>
    /// Returns a valid cat closest to player without being seen.
    /// </summary>
    private GameObject GetWeightedValidCat(List<GameObject> cats)
    {
        Dictionary<GameObject, float> weightedCats = new Dictionary<GameObject, float>();

        foreach (GameObject cat in cats)
        {
            if (cat == previousTarget) continue;
            if (pathfindingHelper.IsInLOS(cat.transform.position, _playerHead.transform.position)) continue;
            
            float distanceToPlayerPosition = Vector3.Distance(cat.transform.position, _playerHead.transform.position);
            weightedCats.Add(cat, distanceToPlayerPosition);
        }
        
        var sortedList = weightedCats.OrderBy(x => x.Value).ToList();

        if (sortedList.Count > 0)
        {
            return sortedList[0].Key;
        }
        
        return null;
        
    }

    public override void ExitState()
    {
    }

    [SerializeField] private float lazyUpdateTime;
    [SerializeField] private float lazyUpdateInterval;
    public override void FixedUpdateTick()
    {
        if (lazyUpdateTime <= 0)
        {
            LazyUpdate();
            lazyUpdateTime = lazyUpdateInterval;
        }
        else
        {
            lazyUpdateTime -= Time.fixedDeltaTime;
        }
        
        // sometimes reachedEndOfPath is finicky
        if (currentTarget != null && followerEntity.reachedEndOfPath && Vector3.Distance(currentTarget.transform.position, transform.position) <= stopDistance + stopDistancePadding)
        {
            if (stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.LuringToCopied] is CopyCatLuringToCopiedState luringToCopied)
            {
                luringToCopied.target = currentTarget;
                ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.LuringToCopied]);
                return;
            }
        }
    }

    private void LazyUpdate()
    {
        if (currentTarget == null)
        {
            //Debug.Log("null");
            AssignCat();
            return;
        }
        
        if (pathfindingHelper.IsInLOS(currentTarget.transform.position, _playerHead.transform.position))
        {
            //Debug.Log("los");
            AssignCat();
            return;
        }
        
        if (!pathfindingHelper.IsPathPossible(currentTarget.transform.position))
        {
            //Debug.Log("path not possible");
            AssignCat();
            return;
        }
    }

    [ContextMenu("State to Retreat")]
    public void ForceToRetreat()
    {
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Retreat]);
    }

    public override void UpdateTick()
    {
        if (pathfindingHelper.RetreatCheck()) return;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catNearbyOmissionDistance);
        
        Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, catSearchDistance);
    }
}
