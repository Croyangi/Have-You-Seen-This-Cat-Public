using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationName;
    [SerializeField] private string layerName;
    private void Start()
    {
        animator = GetComponent<Animator>();
        PlayAnimation(animationName, layerName);
    }
    
    public void PlayAnimation(string state, string layer)
    {
        int index = animator.GetLayerIndex(layer);
        animator.Play(state, index);
    }
}
