using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerVFXHelper : MonoBehaviour
{
    [SerializeField] private float scareTimer;
    [SerializeField] private float scareDefaultTime;
    [SerializeField] private float scareFadeInTime;
    [SerializeField] private float scareFadeOutTime;
    [SerializeField] private Volume scareEffectVolume;
    [SerializeField] private float scareFatigueTimeThreshold;
    [SerializeField] private PlayerMovementHelper playerMovementHelper;
    [SerializeField] private Image blackOutImage;

    private void Awake()
    {
        PlayerFadeToAwake(0f); 
    }
    
    private void Start()
    {
        CleanUp();
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        scareTimer = 0f;
        scareEffectVolume.weight = 0;
        if (_scareSFXSource != null) Destroy(_scareSFXSource.gameObject);
        masterMixer.SetFloat("MasterLowPass", restingLowPassCutoff);
    }
    
    public void Scare(float time = -1f)
    {
        if (scareTimer <= 0)
        {
            _scareSFXSource = ManagerSFX.Instance.PlayRawSFX(scareSFX, 0.1f, true, mixerGroup: ManagerAudioMixer.Instance.AMGSFX).GetComponent<AudioSource>();
        }
        
        scareTimer = (int) time == -1 ? scareDefaultTime : time;
        if (scareTimer >= scareFatigueTimeThreshold) playerMovementHelper.Stamina = 0f;
        
        StopAllCoroutines();
        StartCoroutine(ScareUpdate());
    }

    [SerializeField] private float maxScareSFX;
    [SerializeField] private AudioClip scareSFX;
    private AudioSource _scareSFXSource;
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private float restingLowPassCutoff;
    [SerializeField] private float activeLowPassCutoff;
    
    private IEnumerator ScareUpdate()
    {
        float startTime = scareTimer;

        while (scareTimer > 0f)
        {
            scareTimer -= Time.deltaTime;

            if (scareTimer < scareFadeOutTime)
            {
                float t = Mathf.Clamp01(scareTimer / scareFadeOutTime);
                scareEffectVolume.weight = t;
                _scareSFXSource.volume = t * maxScareSFX;
                masterMixer.SetFloat("SFXLowPass", Mathf.Lerp(restingLowPassCutoff, activeLowPassCutoff, t));

            }
            else
            {
                float elapsed = startTime - scareTimer;
                float t = Mathf.Clamp01(elapsed / scareFadeInTime);
                scareEffectVolume.weight = t;
                _scareSFXSource.volume = t * maxScareSFX;
                masterMixer.SetFloat("SFXLowPass", Mathf.Lerp(restingLowPassCutoff, activeLowPassCutoff, t));

            }

            yield return null;
        }

        scareEffectVolume.weight = 0;
        if (_scareSFXSource.gameObject != null) Destroy(_scareSFXSource.gameObject);
    }

    
    [SerializeField] private float fadeDuration = 3f;

    [ContextMenu("Fullscreen Color")]
    public void PlayerFadeToAwake(float time = -1f)
    {
        time = (int) time == -1 ? fadeDuration : time;
        StartCoroutine(FadeColor(Color.black, Color.clear, time));
        StartCoroutine(FadeLowPass(0f, restingLowPassCutoff, time));
    }
    
    public void PlayerFadeToBlack(float time = -1f)
    {
        time = (int) time == -1 ? fadeDuration : time;
        StartCoroutine(FadeColor(Color.clear, Color.black, time));
        StartCoroutine(FadeLowPass(restingLowPassCutoff, 0f, time));
    }

    private IEnumerator FadeColor(Color from, Color to, float duration)
    {
        if (duration > 0)
        {
            float t = 0f;
            blackOutImage.color = from;
            while (t < duration)
            {
                t += Time.deltaTime;
                Color lerped = Color.Lerp(from, to, t / duration);
                blackOutImage.color = lerped;
                yield return null;
            }
        }
        
        blackOutImage.color = to;
    }

    private IEnumerator FadeLowPass(float from, float to, float duration)
    {
        if (duration > 0)
        {
            float t = 0f;
            masterMixer.SetFloat("MasterLowPass", from);
            while (t < duration)
            {
                t += Time.deltaTime;
                float lerped = Mathf.Lerp(from, to, t / duration);
                masterMixer.SetFloat("MasterLowPass", lerped);
                yield return null;
            }
        }
        
        masterMixer.SetFloat("MasterLowPass", to);
    }
    
}
