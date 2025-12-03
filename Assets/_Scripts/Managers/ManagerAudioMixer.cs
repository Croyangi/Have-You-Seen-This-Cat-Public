using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ManagerAudioMixer : MonoBehaviour, IDataPersistence
{
    [Header("References")]
    [SerializeField] private AudioMixer audioMixer;
    [field: SerializeField] public AudioMixerGroup AMGMaster { get; private set; }
    [field: SerializeField] public AudioMixerGroup AMGSFX { get; private set; }
    [field: SerializeField] public AudioMixerGroup AMGMusic { get; private set; }
    
    private const string _masterVolume = "MasterVolume";
    private const string _musicVolume = "SFXVolume";
    private const string _sfxVolume = "MusicVolume";

    public static ManagerAudioMixer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        Instance = this;
    }

    public void LoadData(GameData data)
    {
        SetMasterVolume(data.settings.masterVolume);
        SetMusicVolume(data.settings.musicVolume);
        SetSFXVolume(data.settings.sfxVolume);
    }

    public void SaveData(ref GameData data)
    {
    }
    
    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat(_masterVolume, Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat(_musicVolume, Mathf.Log10(level) * 20f);
    }

    public void SetSFXVolume(float level)
    {
        audioMixer.SetFloat(_sfxVolume , Mathf.Log10(level) * 20f);
    }
}
