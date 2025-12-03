using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour, IDataPersistence
{
    [SerializeField] protected List<GameObject> toggleGroup;
    [SerializeField] protected GameObject mainCamera;
    [SerializeField] protected GameObject returnToMainMenuButton;
    [SerializeField] private Canvas underlayCanvas;
    
    public virtual void Awake()
    {
        mainCamera = Camera.main.gameObject;
        underlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        underlayCanvas.worldCamera = Camera.main;
        
        foreach (GameObject toggle in toggleGroup)
        {
            toggle.SetActive(false);
        }
    }

    public void OnPressReturnToMainMenu()
    {
        Time.timeScale = _initTimeScale;
        SceneLoader.Load(SceneID.MainMenu);
    }

    [field: SerializeField] public bool IsPaused { get; protected set; }
    protected float _initTimeScale = 1f;
    protected CinemachineBlendDefinition _oldBlend;
    [SerializeField] protected AudioClip pauseSound;
    public virtual void OnPausePerformed()
    {
        IsPaused = !IsPaused;
        if (IsPaused)
        {
            _initTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = _initTimeScale;
        }
        
        mainCamera.SetActive(!IsPaused);
        foreach (GameObject toggle in toggleGroup)
        {
            toggle.SetActive(IsPaused);
        }
        Cursor.visible = IsPaused;
        Cursor.lockState = IsPaused ? CursorLockMode.Confined : CursorLockMode.Locked;
        if (ManagerPlayer.instance != null) ManagerPlayer.instance.PlayerUIHelper.SetVisibility(!IsPaused, ManagerPlayer.instance.PlayerUIHelper.All, "pauseMenu");
        
        ManagerSFX.Instance.PlayRawSFX(pauseSound, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, 1 + (IsPaused ? 0.3f : 0f), false);
    }
    
    [SerializeField] private AudioClip generalUIHover;
    [SerializeField] private float currentSFXSpacing = 0;
    [SerializeField] private float sfxSpacingAmount = 10;
    [SerializeField] private GameObject masterSlider;
    [SerializeField] private GameObject musicSlider;
    [SerializeField] private GameObject sfxSlider;
    public void LoadData(GameData data)
    {
        masterSlider.GetComponent<Slider>().value = data.settings.masterVolume;
        musicSlider.GetComponent<Slider>().value = data.settings.musicVolume;
        sfxSlider.GetComponent<Slider>().value = data.settings.sfxVolume;
    }

    public void SaveData(ref GameData data)
    {
        data.settings.masterVolume = masterSlider.GetComponent<Slider>().value;
        data.settings.musicVolume = musicSlider.GetComponent<Slider>().value;
        data.settings.sfxVolume = sfxSlider.GetComponent<Slider>().value;
    }

    public void OnSliderChanged(GameObject sliderObj)
    {
        if (currentSFXSpacing <= 0)
        {
            ManagerSFX.Instance.PlayRawSFX(generalUIHover, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, 0.3f, true);
            currentSFXSpacing = sfxSpacingAmount;
        } else
        {
            currentSFXSpacing--;
        }
        
        if (sliderObj == masterSlider)
        {
            ManagerAudioMixer.Instance.SetMasterVolume(masterSlider.GetComponent<Slider>().value);
        } else if (sliderObj == musicSlider)
        {
            ManagerAudioMixer.Instance.SetMusicVolume(musicSlider.GetComponent<Slider>().value);
        } else if (sliderObj == sfxSlider)
        {
            ManagerAudioMixer.Instance.SetSFXVolume(sfxSlider.GetComponent<Slider>().value);
        }
    }

    public void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
