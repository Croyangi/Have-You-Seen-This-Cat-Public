using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCameraHelper : MonoBehaviour, IProcessable
{
    [Header("References")]
    [SerializeField] private Transform head;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float verticalClampAngle = 70f; // Max angle the camera can look up or down
    [SerializeField] private float xRotation = 0f; // Stores current vertical rotation
    
    //[SerializeField] private ;
    [SerializeField] private Vector3 swayMultiplier;
    [SerializeField] private Vector3 swaySpeed;
    
    [field: SerializeField] public bool IsProcessing { get; set;}

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (IsProcessing)
        {
            CameraLook();
        }
    }

    private void CameraLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.fixedDeltaTime;

        // Rotate the head horizontally
        head.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically and clamp it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClampAngle, verticalClampAngle);

        // Apply rotation to the camera
        head.localRotation = Quaternion.Euler(xRotation, head.localRotation.eulerAngles.y, 0f);
        
        Vector3 offset = new Vector3(
            Mathf.Sin(Time.time * swaySpeed.x) * swayMultiplier.x,
            Mathf.Cos(Time.time * swaySpeed.y) * swayMultiplier.y,
            0
        );
    }

    private CinemachineBasicMultiChannelPerlin noise;
    public void SetCameraShake(NoiseSettings noiseProfile, float amplitude, float frequency)
    {
        if (noise == null)
        {
            Debug.LogWarning("No BasicMultiChannelPerlin component found.");
            return;
        }

        noise.m_NoiseProfile = noiseProfile;
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;
    }
    
    public class CameraShakeItem
    {
        public NoiseSettings Noise;
        public float Amplitude;
        public float Frequency;
        public float Duration;
        public Vector3 Source;
        public float Range;

        public CameraShakeItem(NoiseSettings noise, float amplitude, float frequency, float duration, Vector3 source, float range)
        {
            Noise = noise;
            Amplitude = amplitude;
            Frequency = frequency;
            Duration = duration;
            Source = source;
            Range = range;
        }
    }
    
    [SerializeField] private List<CameraShakeItem> _cameraShakeItems = new List<CameraShakeItem>();
    private CameraShakeItem _currentShakeItem;

    public void QueueCameraShake(NoiseSettings noiseProfile, float amplitude, float frequency, float duration, float delay = 0f, Vector3 source = default, float range = -1)
    {
        CameraShakeItem shakeItem = new CameraShakeItem(noiseProfile, amplitude, frequency, duration, source, range);
        if (delay > 0)
        {
            StartCoroutine(DelayedQueue(delay, shakeItem));
            return;
        }
        _cameraShakeItems.Add(shakeItem);
        SetCurrentShakeItem();
    }

    private IEnumerator DelayedQueue(float time, CameraShakeItem shakeItem)
    {
        yield return new WaitForSeconds(time);
        _cameraShakeItems.Add(shakeItem);
        SetCurrentShakeItem();
    }

    private void SetCurrentShakeItem()
    {
        _currentShakeItem = null;
        
        if (_cameraShakeItems.Count == 0) return;
        _currentShakeItem = _cameraShakeItems[0];

        foreach (var csi in _cameraShakeItems)
        {
            if (csi.Duration <= 0) continue;

            float currentAmp = csi.Amplitude;
            float highestAmp = _currentShakeItem.Amplitude;
            if (csi.Range > 0)
            {
                currentAmp *= Mathf.Clamp01(1 - Vector3.Distance(head.position, csi.Source) / csi.Range);
            }

            if (_currentShakeItem.Range > 0)
            {
                highestAmp *= Mathf.Clamp01(1 - Vector3.Distance(head.position, _currentShakeItem.Source) / _currentShakeItem.Range);
            }
            
            if (currentAmp > highestAmp)
            {
                _currentShakeItem = csi;
            }
        }
    }

    public NoiseSettings noiseSetting;
    //
    // [ContextMenu("Generate Shake")]
    // public void Test()
    // {
    //     StartCoroutine(TestRun());
    // }
    //
    // private IEnumerator TestRun()
    // {
    //     while (true)
    //     {
    //         //QueueCameraShake(noiseSetting, amp, frequency, duration, source: transform.position, range: 25f);
    //         yield return new WaitForSeconds(0.5f);
    //     }
    // }


    private void FixedUpdate()
    {
        if (_currentShakeItem != null)
        {
            // Decrease duration first
            _currentShakeItem.Duration -= Time.fixedDeltaTime;
            if (_currentShakeItem.Duration <= 0)
            {
                _cameraShakeItems.Remove(_currentShakeItem);
                _currentShakeItem = null;
                if (_cameraShakeItems.Count == 0) 
                    ClearCameraShake();
            }
            
            SetCurrentShakeItem();
            
            if (_currentShakeItem != null)
            {
                CameraShakeItem csi = _currentShakeItem;
                float amplitude = csi.Amplitude;
                if (csi.Range > 0) amplitude *= Mathf.Clamp01(1 - Vector3.Distance(head.position, csi.Source) / csi.Range);
                
                SetCameraShake(csi.Noise, amplitude, csi.Frequency);
            }
        }
    }

    public void ClearCameraShake()
    {
        if (noise == null) return;

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
        noise.m_NoiseProfile = null;
    }
}
