using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CopyCatAnimationEventHelper : MonoBehaviour
{
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private AudioClip roarSFX;
    [SerializeField] private AudioClip[] transformSFXs;
    
    [SerializeField] private CopyCatStateMachine stateMachine;
    [SerializeField] private CopyCatRetreatState retreatState;

    private void OnEnable()
    {
        StartCoroutine(SetManagers());
    }

    private ManagerSFX _managerSFX;
    private ManagerAudioMixer _managerAudioMixer;
    private ManagerPlayer _managerPlayer;

    private IEnumerator SetManagers()
    {
        yield return new WaitUntil(() => ManagerSFX.Instance != null);
        _managerSFX = ManagerSFX.Instance;
        
        yield return new WaitUntil(() => ManagerAudioMixer.Instance != null);
        _managerAudioMixer = ManagerAudioMixer.Instance;
        
        yield return new WaitUntil(() => ManagerPlayer.instance != null);
        _managerPlayer = ManagerPlayer.instance;
    }
    
    public void PlayFootstepSFX()
    {
        float sfxRange = stateMachine.CurrentState == retreatState ? 20f : 50f;
        float cameraShakeRange = stateMachine.CurrentState == retreatState ? 15f : 30f;
        
        _managerPlayer.PlayerCameraHelper.QueueCameraShake(roarNoiseSettings, 0.5f, 5f, 0.1f, source: transform.position, range: cameraShakeRange);
        SFXObject sfx = _managerSFX.PlaySFX(footsteps[Random.Range(0, footsteps.Length)], transform.position, 0.1f, false, _managerAudioMixer.AMGSFX, pitchShift: 0.1f, maxDistance: sfxRange);
        _managerSFX.ApplyLowPassFilter(sfx);
    }

    [SerializeField] private NoiseSettings roarNoiseSettings;
    public void PlayRoarSFX()
    {
        _managerPlayer.PlayerCameraHelper.QueueCameraShake(roarNoiseSettings, 0.7f, 3f, roarSFX.length - 3f, 0.4f, transform.position, 25f);
        SFXObject sfx = _managerSFX.PlaySFX(roarSFX, transform.position, 0.1f, false, _managerAudioMixer.AMGSFX, pitchShift: 0.1f, maxDistance: 100f);
        _managerSFX.ApplyLowPassFilter(sfx);
    }
    
    public void PlayTransformSFX()
    {
        SFXObject sfx = _managerSFX.PlaySFX(transformSFXs[Random.Range(0, transformSFXs.Length)], transform.position, 0.1f, false, _managerAudioMixer.AMGSFX, pitchShift: 0.1f, maxDistance: 100f);
        _managerSFX.ApplyLowPassFilter(sfx);
    }
}
