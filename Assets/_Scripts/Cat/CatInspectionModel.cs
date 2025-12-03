using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatInspectionModel : MonoBehaviour
{
    [Header("Physical Modifiers")]
    public List<BodyPart> bodyParts;
    public List<CatPhysicalModifier> defaultModifiers;
    public List<Material[]> OriginalMaterials = new List<Material[]>();
    
    [Serializable]
    public class BodyPart
    {
        public int id;
        public SkinnedMeshRenderer smr;
        public CatPhysicalModifier catPhysicalModifier;
    }
    
    
    [Header("General References")]
    [SerializeField] private Animator animator;
    
    [SerializeField] private float tailSpeed;
    [SerializeField] private float tailSpeedBuffer;
    
    [SerializeField] private bool isBlinking = true;
    [SerializeField] private float blinkingSpeed;
    [SerializeField] private float blinkingBuffer;
    
    
    [Header("State References")]
    private const string TailSpeedMultiplier = "tailSpeed";
    private const string WaddleSpeedMultiplier = "waddleSpeed";

    public string BaseLayer { get; private set; }  = "Base";
    public string TailLayer { get; private set; } = "Tail";
    public string EyeLayer { get; private set; } = "Eye";
    
    
    public string Cancel { get; private set; } = "Empty";
    public string TailAnim { get; private set; } = "tail";
    public string BlinkAnim { get; private set; } = "blink";


    private void Awake()
    {
        var runtimeController = animator.runtimeAnimatorController;
        var newController = Instantiate(runtimeController);
        animator.runtimeAnimatorController = newController;
        
        PlayAnimation(TailAnim, TailLayer);
        animator.SetFloat(TailSpeedMultiplier, tailSpeed + Random.Range(-tailSpeedBuffer, tailSpeedBuffer));
        
        StopAllCoroutines();
        StartCoroutine(AmbientBlinking());
    }

    private void OnEnable()
    {
        PlayAnimation(TailAnim, TailLayer);
        animator.SetFloat(TailSpeedMultiplier, tailSpeed + Random.Range(-tailSpeedBuffer, tailSpeedBuffer));
        
        StopAllCoroutines();
        StartCoroutine(AmbientBlinking());
    }
    
    private void PlayAnimation(string state, string layer, float transitionTime = 0)
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
}


