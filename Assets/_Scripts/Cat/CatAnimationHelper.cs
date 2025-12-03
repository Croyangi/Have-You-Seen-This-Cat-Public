using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatAnimationHelper : MonoBehaviour
{
    [Header("General References")]
    [SerializeField] private Animator animator;
    
    private Coroutine idleAnimationCoroutine;
    [SerializeField] private FollowerEntity followerEntity;

    [Header("Settings")] 
    [SerializeField] private float waddleSpeedRatio;
    [SerializeField] private float waddleSpeedMin;
        
    [SerializeField] private float tailSpeed;
    [SerializeField] private float tailSpeedBuffer;
    
    [SerializeField] private float blinkingSpeed;
    [SerializeField] private float blinkingBuffer;
    [SerializeField] private bool isBlinking = true;
    
    [SerializeField] private float idleAnimationSpeed;
    [SerializeField] private float idleAnimationBuffer;
    
    
    [Header("State References")]
    private const string TailSpeedMultiplier = "tailSpeed";
    private const string WaddleSpeedMultiplier = "waddleSpeed";
    private const string TransformSpeedMultiplier = "transformSpeed";

    public string BaseLayer { get; private set; }  = "Base";
    
    public string IdleAnimationsLayer { get; private set; } = "IdleAnimations";
    public string TailLayer { get; private set; } = "Tail";
    public string EyeLayer { get; private set; } = "Eye";
    
    
    public string Cancel { get; private set; } = "Empty";
    public string TailAnim { get; private set; } = "tail";
    public string BlinkAnim { get; private set; } = "blink";
    public string SitAnim { get; private set; } = "sit";
    public string WaddleAnim { get; private set; } = "waddle"; 
    public string StillAnim { get; private set; } = "null"; 
    public string LookAroundAnim { get; private set; } = "look_around"; 
    public string LookLeftAnim { get; private set; } = "look_left"; 
    public string LookRightAnim { get; private set; } = "look_right"; 
    public string DanceAnim { get; private set; } = "dance"; 
    public string Transform { get; private set; } = "transform"; 

    [SerializeField] private string currentBaseLayerState = "";
    [field: SerializeField] public bool IsDynamicWalking { get; set; }

    private void Awake()
    {
        var runtimeController = animator.runtimeAnimatorController;
        var newController = Instantiate(runtimeController);
        animator.runtimeAnimatorController = newController;
        
        PlayAnimation(TailAnim, TailLayer);
        animator.SetFloat(TailSpeedMultiplier, tailSpeed + Random.Range(-tailSpeedBuffer, tailSpeedBuffer));
        
        StartCoroutine(AmbientBlinking());
    }

    private void OnEnable()
    {
        PlayAnimation(TailAnim, TailLayer);
        animator.SetFloat(TailSpeedMultiplier, tailSpeed + Random.Range(-tailSpeedBuffer, tailSpeedBuffer));
        
        StartCoroutine(AmbientBlinking());
    }

    private void FixedUpdate()
    {
        SetWaddleSpeed(followerEntity.maxSpeed);
        SetBaseWalkingAnimations();
    }

    private void SetBaseWalkingAnimations()
    {
        if (!IsDynamicWalking) return;
        
        if (followerEntity.desiredVelocity.magnitude > 0.1f)
        {
            PlayAnimation(WaddleAnim, BaseLayer);
        }
        else
        {
            PlayAnimation(SitAnim, BaseLayer);
        }
    }
    
    private void SetWaddleSpeed(float speed)
    {
        if (speed / waddleSpeedRatio < 1)
        {
            animator.SetFloat(WaddleSpeedMultiplier, 1);
        }
        else
        {
            animator.SetFloat(WaddleSpeedMultiplier, speed / waddleSpeedRatio);
        }
    }
    
    public void SetTransformSpeed(float speed)
    {
        animator.SetFloat(TransformSpeedMultiplier, speed);
    }

    public void PlayAnimation(string state, string layer, float transitionTime = 0, bool forcePlay = false)
    {
        int index = animator.GetLayerIndex(layer);

        if (transitionTime == 0)
        {
            animator.Play(state, index);
        }
        else
        {
            animator.CrossFade(state, transitionTime, index);
        }
        
        currentBaseLayerState = state;
        animator.Update(0f);
    }

    private IEnumerator AmbientBlinking()
    {
        while (isBlinking)
        {
            PlayAnimation(BlinkAnim, EyeLayer);
            yield return new WaitForSeconds(blinkingSpeed + Random.Range(-blinkingBuffer, blinkingBuffer));
            PlayAnimation(null, EyeLayer);
        }

        yield return null;
    }

    public void ToggleIdleAnimations(bool state)
    {
        if (state)
        {
            idleAnimationCoroutine = StartCoroutine(IdleAnimation());
        }
        else if (idleAnimationCoroutine != null)
        {
            StopCoroutine(idleAnimationCoroutine);
            PlayAnimation(Cancel, IdleAnimationsLayer, 0.3f);
        }
    }

    private IEnumerator IdleAnimation()
    {
        // Tack extra wait in between
        yield return new WaitForSeconds(idleAnimationSpeed + Random.Range(-idleAnimationBuffer, idleAnimationBuffer));
        
        // Play
        PlayAnimation(GetRandomIdleAnimation(), IdleAnimationsLayer, 0.3f);
        
        // Wait duration of animation
        int index = animator.GetLayerIndex(IdleAnimationsLayer);
        animator.Update(Time.deltaTime);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(index).length);
        
        // Return to normal
        PlayAnimation(Cancel, IdleAnimationsLayer, 0.3f);

        idleAnimationCoroutine = StartCoroutine(IdleAnimation());
    }

    private string GetRandomIdleAnimation()
    {
        string[] idleAnimations = { LookAroundAnim, LookLeftAnim, LookRightAnim, DanceAnim };
        return idleAnimations[Random.Range(0, idleAnimations.Length)];
    }
}
