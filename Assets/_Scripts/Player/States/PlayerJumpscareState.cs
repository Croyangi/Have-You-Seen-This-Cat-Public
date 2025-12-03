using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerJumpscareState : State<PlayerStateMachine>
{
    [Header("References")]
    [SerializeField] private PlayerInputHelper inputHelper;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerFlashlightHelper flashlightHelper;
    
    public override void EnterState()
    {
        flashlightHelper.SetFlashlight(true);
        rb.isKinematic = true;
        PlayerInputHelper pih = ManagerPlayer.instance.PlayerInputHelper;
        pih.SetProcessing(false, pih.All, "jumpscare");
    }
    public override void ExitState()
    {
        rb.isKinematic = false;
        PlayerInputHelper pih = ManagerPlayer.instance.PlayerInputHelper;
        pih.SetProcessing(true, pih.All, "jumpscare");
    }
}
