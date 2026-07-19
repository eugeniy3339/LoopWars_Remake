using System;
using System.Collections.Generic;
using UnityEngine;

public static class MusicManager
{ 
    private static SoundScript curMusicScript;
    public static SoundScriptableObject curMusic { get; private set; }

    private static List<SoundScriptableObject> curMusicList;

    public static event Action<SoundScriptableObject> onMusicStarted;
    public static event Action onMusicEnded;

    private static void OnMusicStopped(SoundScript soundScript)
    {
        StartMusicWithLoop();
    }

    public static void StopCurMusic()
    {
        if (curMusicScript == null) return;
        curMusicScript.onSoundFinished -= OnMusicStopped;
        curMusicScript.StopSound(2f);
        onMusicEnded?.Invoke();
    }

    public static void StartMusicWithLoop()
    {
        if (curMusicList != null)
            StartMusicWithLoop(GetRandomMusic());
    }

    public static void StartMusicWithoutLoop()
    {
        if (curMusicList != null)
            StartMusicWithoutLoop(GetRandomMusic());
    }

    public static void StartMusicWithLoop(SoundScriptableObject musicSound)
    {
        StopCurMusic();
        StartMusicWithoutLoop();
        curMusicScript.onSoundFinished += OnMusicStopped;
    }

    public static void StartMusicWithoutLoop(SoundScriptableObject musicSound)
    {
        StopCurMusic();
        curMusic = musicSound;
        curMusicScript = SoundsManager.StartSound(musicSound, null, Vector2.zero, 2f, true);
        onMusicStarted?.Invoke(musicSound);
    }


    private static SoundScriptableObject GetRandomMusic()
    {
        return curMusicList[UnityEngine.Random.Range(0, curMusicList.Count)];
    }



    public static void SetCurMusicList(SoundsListScriptableObject musicList)
    {
        curMusicList = musicList.sounds;
    }

    public static void SetCurMusicList(MusicMapsPairsListScriptableObject musicMapsPairsList)
    {
        curMusicList = new List<SoundScriptableObject>();
        foreach (var musicMapsPair in musicMapsPairsList.musicMapsPairs)
        {
            curMusicList.Add(musicMapsPair.music);
        }
    }

    public static void SetCurMusicList(SoundScriptableObject music)
    {
        curMusicList = new List<SoundScriptableObject> { music };
    }
}
