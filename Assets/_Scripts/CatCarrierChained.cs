using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CatCarrierChained : MonoBehaviour
{
    [SerializeField] private GameObject pivotPoint;
    [SerializeField] private float rotationMin;
    [SerializeField] private float rotationMax;
    [SerializeField] private float timeMin;
    [SerializeField] private float timeMax;

    [SerializeField] private float detectionRadius;
    [SerializeField] private SphereCollider detectionCollider;

    private void Awake()
    {
        detectionCollider.radius = detectionRadius;
        _isProcessing = false;
    }

    private bool _isProcessing;
    private Tween _rotationTween;
    public void OnLoad()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        
        pivotPoint.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (_isProcessing)
        {
            _rotationTween = pivotPoint.transform.DOLocalRotate(new Vector3(0, GetRandomSign() * Random.Range(rotationMin, rotationMax), 0), Random.Range(timeMin, timeMax)).SetEase(Ease.InOutSine);
            yield return _rotationTween.WaitForCompletion();
        }
    }

    private int GetRandomSign()
    {
        return Random.value > 0.5 ? -1 : 1;
    }

    public void OnCull()
    {
        if (!_isProcessing) return;
        _isProcessing = false;
        
        _rotationTween?.Kill();
    }
}
