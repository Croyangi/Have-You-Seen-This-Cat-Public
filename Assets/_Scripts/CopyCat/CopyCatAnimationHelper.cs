using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class CopyCatAnimationHelper : MonoBehaviour
{
    [Header("General References")]
    [SerializeField] private Animator animator;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private GameObject copyCat;
    [SerializeField] private float slerpRotationScale;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private bool isRotatingWithRigidbody;
    [SerializeField] private bool canRotate = true;

    [Header("State References")]
    private const string ChaseSpeedMultiplier = "chaseSpeed";
    private const string WalkSpeedMultiplier = "walkSpeed";
    private const string TransformationSpeedMultiplier = "transformSpeed";
    
    public string BaseLayer { get; private set; }  = "Base";
    [SerializeField] private string currentBaseLayerState;
    
    public string Cancel { get; private set; } = "Empty";
    public string Chase { get; private set; } = "chase";
    public string Walk { get; private set; } = "walk";
    public string Idle { get; private set; } = "idle";
    public string Transform { get; private set; } = "transform";
    public string Crash { get; private set; } = "crash";
    public string Roar { get; private set; } = "roar";
    public string Jumpscare { get; private set; } = "jumpscare";

    [Header("Settings")] 
    [SerializeField] private float chaseSpeedRatio;
    [SerializeField] private float walkSpeedRatio;

    private void Awake()
    {
        var runtimeController = animator.runtimeAnimatorController;
        var newController = Instantiate(runtimeController);
        animator.runtimeAnimatorController = newController;
    }

    private void Start()
    {
        PlayAnimation(Cancel, BaseLayer);
    }
    
    public void PlayAnimation(string state, string layer, bool forcePlay = false)
    {
        int index = animator.GetLayerIndex(layer);

        if (state.Equals(currentBaseLayerState) && !forcePlay)
        {
            return;
        }
        
        animator.Play(state, index);
        
        currentBaseLayerState = state;
        animator.Update(0f);
    }

    private void FixedUpdate()
    {
        if (followerEntity.isStopped)
        {
            SetChaseSpeed(rb.linearVelocity.magnitude);
            SetWalkSpeed(rb.linearVelocity.magnitude);
        }
        else
        {
            SetChaseSpeed(followerEntity.velocity.magnitude);
            SetWalkSpeed(followerEntity.velocity.magnitude);
        }

        RotationWithVelocity();
    }

    private void SetChaseSpeed(float speed)
    {
        animator.SetFloat(ChaseSpeedMultiplier, speed / chaseSpeedRatio);
    }
    
    private void SetWalkSpeed(float speed)
    {
        animator.SetFloat(WalkSpeedMultiplier, speed / walkSpeedRatio);
    }

    public void SetTransformSpeed(float speed)
    {
        animator.SetFloat(TransformationSpeedMultiplier, speed);
    }
    
    public float GetCurrentTime(string layer)
    {
        int index = animator.GetLayerIndex(layer);
        animator.Update(Time.deltaTime);
        return animator.GetCurrentAnimatorStateInfo(index).length;
    }

    public void SetRotationWithVelocityCheck(bool state) => isRotatingWithRigidbody = state;

    public void SetCanRotate(bool state) => canRotate = state;
    
    private void RotationWithVelocity()
    {
        if (!canRotate) return;
        
        if (isRotatingWithRigidbody && rb.linearVelocity != Vector3.zero)
        {
            Vector3 rbVelocity = -rb.linearVelocity;
            rbVelocity.y = 0; // Ignore Y-axis changes

            // Smooth the rotation using Slerp
            Quaternion finalRotation = Quaternion.Slerp(
                copyCat.transform.rotation,
                Quaternion.LookRotation(rbVelocity),
                slerpRotationScale
            );
            
            copyCat.transform.rotation = finalRotation;
        }
        else if (followerEntity.velocity != Vector3.zero)
        {
            Vector3 velocity = -followerEntity.velocity;
            velocity.y = 0; // Ignore Y-axis changes

            if (velocity == Vector3.zero)
            {
                return;
            }

            // Smooth the rotation using Slerp
            Quaternion finalRotation = Quaternion.Slerp(
                copyCat.transform.rotation,
                Quaternion.LookRotation(velocity),
                slerpRotationScale
            );

            copyCat.transform.rotation = finalRotation;
        }
    }
}
