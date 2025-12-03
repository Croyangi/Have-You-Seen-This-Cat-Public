using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;

public class CopyCatTransformingState : State<CopyCatStateMachine>
{
    
    [Header("References")]
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] private CopyCatStateMachine copyCatStateMachine;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    
    [SerializeField] private float transformationDuration;

    [Header("Cat")] 
    [SerializeField] private GameObject mimicCat;
    [SerializeField] private GameObject copyCat;
    [SerializeField] private GameObject copyCatModel;
    [SerializeField] private GameObject hitboxes;
    [SerializeField] private CapsuleCollider mainBodyCollider;
    [SerializeField] private CapsuleCollider headCollider;
    [SerializeField] private Rigidbody copyCatRigidBody;
    

    public override void EnterState()
    {
        mimicCat = ManagerCopyCat.Instance.MimicCat;
        
        animationHelper.SetCanRotate(false);
        copyCatRigidBody.isKinematic = false;
        
        copyCat.transform.position = mimicCat.transform.position;
        copyCatRigidBody.position = mimicCat.transform.position;
        
        followerEntity.enabled = false;

        pathfindingHelper.IsFearless = true;
        
        mimicCat.GetComponentInChildren<CatStateMachine>().RequestStateChange(mimicCat.GetComponentInChildren<CatStateMachine>().CatStatesDictionary[CatStateMachine.CatStates.Transforming]);   
        
        ManagerCat.instance.RemoveCat(mimicCat);
        ManagerCat.instance.ResetCatChainAiTarget();
        
        StartCoroutine(RevealCopyCat());
        
        ManagerCat.instance.OnScatterCats();
        
        ManagerPlayer.instance.PlayerVFXHelper.Scare();
    }

    public override void ExitState()
    {
        followerEntity.isStopped = false;
        followerEntity.enabled = true;
    }

    public override void FixedUpdateTick()
    {
        if (mimicCat != null)
        {
            mimicCat.transform.position = copyCat.transform.position;
        }
    }

    public override void UpdateTick()
    {

    }

    private IEnumerator RevealCopyCat()
    {
        Vector3 direction = Vector3.ProjectOnPlane((copyCat.transform.position - ManagerPlayer.instance.PlayerHead.transform.position), Vector3.up);
        copyCat.transform.rotation = Quaternion.LookRotation(direction);
        mimicCat.transform.rotation = Quaternion.LookRotation(-direction);
        
        copyCatModel.SetActive(true);
        hitboxes.SetActive(true);
        headCollider.enabled = false;
        mainBodyCollider.radius /= 2f;
        
        // Play anims
        animationHelper.PlayAnimation(animationHelper.Transform, animationHelper.BaseLayer);
        float animDur = animationHelper.GetCurrentTime(animationHelper.BaseLayer);
        float multiplier = animDur / transformationDuration;
        animationHelper.SetTransformSpeed(multiplier);
        mimicCat.GetComponentInChildren<CatAnimationHelper>().SetTransformSpeed(multiplier);
        
        yield return new WaitForSeconds(animDur / multiplier);
        Destroy(mimicCat);
        
        headCollider.enabled = true;
        mainBodyCollider.radius *= 2;
        animationHelper.SetCanRotate(true);
        copyCatRigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting]);
    }
}
