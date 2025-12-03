using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CatInspectingState : State<CatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatAnimationHelper animationHelper;
    [SerializeField] private Cat cat;
    [SerializeField] private CatPhysicalModifierHelper physicalModifierHelper;
    
    [SerializeField] private GameObject purringSFXObject;
    [SerializeField] private AudioClip catPurring;
    [SerializeField] private AudioClip copyCatPurring;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed);
        followerEntity.enabled = false;
        rb.useGravity = false;
        animationHelper.IsDynamicWalking = false;
        animationHelper.PlayAnimation(animationHelper.StillAnim, animationHelper.BaseLayer);
        
        AudioClip clip = physicalModifierHelper.isMimic ? copyCatPurring : catPurring;
        purringSFXObject = ManagerSFX.Instance.PlaySFX(clip, transform.position, 0.01f, true, ManagerAudioMixer.Instance.AMGSFX, cat.transform, 0.1f).gameObject;
        
        _managerCat = ManagerCat.instance;
    }

    public override void ExitState()
    {
        animationHelper.IsDynamicWalking = true;
        followerEntity.enabled = true;
        rb.useGravity = true;
        
        Destroy(purringSFXObject);
    }


    private ManagerCat _managerCat;
    public override void UpdateTick()
    {
        if (!_managerCat.IsRollCalling)
        {
            //_managerCat.ResetCatChainAiTarget();
        }
        
        if (!cat.IsBeingInteracted)
        {
            if (_managerCat.IsRollCalling)
            {
                ChangeState(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.RollCall]);
            }
            else
            {
                ChangeState(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.Idle]);
            }
        }

    }
}
