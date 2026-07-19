using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultMusicMapsPairsList", menuName = "Maps/Create new Music Maps Pairs List")]
public class MusicMapsPairsListScriptableObject : ScriptableObject
{
    public static MusicMapsPairsListScriptableObject Instance { get { return Resources.Load<MusicMapsPairsListScriptableObject>("DefaultMusicMapsPairsList"); } }
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

    public int GetIndex(SoundScriptableObject soundScriptableObject)
    {
        try
        {
            return musicMapsPairs.IndexOf(musicMapsPairs.Find((x) => x.music == soundScriptableObject));
        }
        catch
        {
            return 0;
        }
    }

    [Serializable]
    public class SoundMapPair
    {
        public SoundScriptableObject music;
        public MapsListScriptableObject maps;
    }
}
