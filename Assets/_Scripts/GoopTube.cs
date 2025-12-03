using UnityEngine;

public class GoopTube : MonoBehaviour
{
    [SerializeField] private AudioClip goopTubeAmbienceSFX;
    [SerializeField] private Transform audioEmitter;

    [SerializeField] private float detectionRadius;
    [SerializeField] private SphereCollider detectionCollider;

    private void Awake()
    {
        detectionCollider.radius = detectionRadius;
        _isProcessing = false;
    }

    private bool _isProcessing;
    private GameObject _goopTubeAmbienceSFXObj;
    public void StartUp()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        
        SFXObject sfxObj = ManagerSFX.Instance.PlaySFX(goopTubeAmbienceSFX, audioEmitter.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, maxDistance: detectionRadius, isLooping: true);
        _goopTubeAmbienceSFXObj = sfxObj.gameObject;
        ManagerSFX.Instance.ApplyLowPassFilter(sfxObj);
        
        sfxObj.audioSource.time = Random.Range(0, sfxObj.audioSource.clip.length);
    }

    public void CleanUp()
    {
        if (!_isProcessing) return;
        _isProcessing = false;

        Destroy(_goopTubeAmbienceSFXObj);
    }
}
