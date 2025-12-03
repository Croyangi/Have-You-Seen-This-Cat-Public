using System.Collections;
using UnityEngine;

public class RecordingDotUI : MonoBehaviour, IUI
{
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject dot;
    
    public bool IsVisible { get; set; }
    

    private void OnDestroy()
    {
        if (_recordingDotBlinking != null) StopCoroutine(_recordingDotBlinking);
        if (!gameObject.activeSelf) return;
        _recordingDotBlinking = StartCoroutine(RecordingDotBlinking());
    }

    public void OnVisibilityChanged()
    {
        ui.SetActive(IsVisible);
        
        if (IsVisible)
        {
            if (_recordingDotBlinking != null) StopCoroutine(_recordingDotBlinking);
            _recordingDotBlinking = StartCoroutine(RecordingDotBlinking());
        }
    }
    
    [SerializeField] private float recordingDotBlinkingInterval;
    private Coroutine _recordingDotBlinking;
    private IEnumerator RecordingDotBlinking()
    {
        while (IsVisible)
        {
            dot.SetActive(false);
            yield return new WaitForSeconds(recordingDotBlinkingInterval);
            dot.SetActive(true);
            yield return new WaitForSeconds(recordingDotBlinkingInterval);
        }
    }
}
