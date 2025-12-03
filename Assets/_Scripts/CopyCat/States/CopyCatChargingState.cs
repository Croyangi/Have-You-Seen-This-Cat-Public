using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatChargingState : State<CopyCatStateMachine>
{
    
    [Header("Building Block References")]
    [SerializeField] private CopyCatPathfindingHelper pathfindingHelper;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private CopyCatAnimationHelper animationHelper;
    [SerializeField] private OnCollisionHandler onCollisionHandler;
    private CopyCatHuntingState huntingState;

    [Header("References")] 
    [SerializeField] private GameObject playerHead;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider headCollider;
    [SerializeField] private float failSafeTimer;
    [SerializeField] private float failSafeTimerSet;
    
    [SerializeField] private AudioClip crashSfx;
    
    [Header("Charge Settings")]
    [SerializeField] private bool hasCrashed;
    [SerializeField] private float lastVelocityMagnitude;
    [SerializeField] private float chargeAcceleration; // Acceleration
    [SerializeField] private float chargeCrashMultiplier; // Multiplies fastestChargeMagnitude
    [SerializeField] private float chargeCrashThreshold; // Threshold to detect an actual crash
    [SerializeField] private float chargeCrashTimer;
    [SerializeField] private float chargeCrashSet;
    [SerializeField] private Vector3 lastChargeDirection;
    [SerializeField] private float chargeForeverThreshold;

    private void OnEnable()
    {
        onCollisionHandler.OnCollisionEnterAction += OnHandleCollision;
    }
    
    private void OnDisable()
    {
        onCollisionHandler.OnCollisionEnterAction -= OnHandleCollision;
        CopyCatAttackHelper.OnKillPlayer -= OnKillPlayer;
    }

    private void OnKillPlayer()
    {
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Jumpscare]);
    }
    
    public override void EnterState()
    {
        huntingState = stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting] as CopyCatHuntingState;
        
        failSafeTimer = failSafeTimerSet;
        
        CopyCatAttackHelper.OnKillPlayer += OnKillPlayer;
        
        chargeCrashTimer = chargeCrashSet;
        
        animationHelper.PlayAnimation(animationHelper.Chase, animationHelper.BaseLayer);
        animationHelper.SetRotationWithVelocityCheck(true);
        animationHelper.SetCanRotate(true);
        
        playerHead = ManagerPlayer.instance.PlayerHead;
        
        followerEntity.isStopped = true;
        
        rb.linearVelocity = followerEntity.velocity;
        
        headCollider.enabled = true;
    }

    public override void ExitState()
    {
        CopyCatAttackHelper.OnKillPlayer -= OnKillPlayer;

        followerEntity.isStopped = false;
        headCollider.enabled = false;
        animationHelper.SetRotationWithVelocityCheck(false);
        animationHelper.SetCanRotate(false);
        
        pathfindingHelper.IsFearless = false;
    }

    private void AggressiveExit()
    {
        followerEntity.isStopped = false;
        headCollider.enabled = false;
        animationHelper.SetRotationWithVelocityCheck(false);
        animationHelper.SetCanRotate(false);
    }

    public override void FixedUpdateTick()
    {
        failSafeTimer = Mathf.Max(0f, failSafeTimer - Time.deltaTime);
        if (failSafeTimer <= 0)
        { 
            ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting], AggressiveExit);    
            return;
        }
        
        
        ChargeUpdate();
        huntingState.OmniscientTimer();
        
        huntingState.lastSeenPlayerPosition = playerHead.transform.position;
        
        // Inverse
        if (hasCrashed || rb.linearVelocity.magnitude > chargeForeverThreshold) return;
        
        if (pathfindingHelper.GetCopyCatCompleteLOS() &&
            huntingState.HasClearChargePath(playerHead.transform.position)) return;
    
        huntingState.isFromCharging = true;
        ChangeState(stateMachine.CopyCatStatesDictionary[CopyCatStateMachine.CopyCatStates.Hunting], AggressiveExit);    
    }
    
    public override void UpdateTick()
    {
        lastVelocityMagnitude = rb.linearVelocity.magnitude;
    }

    // On crashing HEAD-first
    private void OnHandleCollision(Collision collision)
    {
        if (collision.contacts[0].thisCollider.gameObject.name.ToLower() != "head")
        {
            return;
        }
        
        if ((LayerUtility.Environment.value & (1 << collision.gameObject.layer)) == 0)
        {
            return;
        }
        
        if (!hasCrashed && lastVelocityMagnitude > chargeCrashThreshold)
        {
            //Debug.Log("CRASH: " + lastVelocityMagnitude);
            hasCrashed = true;
            
            chargeCrashTimer = chargeCrashSet + ((lastVelocityMagnitude - chargeCrashThreshold) * chargeCrashMultiplier);
            rb.linearVelocity = Vector3.zero;
            
            animationHelper.PlayAnimation(animationHelper.Crash, animationHelper.BaseLayer);
            animationHelper.SetCanRotate(false);
            
            ManagerSFX.Instance.PlaySFX(crashSfx, transform.position, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        }
        else if (lastVelocityMagnitude < chargeCrashThreshold)
        {
            //Debug.Log("WEAK CRASH: " + (lastVelocityMagnitude));
        }
        else
        {
            //Debug.Log("INVALID CRASH");
        }
    }

    private void ChargeUpdate()
    {
        if (!hasCrashed)
        {
            ChargePhysicsUpdate();
        }
        else
        {
            ChargeCrashUpdate();
        }
    }

    private void ChargeCrashUpdate()
    {
        chargeCrashTimer = Mathf.Clamp(chargeCrashTimer -= Time.fixedDeltaTime, 0f, 999999);
        if (chargeCrashTimer <= 0)
        {
            hasCrashed = false;
            chargeCrashTimer = chargeCrashSet;
            
            animationHelper.PlayAnimation(animationHelper.Chase, animationHelper.BaseLayer);
            animationHelper.SetCanRotate(true);
        }
    }
    
    private void ChargePhysicsUpdate()
    {
        // Go to player, but if too fast, keep going
        if (rb.linearVelocity.magnitude < chargeForeverThreshold) {
            Vector3 direction = playerHead.transform.position - transform.position;
            lastChargeDirection = direction;
            
            // Face the direction of movement more quickly
            // Decelerate more aggressively if off-course
            float alignment = Vector3.Dot(rb.linearVelocity.normalized, direction);
            if (alignment < 0.5f) // If we're not mostly moving in the right direction
            {
                rb.linearVelocity *= 0.9f; // Apply some artificial deceleration
            }
            
            rb.AddForce(direction * chargeAcceleration, ForceMode.Force);
        }
        else
        {
            rb.AddForce(lastChargeDirection * chargeAcceleration, ForceMode.Force);
        }
    }
}
