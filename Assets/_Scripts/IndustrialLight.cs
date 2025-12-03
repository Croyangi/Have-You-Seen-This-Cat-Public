using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[SelectionBase]
public class IndustrialLight : MonoBehaviour
{
    [SerializeField] private List<GameObject> lightsOn = new List<GameObject>();
    [SerializeField] private List<GameObject> lightsOff = new List<GameObject>();

    [SerializeField] private bool isOn;

    [SerializeField] private AudioClip[] lightFlickerSFXs;
    [SerializeField] private AudioClip lightFlickerEndSFX;
    private void Awake()
    {
        isOn = lightsOn[0].activeSelf;
    }
    
    public void ToggleLights(bool state)
    {
        foreach (GameObject l in lightsOn) 
        {
            l.SetActive(state);
        }

        foreach (GameObject l in lightsOff) 
        {
            l.SetActive(!state);
        }

        isOn = state;
    }

    public void Flicker(bool state)
    {
        StartCoroutine(OnFlicker(state));
    }
    
    private IEnumerator OnFlicker(bool state)
    {
        ToggleLights(!isOn);
        ManagerSFX.Instance.PlaySFX(lightFlickerSFXs[Random.Range(0, lightFlickerSFXs.Length)], transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        yield return new WaitForSeconds(0.3f);
        ToggleLights(!isOn);
        ManagerSFX.Instance.PlaySFX(lightFlickerSFXs[Random.Range(0, lightFlickerSFXs.Length)], transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < 3; i++)
        {
            ToggleLights(!isOn);
            ManagerSFX.Instance.PlaySFX(lightFlickerSFXs[Random.Range(0, lightFlickerSFXs.Length)], transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            ToggleLights(!isOn);
            ManagerSFX.Instance.PlaySFX(lightFlickerSFXs[Random.Range(0, lightFlickerSFXs.Length)], transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }

        ToggleLights(!state);
        ManagerSFX.Instance.PlaySFX(lightFlickerSFXs[Random.Range(0, lightFlickerSFXs.Length)], transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        yield return new WaitForSeconds(0.3f);
        ToggleLights(state);
        ManagerSFX.Instance.PlaySFX(lightFlickerEndSFX, transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
