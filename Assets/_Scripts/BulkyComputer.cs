using DG.Tweening;
using UnityEngine;

public class BulkyComputer : MonoBehaviour
{
   [SerializeField] private GameObject[] wheels;
   [SerializeField] private float rotationSpeed;

    [SerializeField] private AudioClip bulkyComputerAmbienceSFX;
    [SerializeField] private Transform audioEmitter;

    [SerializeField] private float detectionRadius;
    [SerializeField] private SphereCollider detectionCollider;

    private void Awake()
    {
        detectionCollider.radius = detectionRadius;
        _isProcessing = false;
    }

    private bool _isProcessing;
    private GameObject _bulkyComputerAmbienceSFXObj;
    private Tween _wheelSpinTween0;
    private Tween _wheelSpinTween1;
    public void StartUp()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        
        SFXObject sfxObj = ManagerSFX.Instance.PlaySFX(bulkyComputerAmbienceSFX, audioEmitter.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, maxDistance: detectionRadius, isLooping: true);
        _bulkyComputerAmbienceSFXObj = sfxObj.gameObject;
        ManagerSFX.Instance.ApplyLowPassFilter(sfxObj);
        
        sfxObj.audioSource.time = Random.Range(0, sfxObj.audioSource.clip.length);
        
        // One full rotation every (360 / rotationSpeed) seconds
        float duration = 360f / rotationSpeed;
        _wheelSpinTween1 = wheels[0].transform
            .DOLocalRotate(new Vector3(0f, 0f, 360f), duration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
        
        _wheelSpinTween1 = wheels[1].transform
            .DOLocalRotate(new Vector3(0f, 0f, -360f), duration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

    public void CleanUp()
    {
        if (!_isProcessing) return;
        _isProcessing = false;
        
        _wheelSpinTween0?.Kill();
        _wheelSpinTween1?.Kill();
        Destroy(_bulkyComputerAmbienceSFXObj);
    }
}
