using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Pathfinding;
using Shapes;
using UnityEngine;
using UnityEngine.AI;

public class CopyCatCopiedState : State<CopyCatStateMachine>
{
    
    [Header("References")] 
    [SerializeField] private CopyCat copyCat;

    [Header("Cat")] 
    [SerializeField] private GameObject viableTarget;
    [SerializeField] private AIDestinationSetter catAIDestinationSetter;
    [SerializeField] private FollowerEntity catFollowerEntity;
    [SerializeField] private CatStateMachine catStateMachine;
    [SerializeField] private GameObject cat;
    [SerializeField] private GameObject catModel;
    private CatCopiedState _catCopiedState;
    [SerializeField] private bool isDetached;

    [Header("Settings")] 
    public float patienceTimer;
    [SerializeField] private float patienceSet;
    [SerializeField] private float patienceSetMin;
    
    [SerializeField] private float groundRaycastDistance;
    [SerializeField] private float groundUnwalkableTolerance;
    
    [SerializeField] private float catJitterPatienceThreshold;
    [SerializeField] private float catJitter;
    [SerializeField] private float catJitterMultiplier;
    
    [SerializeField] private float aggressionPatienceMultiplier;
    
    private Vector3 _nodePos;

    private void Awake()
    {
        
    }

    public override void EnterState()
    {
        cat = ManagerCopyCat.Instance.MimicCat;
        patienceTimer = Mathf.Max(patienceSet - (copyCat.aggression * aggressionPatienceMultiplier), patienceSetMin);
        
        catJitterPatienceThreshold = patienceTimer / 3f;
        
        catStateMachine = cat.GetComponentInChildren<CatStateMachine>();
        catAIDestinationSetter = cat.GetComponentInChildren<AIDestinationSetter>();
        catFollowerEntity = cat.GetComponentInChildren<FollowerEntity>();

        isDetached = false;
        
        if (catStateMachine.CatStatesDictionary[CatStateMachine.CatStates.Copied] is CatCopiedState copiedState)
        {
            _catCopiedState = copiedState;
            catModel = _catCopiedState.CatModel;
        }
    }

    public override void ExitState()
    {
        if (viableTarget != null)
        {
            Destroy(viableTarget);
        }
    }
    
    public override void FixedUpdateTick()
    {
        if (cat == null)
        {
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Respawn]);
            return;
        }
        
        patienceTimer = Mathf.Max(0, patienceTimer -= Time.fixedDeltaTime);

        if (patienceTimer < catJitterPatienceThreshold) JitterCat();
        
        if (patienceTimer <= 0)
        {
            if (catStateMachine.CurrentState == catStateMachine.CatStatesDictionary[CatStateMachine.CatStates.Inspecting])
            {
                PlayerInventoryHelper playerInventory = ManagerPlayer.instance.PlayerInventoryHelper;
                playerInventory.DropHeldItem();
                return;
            } 
            
            if (!isDetached)
            {
                isDetached = true;
                DetachCat();
                return;
            }

            if (catAIDestinationSetter.target == null || !catFollowerEntity.reachedEndOfPath ||
                !catFollowerEntity.hasPath || catFollowerEntity.pathPending) return;

            ManagerCopyCat.Instance.SetMimicCat(cat);
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Transforming]);

        }
    }

    [ContextMenu("Test")]
    public void Fail()
    {
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Respawn]);
    }

    private void JitterCat()
    {
        float math = Mathf.Abs(catJitterPatienceThreshold - patienceTimer) * catJitterMultiplier;
        
        float jitterAmount = catJitter + math;
        
        Vector3 jitter = new Vector3(
            UnityEngine.Random.Range(-jitterAmount, jitterAmount),
            UnityEngine.Random.Range(-jitterAmount, jitterAmount),
            UnityEngine.Random.Range(-jitterAmount, jitterAmount)
        );

        catModel.transform.localPosition = jitter;
    }

    private void DetachCat()
    {
        catStateMachine.RequestStateChange(_catCopiedState); 
        
        if (viableTarget != null)
        {
            Destroy(viableTarget);
        }
        
        Vector3 point = GetNearestViablePosition(ManagerPlayer.instance.PlayerHead.transform.position);
        viableTarget = new GameObject();
        viableTarget.transform.position = point;
            
        catAIDestinationSetter.target = viableTarget.transform;
        catFollowerEntity.destination = viableTarget.transform.position;
        catFollowerEntity.SearchPath();
        return;
    }

    private Vector3 GetNearestViablePosition(Vector3 position, float maxDistance = 5f)
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(position, out navHit, maxDistance, NavMesh.AllAreas))
        {
            return navHit.position; // nearest valid spot on NavMesh
        }

        // fallback: just return original position
        return position;
    }

    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && stateMachine.CurrentState == stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Copied] && cat !=null)
        {
            Gizmos.color = Color.white;
            DrawStringGizmo.DrawString(patienceTimer.ToString("F2"), cat.transform.position + new Vector3(0, 2f, 0), Gizmos.color, new Vector2(0.5f, 0.5f), 15f);
 

            Gizmos.color = Color.red;
            Gizmos.DrawCube(_nodePos, Vector3.one * 0.5f);
        }
    }
}
