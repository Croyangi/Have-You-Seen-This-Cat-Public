using UnityEngine;

public class MainMenuPauseMenu : PauseMenu
{
    [SerializeField] private GameObject cleanUI;
    
    public override void Awake()
    {
        base.Awake();
        returnToMainMenuButton.SetActive(false);
    }
    
    public override void OnPausePerformed()
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
        
        cleanUI.SetActive(!IsPaused);
        mainCamera.SetActive(!IsPaused);
        foreach (GameObject toggle in toggleGroup)
        {
            toggle.SetActive(IsPaused);
        }
        
        ManagerSFX.Instance.PlayRawSFX(pauseSound, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, 1 + (IsPaused ? 0.3f : 0f), false);
    }
}
