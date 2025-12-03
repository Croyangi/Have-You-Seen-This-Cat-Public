using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ManagerSFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<SFXObject> sfxObjects;
    [SerializeField] private Transform listener;
    //public LayerMask occlusionMask;
    [SerializeField] private float checkInterval = 0.1f;
    //public float muffledCutoff = 800f;
    //public float normalCutoff = 22000f;

    [SerializeField] private float minCutoffFrequency;
    [SerializeField] private float maxCutoffFrequency;
    [SerializeField] private float occlusionDistanceMultiplier = 1f;
    
    [SerializeField] private GameObject SFXObject;
    [SerializeField] private GameObject rawSFXObject;
    [SerializeField] private AudioSource ambienceSFXSource;

    public static ManagerSFX Instance { get; private set; }

    private void Start()
    {
        listener = Camera.main.transform;
        InvokeRepeating(nameof(CheckForOcclusion), 0f, checkInterval);
    }

    private void CheckForOcclusion()
    {
        for (int i = sfxObjects.Count - 1; i >= 0; i--)
        {
            SFXObject sfxObj = sfxObjects[i];
            if (sfxObj == null)
            {
                sfxObjects.Remove(sfxObj);
                continue;
            }
            
            AudioSource source = sfxObj.audioSource;
            if (sfxObj.audioLowPassFilter == null) continue;
            if (!source.isPlaying) continue;
            if (Vector3.Distance(listener.position, source.transform.position) > source.maxDistance) continue;
            
            DetectOcclusion(sfxObj);
        }
    }

    private void DetectOcclusion(SFXObject sfxObj)
    {
        Vector3 origin = sfxObj.audioSource.transform.position;
        Vector3 end = listener.position;
        Vector3 direction = (end - origin).normalized;
        float range = Vector3.Distance(origin, end);
        Ray ray = new Ray(origin, direction);
        
        if (Physics.Raycast(ray, out RaycastHit hit, range, LayerUtility.Environment))
        {
            sfxObj.isOccluded = true;
            Debug.DrawRay(origin, direction * range, Color.red); // Occluded
        }
        else
        {
            sfxObj.isOccluded = false;
            Debug.DrawRay(origin, direction * range, Color.green); // Clear
        }
        
        SetOcclusion(sfxObj);
    }


    private void SetOcclusion(SFXObject sfxObj)
    {
        AudioSource source = sfxObj.audioSource;
        
        if (sfxObj.isOccluded)
        {
            source.volume = sfxObj.originalVolume * 0.7f;
            
            float dist = Vector3.Distance(listener.position, source.transform.position) * occlusionDistanceMultiplier;
            float mult = Mathf.Clamp01(dist / source.maxDistance);

            sfxObj.audioLowPassFilter.cutoffFrequency = Mathf.Lerp(maxCutoffFrequency, minCutoffFrequency, mult);
        }
        else
        {
            source.volume = sfxObj.originalVolume;
            sfxObj.audioLowPassFilter.cutoffFrequency = 22000f;
        }

    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one SFX Manager in the scene.");
        }
        Instance = this;
    }

    private int RandomSign()
    {
        return Random.value < .5 ? 1 : -1;
    }

    public void AddSFXObject(SFXObject sfxObject)
    {
        sfxObjects.Add(sfxObject);
    }

    public SFXObject PlaySFX(AudioClip audioClip, Vector3 pos, float volume = 1f, bool isLooping = false,
        AudioMixerGroup mixerGroup = null, Transform parent = null, float pitchShift = 0f, bool isRandomPitch = true,
        float maxDistance = 25f, bool timeScaled = true)
    {
        GameObject obj = Instantiate(SFXObject, pos, Quaternion.identity);
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        SFXObject sfxObj = obj.GetComponent<SFXObject>();
        sfxObjects.Add(sfxObj);
        
        // Settings
        sfxObj.originalVolume = volume;
        
        if (parent != null)
        {
            obj.transform.parent = parent;
        }

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = isLooping;
        audioSource.maxDistance = maxDistance;
        audioSource.ignoreListenerPause = !timeScaled;
        
        // Pitch Shift
        if (pitchShift != 0)
        {
            float pitch = pitchShift;
            if (isRandomPitch)
            {
                pitch = audioSource.pitch + (RandomSign() * Random.Range(0, pitchShift));
            }
            audioSource.pitch = pitch;
        }

        // Mixing
        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
        }

        audioSource.Play();

        // Looping
        if (isLooping == false)
        {
            float clipLength = audioSource.clip.length;
            Destroy(audioSource.gameObject, clipLength / audioSource.pitch);
        }
        
        return sfxObj;
    }
    
    public AudioSource PlayAmbienceSFX(AudioClip audioClip, float volume = 1f)
    {
        ambienceSFXSource.clip = audioClip;
        ambienceSFXSource.volume = volume;
        ambienceSFXSource.loop = true;
        ambienceSFXSource.outputAudioMixerGroup = ManagerAudioMixer.Instance.AMGSFX;
        ambienceSFXSource.Play();

        return ambienceSFXSource;
    }

    public void StopAmbienceSFX()
    {
        ambienceSFXSource.Stop();
    }
    
    public GameObject PlayRawSFX(AudioClip audioClip, float volume = 1f, bool isLooping = false, AudioMixerGroup mixerGroup = null, float pitchShift = 0f, bool isRandomPitch = true)
    {
        GameObject obj = Instantiate(rawSFXObject, Vector3.zero, Quaternion.identity);
        AudioSource audioSource = obj.GetComponent<AudioSource>();

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = isLooping;
        
        // Pitch Shift
        if (pitchShift != 0)
        {
            float pitch = pitchShift;
            if (isRandomPitch)
            {
                pitch = audioSource.pitch + (RandomSign() * Random.Range(0, pitchShift));
            }
            audioSource.pitch = pitch;
        }

        // Mixing
        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
        }

        audioSource.Play();

        // Looping
        if (isLooping == false)
        {
            float clipLength = audioSource.clip.length;
            Destroy(audioSource.gameObject, clipLength / audioSource.pitch);
        }
        
        return audioSource.gameObject;
    }

    public void ApplyLowPassFilter(SFXObject obj, float cutoffFrequency = 1000, float lowpassResonanceQ = 1)
    {
        AudioLowPassFilter filter = obj.gameObject.AddComponent<AudioLowPassFilter>();
        filter.cutoffFrequency = cutoffFrequency;
        filter.lowpassResonanceQ = lowpassResonanceQ;
        obj.audioLowPassFilter = filter;
    }
}