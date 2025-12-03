using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatCollectionTerminalAnimationHelper : MonoBehaviour
{
    [SerializeField] private CatCollectionTerminal catCollectionTerminal;
    
    public enum Expressions
    {
        Quota, Error, Unimpressed, Ibuprofen, Heart
    }

    public Expressions expression;
    [SerializeField] private GameObject quota, error, unimpressed, ibuprofen, heart;
    
    private Dictionary<Expressions, GameObject> _expressionsDictionary = new Dictionary<Expressions, GameObject>();

    private void Awake()
    {
        _expressionsDictionary[Expressions.Quota] = quota;
        _expressionsDictionary[Expressions.Error] = error;
        _expressionsDictionary[Expressions.Heart] = heart;
        _expressionsDictionary[Expressions.Unimpressed] = unimpressed;
        _expressionsDictionary[Expressions.Ibuprofen] = ibuprofen;

        ApplyQuota();
    }

    [SerializeField] private AudioClip[] lightFlickerSFXs;
    public void PlayExpression(Expressions expression)
    {
        if (this.expression != expression && ManagerSFX.Instance != null)
        {
            ManagerSFX.Instance.PlaySFX(lightFlickerSFXs[Random.Range(0, lightFlickerSFXs.Length)], transform.position, 0.3f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        }
        
        this.expression = expression;
        
        foreach (var kvp in _expressionsDictionary)
            kvp.Value.SetActive(kvp.Key == expression);
    }

    private Vector3 _originalTerminalPosition;
    [SerializeField] private float jitterStrength;
    [SerializeField] private GameObject terminalModel;
    [SerializeField] private AudioClip engineIdleSFX;
    private GameObject _engineIdleSFXObj;
    [SerializeField] private AudioClip engineIgnitionSFX;
    [SerializeField] private AudioClip engineStopSFX;
    [SerializeField] private float delayedEngineStopTime;
    
    public IEnumerator JitterTerminal()
    {
        yield return new WaitForSeconds(0.5f);
        ManagerSFX.Instance.PlaySFX(engineIgnitionSFX, transform.position, 0.02f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        yield return new WaitForSeconds(0.9f);
        
        _engineIdleSFXObj = ManagerSFX.Instance.PlaySFX(engineIdleSFX, transform.position, 0.025f, true, mixerGroup: ManagerAudioMixer.Instance.AMGSFX).gameObject;
        
        _originalTerminalPosition = terminalModel.transform.localPosition;

        bool isTurningOff = false;
        float jitterAmount = jitterStrength;
        float timer = delayedEngineStopTime;
        
        while (catCollectionTerminal.IsOn || timer > 0)
        {
            if (!isTurningOff && !catCollectionTerminal.IsOn)
            {
                if (_engineIdleSFXObj != null) Destroy(_engineIdleSFXObj);
                ManagerSFX.Instance.PlaySFX(engineStopSFX, transform.position, 0.01f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
                isTurningOff = true;
            }

            if (isTurningOff) timer -= Time.deltaTime;
        
            Vector3 jitter = new Vector3(
                Random.Range(-jitterAmount, jitterAmount),
                Random.Range(-jitterAmount, jitterAmount),
                Random.Range(-jitterAmount, jitterAmount)
            );

            terminalModel.transform.localPosition = _originalTerminalPosition + jitter;
            yield return new WaitForFixedUpdate();
        }
        
        terminalModel.transform.localPosition = _originalTerminalPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        ApplyQuota();
    }

    public void ApplyQuota()
    {
        if (_lockScreen != null) StopCoroutine(_lockScreen);
        PlayExpression(Expressions.Quota);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_lockScreen != null) StopCoroutine(_lockScreen);
        _lockScreen = StartCoroutine(ApplyLockScreen());
    }

    private Coroutine _lockScreen;
    private IEnumerator ApplyLockScreen()
    {
        while (catCollectionTerminal.IsOn)
        {
            yield return new WaitForFixedUpdate();
        }
        
        yield return new WaitForSeconds(5f);
        PlayRandomExpression();
    }

    public void PlayRandomExpression()
    {
        var keys = new List<Expressions>(_expressionsDictionary.Keys);
        Expressions randomExpression = keys[Random.Range(2, keys.Count)];
        PlayExpression(randomExpression);
    }
}
