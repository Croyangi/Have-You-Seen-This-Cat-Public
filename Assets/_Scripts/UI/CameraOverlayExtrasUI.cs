using System.Collections;
using UnityEngine;

public class CameraOverlayExtrasUI : MonoBehaviour, IUI
{
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject dot;
    [SerializeField] private GameObject batterySegment;
    
    public bool IsVisible { get; set; }
    
    private void OnDestroy()
    {
        if (_recordingDotBlinking != null) StopCoroutine(_recordingDotBlinking);
        if (_batterySegmentBlinking != null) StopCoroutine(_batterySegmentBlinking);
        if (!gameObject.activeSelf) return;
        _recordingDotBlinking = StartCoroutine(RecordingDotBlinking());
        _batterySegmentBlinking = StartCoroutine(BatterySegmentBlinking());
    }

    public void OnVisibilityChanged()
    {
        ui.SetActive(IsVisible);
        
        if (IsVisible)
        {
            if (_recordingDotBlinking != null) StopCoroutine(_recordingDotBlinking);
            _recordingDotBlinking = StartCoroutine(RecordingDotBlinking());
            
            if (_batterySegmentBlinking != null) StopCoroutine(_batterySegmentBlinking);
            _batterySegmentBlinking = StartCoroutine(BatterySegmentBlinking());
        }
    }
    
    [SerializeField] private float recordingDotBlinkingInterval;
    private Coroutine _recordingDotBlinking;
    private IEnumerator RecordingDotBlinking()
    {
        while (IsVisible)
        {
            dot.SetActive(!dot.activeSelf);
            yield return new WaitForSeconds(recordingDotBlinkingInterval);
        }
    }
    
    [SerializeField] private float batterySegmentBlinkingInterval;
    private Coroutine _batterySegmentBlinking;
    private IEnumerator BatterySegmentBlinking()
    {
        while (IsVisible)
        {
            for (int i = 0; i < 3; i++)
            {
                batterySegment.SetActive(!batterySegment.activeSelf);
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            }
            
            batterySegment.SetActive(true);
            yield return new WaitForSeconds(batterySegmentBlinkingInterval);
        }
    }
}
