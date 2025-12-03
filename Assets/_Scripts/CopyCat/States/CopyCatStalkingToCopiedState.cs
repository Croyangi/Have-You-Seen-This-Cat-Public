using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatStalkingToCopiedState : State<CopyCatStateMachine>
{
    
    [Header("References")] 
    [SerializeField] private CopyCatTransformingState copyCatTransformingState;
    [SerializeField] private CopyCatCopiedState copyCatCopiedState;
    
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private FollowerEntity followerEntity;
    
    [Header("Cat")] 
    [SerializeField] private GameObject copyCat;
    [SerializeField] private GameObject hitboxes;
    [SerializeField] private Rigidbody copyCatRigidBody;
    [SerializeField] private Transform parentTransform;

    [Header("Settings")] 
    [SerializeField] private float patienceTimer;
    [SerializeField] private float patienceSet;

    public override void EnterState()
    {
        patienceTimer = patienceSet;
        
        animationHelper.PlayAnimation(animationHelper.Idle, animationHelper.BaseLayer);
        
        copyCat.SetActive(false);
        
        aiDestinationSetter.target = null;
        followerEntity.enabled = false;
        
        hitboxes.SetActive(false);
        copyCatRigidBody.isKinematic = true;

        GameObject cat = ManagerCopyCat.Instance.MimicCat;
        CatPhysicalModifierHelper pmh = cat.GetComponentInChildren<CatPhysicalModifierHelper>();
        pmh.ForceAddInvalidPhysicalModifiers();
        
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Copied]);
    }

    public override void ExitState()
    {
    }
}
