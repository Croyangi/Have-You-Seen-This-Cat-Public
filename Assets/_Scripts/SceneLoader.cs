using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneID
{
    Intro,
    MainMenu,
    Tutorial,
    WaitingRoom,
    Gameplay,
    DemoEnd
}

public static class SceneLoader
{
    public static SceneID Current;
    public static SceneID Previous;
    
    public static void Load(SceneID scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        string sceneName = scene.ToString();
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Previous = Current;
            Current = scene;
            SceneManager.LoadScene(sceneName, mode);
        }
        else
        {
            Debug.LogError($"Scene {sceneName} not found or not in build settings.");
        }
    }
}