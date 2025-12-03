using System;
using System.Collections.Generic;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuHelper : MonoBehaviour, IDataPersistence
{
    [SerializeField] private List<MainMenuButton> buttons = new List<MainMenuButton>();
    [SerializeField] private Transform[] buttonsParents;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color restrictedColor;
    [SerializeField] private AudioClip errorSFX;
    [SerializeField] private GameObject cleanUI;
    [SerializeField] private GameObject creditsUI;

    [SerializeField] private bool hasCompletedTutorial;

    public void LoadData(GameData data)
    {
        if (data.hasCompletedTutorial)
        {
            hasCompletedTutorial = data.hasCompletedTutorial;
        }
    }

    public void SaveData(ref GameData data)
    {
    }
    
    
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        HideCredits();
        Initialize();
    }
    
    private void Initialize()
    {
        int index = 0;

        foreach (Transform parent in buttonsParents)
        {
            foreach (Transform child in parent)
            {
                if (child.GetComponentInChildren<MainMenuButton>() != null)
                {
                    MainMenuButton button = child.GetComponentInChildren<MainMenuButton>();
                    buttons.Add(button);
                    button.id = index;
                }
                index++;
            }
        }

        index = 0;
        foreach (MainMenuButton button in buttons)
        {
            if (button.isRestricted)
            {
                button.backing.color = restrictedColor;
                button.textMesh.color = Color.black;
            }
            else
            {
                PointerExitButton(index);
            }
            index++;
        }
    }

    public void PressButton(int id)
    {
        MainMenuButton button = buttons[id];
        
        if (button.isRestricted)
        {
            ManagerSFX.Instance.PlayRawSFX(errorSFX, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            return;
        }

        ProcessButton(button.originalText);
    }
    
    public void ShowCredits()
    {
        creditsUI.SetActive(true);
        cleanUI.SetActive(false);
    }

    public void HideCredits()
    {
        creditsUI.SetActive(false);
        cleanUI.SetActive(true);
    }

    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private string nameYourCatForm = "https://forms.gle/Jy3pBnWvTMz4CD4N7";
    [SerializeField] private string feedbackForm = "https://forms.gle/HRYfvosX49YgHLRh8";

    public virtual void ProcessButton(string text)
    {
        text = text.ToLower();
        if (text == "start")
        {
            string name = hasCompletedTutorial ? SceneID.WaitingRoom.ToString() : SceneID.Tutorial.ToString();
            ManagerDataPersistence.Instance.SaveGame();
            SceneManager.LoadScene(name);
        }
        
        if (text == "settings")
        {
            pauseMenu.OnPausePerformed();
        }

        if (text == "credits")
        {
            ShowCredits();
        }

        if (text == "exit")
        {
            Application.Quit();
        }

        if (text == "feedback form")
        {
            Application.OpenURL(feedbackForm);
        }

        if (text == "name your own cat!")
        {
            Application.OpenURL(nameYourCatForm);
        }
    }

    [SerializeField] private AudioClip buttonEnterSFX;
    public void PointerEnterButton(int id)
    {
        MainMenuButton button = buttons[id];

        if (button.isRestricted) return;
            
        button.backing.color = activeColor;
        button.textAnimator.SetText("<shake a=0.3>" + button.originalText + "</shake>");
        button.textMesh.color = Color.black;
        ManagerSFX.Instance.PlayRawSFX(buttonEnterSFX, 0.3f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
    }

    public void PointerExitButton(int id)
    {
        MainMenuButton button = buttons[id];
        
        if (button.isRestricted) return;
        
        button.backing.color = inactiveColor;
        button.textAnimator.SetText(button.originalText);
        button.textMesh.color = Color.white;
    }
}
