using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ButtonsSoundsManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        foreach(var button in GameObject.FindObjectsOfType<Button>(true))
        {
            button.AddComponent<ButtonSoundsManager>();
        }
    }
}
