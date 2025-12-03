using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CatClock : MonoBehaviour
{
    [SerializeField] private GameObject eyes;
    [SerializeField] private float moveAmount;
    private Vector3 _initEyesPos;
    
    [SerializeField] private GameObject tail;
    [SerializeField] private float rotationAmount;
    private Vector3 _initTailRot;
    
    [SerializeField] private GameObject hand;
    [SerializeField] private int handTicks;
    [SerializeField] private int currentHandTicks;
    [SerializeField] private AudioClip[] handTickSFXs;
    [SerializeField] private Transform audioEmitter;
    
    [SerializeField] private float tickSpeed;
    [SerializeField] private float animationTickSpeed;

    [SerializeField] private float detectionRadius;
    [SerializeField] private SphereCollider detectionCollider;

    private float _timeOffset;

    private void Awake()
    {
        _initEyesPos = eyes.transform.localPosition;
        _initTailRot = tail.transform.localRotation.eulerAngles;
        currentHandTicks = 0;
        detectionCollider.radius = detectionRadius;
        
        _timeOffset = Random.Range(0f, 1f);
    }

    private bool _isProcessing;
    
    public void StartUp()
    {
        _isProcessing = true;
        StartCoroutine(FixedUpdateTick());
    }

    public void CleanUp()
    {
        _isProcessing = false;
    }
    
    private float _lastSinValue;
    private IEnumerator FixedUpdateTick()
    {
        while (_isProcessing)
        {
            float time = Time.time + _timeOffset;
            float logicTime = time * tickSpeed;
            float animationTime = time * animationTickSpeed;
        
            float sinValue = Mathf.Sin(logicTime);
            if (Mathf.Sign(sinValue) != Mathf.Sign(_lastSinValue)) Tick();
            _lastSinValue = sinValue;
        
            eyes.transform.localPosition = _initEyesPos + (Vector3.right * (Mathf.Sin(animationTime) * moveAmount));
            tail.transform.localRotation = Quaternion.Euler(_initTailRot + (Vector3.forward * (Mathf.Sin(animationTime) * rotationAmount)));
            
            yield return new WaitForFixedUpdate();
        }
    }

    private void Tick()
    {
        if (handTicks > 0)
        {
            currentHandTicks = (currentHandTicks + 1) % handTicks;
            Vector3 rotValue = Vector3.forward * ((float) currentHandTicks / handTicks * -360f);
            hand.transform.DOLocalRotate(rotValue, 0.3f).SetEase(Ease.OutBounce);
        }
        
        SFXObject sfxObj = ManagerSFX.Instance.PlaySFX(handTickSFXs[Random.Range(0, handTickSFXs.Length)], audioEmitter.position, 0.01f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f, maxDistance: detectionRadius);
        ManagerSFX.Instance.ApplyLowPassFilter(sfxObj);
        
    }
}
