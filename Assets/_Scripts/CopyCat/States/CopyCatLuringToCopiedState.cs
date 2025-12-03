using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class CopyCatLuringToCopiedState : State<CopyCatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private GameObject playerHead;
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] private CopyCatCopiedState copyCatCopiedState;
    
    [Header("Copy Cat")]
    [SerializeField] private GameObject copyCat;
    [SerializeField] private GameObject hitboxes;
    [SerializeField] private Rigidbody copyCatRigidBody;
    
    [Header("Cat")]
    [SerializeField] private GameObject mimicCat;

    public GameObject target;
    [SerializeField] private CatStateMachine catStateMachine;
    [SerializeField] private GameObject catPrefab;

    [SerializeField] private float resumeRoamingTimer; // Timer for different conditions
    [SerializeField] private float resumeRoamingSet;
    [SerializeField] private float resumeRoamingDistanceThreshold;
    [SerializeField] private float patienceTimer; // Global timer when it transforms back
    [SerializeField] private float patienceSet;
    [SerializeField] private bool wasIgnored;


    private void Start()
    {
        playerHead = ManagerPlayer.instance.PlayerHead;
    }

    public override void EnterState()
    {
        patienceTimer = patienceSet;
        resumeRoamingTimer = resumeRoamingSet;
        wasIgnored = false;
        
        animationHelper.PlayAnimation(animationHelper.Idle, animationHelper.BaseLayer);
        
        copyCat.SetActive(false);
        
        aiDestinationSetter.target = null;
        followerEntity.isStopped = true;
        
        hitboxes.SetActive(false);
        copyCatRigidBody.isKinematic = true;
        
        mimicCat = Instantiate(catPrefab, target.transform.position, target.transform.rotation);
        
        catStateMachine = mimicCat.GetComponentInChildren<CatStateMachine>();
        mimicCat.GetComponentInChildren<CatPhysicalModifierHelper>().isMimic = true;
        ManagerCopyCat.Instance.SetMimicCat(mimicCat);

        if (target.TryGetComponent(out CatSpawnpoint catSpawnpoint))
        {
            catSpawnpoint.ReplaceWithMimicCat(mimicCat);
        }
        else
        {
            target.SetActive(false);
        }
    }

    public override void ExitState()
    {
    }

    public override void FixedUpdateTick()
    {
        // If you found the cat
        if (catStateMachine != null && catStateMachine.CurrentState != null && catStateMachine.CurrentState != catStateMachine.CatStatesDictionary[CatStateMachine.CatStates.Lost])
        {
            Debug.Log(catStateMachine.CurrentState);
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Copied]);
        }
        else
        {
            // All timers regarding if the cat hasn't been found yet
            NotFoundUpdate();
        }
    }

    private void NotFoundUpdate()
    {
        // If player ignores the fake
        if (!wasIgnored && pathfindingHelper.CanPlayerSeeCopyCat())
        {
            wasIgnored = true;
        }
        
        // (If player is far enough OR wasIgnored) and does not have LOS to Copycat
        // SPECIFICALLY THE MIMIC'D CAT, COPYCAT IS ONLY FOR LOS DETECT
        bool isFarEnough = Vector3.Distance(mimicCat.transform.position, playerHead.transform.position) > resumeRoamingDistanceThreshold;
        if ((isFarEnough || wasIgnored) && !pathfindingHelper.GetCopyCatPartialLOS())
        {
            resumeRoamingTimer = Mathf.Clamp(resumeRoamingTimer -= Time.fixedDeltaTime, 0f, resumeRoamingSet);
        }
        else
        {
            resumeRoamingTimer = resumeRoamingSet;
        }
        
        // General timer before it resumes Roaming
        patienceTimer = Mathf.Clamp(patienceTimer -= Time.fixedDeltaTime, 0f, patienceSet);
    }

    public void CleanUpTarget()
    {
        copyCat.SetActive(true);
        followerEntity.isStopped = false;
        hitboxes.SetActive(true);
        copyCatRigidBody.isKinematic = false;
        Destroy(mimicCat);

        if (target == null) return;
        
        if (target.TryGetComponent(out CatSpawnpoint catSpawnpoint))
        {
            catSpawnpoint.CleanUpCat();
        }
        else
        {
            target.SetActive(true);
        }
    }

    public override void UpdateTick()
    {
        if (resumeRoamingTimer <= 0 || (patienceTimer <= 0 && !pathfindingHelper.IsInLOS(transform.position, playerHead.transform.position)))
        {
            CleanUpTarget();
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Roaming]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.LuringToCopied] == stateMachine.CurrentState)
        {
            if (pathfindingHelper.IsInLOS(transform.position, playerHead.transform.position))
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            Gizmos.DrawLine(transform.position, playerHead.transform.position);
            
            if (Vector3.Distance(transform.position, playerHead.transform.position) > resumeRoamingDistanceThreshold)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireSphere(transform.position, resumeRoamingDistanceThreshold);
        }
    }
}
