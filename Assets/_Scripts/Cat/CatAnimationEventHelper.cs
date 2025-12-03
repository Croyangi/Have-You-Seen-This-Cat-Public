using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Audio;

public class CatAnimationEventHelper : MonoBehaviour
{
    [SerializeField] private GameObject playerHead;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private FollowerEntity followerEntity;
    [SerializeField] private float volumeMovementMultiplier;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float baseVolume;
    
    [SerializeField] private float distanceThreshold;
    [SerializeField] private float footstepTimer;
    [SerializeField] private bool canPlayFootstep;

    private void Start()
    {
        playerHead = ManagerPlayer.instance.PlayerHead;
        canPlayFootstep = true;
    }

    public void PlayFootstepSound()
    {
        if (!canPlayFootstep) return;
        
        if (Vector3.Distance(transform.position, playerHead.transform.position) > distanceThreshold) return;
        
        float volume = baseVolume;
        volume += followerEntity.velocity.magnitude * volumeMovementMultiplier;
        volume = Mathf.Min(volume, 0.1f);

        float distance = 7f;
        if (aiDestinationSetter.target != null && aiDestinationSetter.target.gameObject != playerHead)
        {
            distance = 5f;
        }
        
        ManagerSFX.Instance.PlaySFX(footstepSounds[Random.Range(0, footstepSounds.Length - 1)], transform.position, volume, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.3f, maxDistance: distance);
        
        canPlayFootstep = false;
        StartCoroutine(FootstepTimer());
    }
    
    private IEnumerator FootstepTimer()
    {
        yield return new WaitForSeconds(footstepTimer);
        canPlayFootstep = true;
    }
}
