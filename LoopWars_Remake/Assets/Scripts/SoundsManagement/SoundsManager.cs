using System;
using System.Collections.Generic;
using UnityEngine;

public static class SoundsManager
{
    private static GameObject sop;
    private static GameObject soundObjectPrefab
    {
        get 
        {
            if (sop == null)
            {
                sop = Resources.Load<GameObject>("SoundObject");
            }

            return sop; 
        }
    }

    public static float globalVolume {
        get {
            if (!PlayerPrefs.HasKey("Volume"))
            {
                globalVolume = 1f;
            }
            return PlayerPrefs.GetFloat("Volume");
        }
        set
        {
            PlayerPrefs.SetFloat("Volume", value);
            PlayerPrefs.Save();
            if (onGlobalVolumeChanged != null)
                onGlobalVolumeChanged(value);
        }
    }

    public static Action<float> onGlobalVolumeChanged;

    private static List<SoundScript> _spawnedSounds = new List<SoundScript>();

    public static SoundScript StartSound(SoundsListScriptableObject soundList, Transform parentTransform, bool dontDestroyOnLoad = false)
    {
        return StartSound(soundList, parentTransform, Vector2.zero, out SoundScriptableObject soundScriptableObject, dontDestroyOnLoad);
    }

    public static SoundScript StartSound(SoundsListScriptableObject soundList, Transform parentTransform, out SoundScriptableObject soundScriptableObject, bool dontDestroyOnLoad = false)
    {
        return StartSound(soundList, parentTransform, Vector2.zero, out soundScriptableObject, dontDestroyOnLoad);
    }

    public static SoundScript StartSound(SoundsListScriptableObject soundList, Transform parentTransform, Vector2 localPosition, bool dontDestroyOnLoad = false)
    {
        return StartSound(soundList, parentTransform, localPosition, out SoundScriptableObject soundScriptableObject, dontDestroyOnLoad);
    }

    public static SoundScript StartSound(SoundsListScriptableObject soundList, Transform parentTransform, Vector2 localPosition, out SoundScriptableObject soundScriptableObject, bool dontDestroyOnLoad = false)
    {
        return StartSound(soundList.sounds, parentTransform, localPosition, out soundScriptableObject, dontDestroyOnLoad);
    }

    public static SoundScript StartSound(List<SoundScriptableObject> sounds, Transform parentTransform, bool dontDestroyOnLoad = false)
    {
        return StartSound(sounds, parentTransform, Vector2.zero, out SoundScriptableObject soundScriptableObject, dontDestroyOnLoad);
    }

    public static SoundScript StartSound(List<SoundScriptableObject> sounds, Transform parentTransform, Vector2 localPosition, out SoundScriptableObject soundScriptableObject, bool dontDestroyOnLoad = false)
    {
        soundScriptableObject = sounds[UnityEngine.Random.Range(0, sounds.Count)];
        if (soundScriptableObject == null)
            return null;
        return StartSound(soundScriptableObject, parentTransform, localPosition, 0f, dontDestroyOnLoad);
    }

    public static SoundScript StartSound(SoundScriptableObject soundScriptableObject, Transform parentTransform, bool dontDestroyOnLoad = false)
    {
        return StartSound(soundScriptableObject, parentTransform, Vector2.zero, 0f, dontDestroyOnLoad);
    }

    public static SoundScript StartSound(SoundScriptableObject soundScriptableObject, Transform parentTransform, Vector2 localPosition, float startTime = 0f, bool dontDestroyOnLoad = false)
    {
        SoundScript soundScript = SpawnSoundObject(parentTransform, localPosition, dontDestroyOnLoad);
        soundScript.soundScriptableObject = soundScriptableObject;
        soundScript.StartSound(startTime);
        soundScript.onSoundFinished += OnSoundFinished;
        _spawnedSounds.Add(soundScript);
        return soundScript;
    }

    private static SoundScript SpawnSoundObject(Transform parent, Vector3 localPosition, bool dontDestroyOnLoad)
    {
        GameObject soundObject = GameObject.Instantiate(soundObjectPrefab, parent);
        soundObject.transform.localPosition = localPosition;
        if (dontDestroyOnLoad)
            GameObject.DontDestroyOnLoad(soundObject);
        SoundScript soundScript = soundObject.GetComponent<SoundScript>();
        return soundScript;
    }

    private static void OnSoundFinished(SoundScript soundScript)
    {
        soundScript.onSoundFinished -= OnSoundFinished;
        GameObject.Destroy(soundScript.gameObject);
        if(_spawnedSounds.Contains(soundScript))
            _spawnedSounds.Remove(soundScript);
    }
 
    public static void PauseSounds()
    {
        foreach (SoundScript soundScript in _spawnedSounds)
        {
            soundScript.PauseSound();
        }
    }

    public static void ResumeSounds()
    {
        foreach (SoundScript soundScript in _spawnedSounds)
        {
            soundScript.ResumeSound();
        }
    }
}