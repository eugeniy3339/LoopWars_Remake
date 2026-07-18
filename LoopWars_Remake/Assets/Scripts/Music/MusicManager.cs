using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private SoundScriptableObject mainMenuMusic;

    private const int gameSceneIndex = 1;

    private SoundScript curMusicScript;
    private bool mainMenu = true;

    public SoundScriptableObject curMusic { get; private set; }

    public static event Action<SoundScriptableObject> onMusicStarted;
    public static event Action onMusicEnded;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        OnMainMenuLoaded();
    }

    private void OnGameSceneLoaded()
    {
        mainMenu = false;
        StartNextMusic(MusicListScriptableObject.Instance.GetRandomMusic().music);
    }

    private void OnMainMenuLoaded()
    {
        mainMenu = true;
        StartNextMusic(mainMenuMusic);
    }

    private void OnMusicStopped(SoundScript soundScript)
    {
        StartNextMusic(MusicListScriptableObject.Instance.GetRandomMusic().music);
    }

    private void StopPreviousMusic()
    {
        if (curMusicScript == null) return;
        curMusicScript.onSoundFinished -= OnMusicStopped;
        curMusicScript.StopSound(2f);
        onMusicEnded?.Invoke();
    }

    private void StartNextMusic(SoundScriptableObject musicSound)
    {
        StopPreviousMusic();
        curMusic = musicSound;
        curMusicScript = SoundsManager.StartSound(musicSound, null, Vector2.zero, 2f, true);
        curMusicScript.onSoundFinished += OnMusicStopped;
        onMusicStarted?.Invoke(musicSound);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == gameSceneIndex && !mainMenu)
            OnGameSceneLoaded();
        else
            OnMainMenuLoaded();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
