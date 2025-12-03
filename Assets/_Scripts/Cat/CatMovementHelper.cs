using System;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CatMovementHelper : MonoBehaviour
{
    [Header("References")]
    [field: SerializeField] public FollowerEntity FollowerEntity { get; set; }

    [field: SerializeField] public AIDestinationSetter AIDestinationSetter { get; set; }
    [SerializeField] private Cat cat;
    [SerializeField] private GameObject catObj;
    [SerializeField] private GameObject catModel;
    [SerializeField] private CatPhysicalModifierHelper physicalModifierHelper;
    [SerializeField] private float sneezeTimer;
    [SerializeField] private float sneezeTimerRandom;
    
    [SerializeField] private Rigidbody rb;

    [Header("Settings")]
    [field: SerializeField] public float MovementSpeed { get; private set; }
    [SerializeField] private float slerpScale;

    private void Awake()
    {
        cat.OnFound += OnFound;
    }

    private void OnDestroy()
    {
        cat.OnFound -= OnFound;
    }

    private void OnFound()
    {
        StartCoroutine(CatSneezeTimer());
    }

    public void SetMovementSpeed(float speed)
    {
        MovementSpeed = speed;
        FollowerEntity.maxSpeed = speed;
    }

    private void FixedUpdate()
    {
        RotationWithVelocity();
    }

    private IEnumerator CatSneezeTimer()
    {
        while (!physicalModifierHelper.isMimic)
        {
            float time = sneezeTimer + Random.Range(-sneezeTimerRandom, sneezeTimerRandom);
            yield return new WaitForSeconds(time);
            StartCoroutine(OnCatSneeze());
        }
    }
    
    [SerializeField] private float catJitter;
    [SerializeField] private float catJitterMultiplier;
    [SerializeField] private AudioClip[] catSneezeSFXs;

    private IEnumerator OnCatSneeze()
    {
        float timer = 5;
        float currentTime = timer;
        while (!physicalModifierHelper.isMimic && currentTime > 0)
        {
            float math = Mathf.Abs(timer - currentTime) * catJitterMultiplier;
        
            float jitterAmount = catJitter + math;
        
            Vector3 jitter = new Vector3(
                Random.Range(-jitterAmount, jitterAmount),
                Random.Range(-jitterAmount, jitterAmount),
                Random.Range(-jitterAmount, jitterAmount)
            );

            catModel.transform.localPosition = jitter;
            
            currentTime -= Time.deltaTime;
            yield return null;
        }
        catModel.transform.localPosition = Vector3.zero;

        if (physicalModifierHelper.isMimic) yield break;
        ManagerSFX.Instance.PlaySFX(catSneezeSFXs[Random.Range(0, catSneezeSFXs.Length)], transform.position, 0.3f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }

    private void RotationWithVelocity()
    {
        if (AIDestinationSetter.target != null)
        {
            Vector3 velocity = FollowerEntity.desiredVelocity;
            velocity.y = 0;

            if (velocity != Vector3.zero)
            {

                // Smooth the rotation using Slerp
                Quaternion finalRotation = Quaternion.Slerp(
                    catObj.transform.rotation,
                    Quaternion.LookRotation(velocity),
                    slerpScale
                );

                catObj.transform.rotation = finalRotation;
            }
        }
    }
}
