using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultMusicList", menuName = "Music/Create new Music List")]
public class MusicListScriptableObject : ScriptableObject
{
    public static MusicListScriptableObject Instance { get { return Resources.Load<MusicListScriptableObject>("DefaultMusicList"); } }
    public List<SoundMapPair> musicMapsPairs = new List<SoundMapPair>();

    public SoundMapPair GetRandomMusic()
    {
        return musicMapsPairs[UnityEngine.Random.Range(0, musicMapsPairs.Count)];
    }

    public MapsListScriptableObject GetMaps(SoundScriptableObject music)
    {
        SoundMapPair pair = musicMapsPairs.Find((p) => p.music == music);
        if (pair == null) return null;
        return pair.maps;
    }

    [Serializable]
    public class SoundMapPair
    {
        public SoundScriptableObject music;
        public MapsListScriptableObject maps;
    }
}
