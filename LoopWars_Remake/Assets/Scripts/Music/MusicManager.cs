using System;
using Unity.Netcode;
using UnityEngine;

public class MusicManager : NetworkBehaviour
{
    public static MusicManager Instance { get; private set; }

    private SoundScript curMusicScript;
    public SoundScriptableObject curMusic { get; private set; }

    public static event Action<SoundScriptableObject> onMusicStarted;
    public static event Action onMusicEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void OnAllPlayersLoaded()
    {
        StartNextMusic();
    }

    private void OnMusicStopped(SoundScript soundScript)
    {
        StartNextMusic();
    }

    private void StartNextMusic()
    {
        MusicListScriptableObject.SoundMapPair musicToPlay = MusicListScriptableObject.Instance.GetRandomMusic();
        if (IsSpawned && IsServer)
            StartNextMusicRpc(MusicListScriptableObject.Instance.musicMapsPairs.IndexOf(musicToPlay));
        else
            StartNextMusic(musicToPlay.music);
    }

    [Rpc(SendTo.Everyone)]
    private void StartNextMusicRpc(int musicIndex)
    {
        StartNextMusic(MusicListScriptableObject.Instance.musicMapsPairs[musicIndex].music);
    }

    private void StopCurMusic()
    {
        if (curMusicScript == null) return;
        curMusicScript.onSoundFinished -= OnMusicStopped;
        curMusicScript.StopSound(2f);
        onMusicEnded?.Invoke();
    }

    private void StartNextMusic(SoundScriptableObject musicSound)
    {
        StopCurMusic();
        curMusic = musicSound;
        curMusicScript = SoundsManager.StartSound(musicSound, null, Vector2.zero, 2f, true);
        curMusicScript.onSoundFinished += OnMusicStopped;
        onMusicStarted?.Invoke(musicSound);
    }

    private void OnEnable()
    {
        GameManager.onAllThePlayersLoaded += OnAllPlayersLoaded;
    }

    private void OnDisable()
    {
        GameManager.onAllThePlayersLoaded -= OnAllPlayersLoaded;
        StopCurMusic();
    }
}
