using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TransformingCatPoster : MonoBehaviour
{
    [SerializeField] private float distanceThreshold;
    [SerializeField] private bool isDetected;
    [SerializeField] private GameObject player;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    private void Awake()
    {
        skinnedMeshRenderer.material = new Material(skinnedMeshRenderer.material);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        isDetected = true;
        player = other.gameObject;
        StopAllCoroutines();
        StartCoroutine(DetectUpdate());
    }

    private void OnTriggerExit(Collider other)
    {
        isDetected = false;
    }
    
    private IEnumerator DetectUpdate()
    {
        float value = (Vector3.Distance(player.transform.position, transform.position) - distanceThreshold) / distanceThreshold;
        value = Mathf.Clamp(1 - value, 0, 1);
        skinnedMeshRenderer.material.SetFloat("_Transition", value);
        
        yield return new WaitForFixedUpdate();
        if (isDetected)
        {
            StartCoroutine(DetectUpdate());
        }
    }
}
