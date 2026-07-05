using UnityEngine;

public class MonoCommandsStarter : MonoBehaviour
{
    public static MonoCommandsStarter Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
