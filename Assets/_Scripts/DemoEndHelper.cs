using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoEndHelper : MainMenuHelper
{

    public void OnPressDone()
    {
        SceneLoader.Load(SceneID.MainMenu);
    } 
}
