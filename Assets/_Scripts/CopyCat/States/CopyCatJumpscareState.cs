using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class CopyCatJumpscareState : State<CopyCatStateMachine>
{
    
    [Header("Building Block References")]
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private CopyCatAttackHelper attackHelper;
    
    [Header("References")]
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private Seeker seeker;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject hitboxes;
    private GameObject _playerHead;
    [SerializeField] private GameObject copyCatObj;
    [SerializeField] private float delayMultiplier;

    [SerializeField] private AudioClip jumpscareSfx;
    private GameObject _jumpscareSfxObject;

    private bool _isInJumpscare;
    
    public override void EnterState()
    {
        if (_isInJumpscare) return;
        _isInJumpscare = true;
        
        attackHelper.canKill = false;
        _playerHead = ManagerPlayer.instance.PlayerHead;
        hitboxes.SetActive(false);

        followerEntity.simulateMovement = false;
        rb.isKinematic = true;
        
        animationHelper.SetCanRotate(false);
        
        ManagerPlayer.instance.OnJumpscare();
        
        _jumpscareSfxObject = ManagerSFX.Instance.PlayRawSFX(jumpscareSfx, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX);
        
        Vector3 look = (copyCatObj.transform.position - _playerHead.transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(look);
        Quaternion tiltedRotation = Quaternion.Euler(-20f, rotation.eulerAngles.y, rotation.eulerAngles.z);
        copyCatObj.transform.rotation = tiltedRotation;
        
        Vector3 flatLook = Vector3.ProjectOnPlane(look, copyCatObj.transform.up).normalized;
        copyCatObj.transform.position = _playerHead.transform.position + (flatLook * 3f) - (copyCatObj.transform.up * 2.5f);
        
        float angleDifference = Quaternion.Angle(_playerHead.transform.localRotation, tiltedRotation);
        _playerHead.transform.DOLocalRotateQuaternion(tiltedRotation, 0.7f).SetEase(Ease.OutElastic);
        
        StartCoroutine(Jumpscare(angleDifference * delayMultiplier));
    }

    private IEnumerator Jumpscare(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);
        animationHelper.PlayAnimation(animationHelper.Jumpscare, animationHelper.BaseLayer);
        yield return new WaitForSeconds(1f);
        ManagerPlayer.instance.PlayerDeath();
        Destroy(_jumpscareSfxObject);
    }

    public override void ExitState()
    {
    }
}
