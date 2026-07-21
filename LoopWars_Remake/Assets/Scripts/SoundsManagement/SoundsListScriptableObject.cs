using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newSoundsListScriptableObject", menuName = "Sounds/Create new Sounds List")]
public class SoundsListScriptableObject : ScriptableObject
{
    public static SoundsListScriptableObject DefaultSoundsList { get { return Resources.Load<SoundsListScriptableObject>("DefaultSoundsListScriptableObject"); } }
    public List<SoundScriptableObject> sounds = new List<SoundScriptableObject>();

    public SoundScriptableObject GetSound(string name)
    {
        return sounds.Find((sound) => sound.name == name);
    }

    public List<SoundScriptableObject> GetSounds(string name)
    {
        List<SoundScriptableObject> sounds = new List<SoundScriptableObject>();
        foreach(var sound in this.sounds)
        {
            if(sound.name == name)
            {
                sounds.Add(sound);
            }
        }

        return sounds;
    }
}