using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AreaAmbienceTrigger : MonoBehaviour
{
    private GameObject _sfxObj;
    private AudioSource _audioSource;
    private AudioLowPassFilter _audioLowPassFilter;
    private Transform _player;
    
    [Header("Settings")]
    [SerializeField] private AudioClip sfx;
    [SerializeField] private float volume;
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float minCutoffFrequency = 1;
    [SerializeField] private float maxCutoffFrequency = 6000f;
    private float _noCutoffFrequency = 22000f;
    [SerializeField] private float lowPassResonance = 1f;
    

    private bool _isProcessing;

    private void OnTriggerEnter(Collider other)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        
        _player = other.gameObject.transform;
        
        SetSFX();

        StartCoroutine(FixedUpdateTick());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_isProcessing) return;
        _isProcessing = false;
        
        if (_sfxObj != null) Destroy(_sfxObj);
    }

    private IEnumerator FixedUpdateTick()
    {
        while (_isProcessing)
        {
            float value = Vector3.Distance(_player.position, new Vector3(transform.position.x, _player.position.y, transform.position.z));
            float percent = 1 - Mathf.InverseLerp(minDistance, maxDistance, value);
            _audioSource.volume = percent * volume;
            _audioLowPassFilter.cutoffFrequency = percent > 0.9 ? _noCutoffFrequency : Mathf.Lerp(minCutoffFrequency, maxCutoffFrequency, percent);
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    private void SetSFX()
    {
        _sfxObj = new GameObject("AreaAmbienceSFX");
        _sfxObj.transform.position = transform.position;
        
        AudioSource audioSource = _sfxObj.AddComponent<AudioSource>();
        _audioLowPassFilter = _sfxObj.gameObject.AddComponent<AudioLowPassFilter>();
        _audioLowPassFilter.cutoffFrequency = _noCutoffFrequency;
        
        _audioSource = audioSource;

        audioSource.clip = sfx;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        // Mixing
        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
        }

        audioSource.Play();
    }
}
