using UnityEngine;

[CreateAssetMenu(fileName = "newSoundScriptableObject", menuName = "Sounds/Create new Sound Scriptable Object")]
public class SoundScriptableObject : ScriptableObject
{
    public AudioClip audioClip;
    [Range(0f, 1f), Tooltip("Default volume of the sound")] public float volume = 1f;
    [Range(0f, 3f), Tooltip("Default pitch of the sound")] public float pitch = 1f;
    public bool loop;
    public bool threeD = true;
    [Tooltip("Max value your volume can be multiplied by (cant be less then 0)")] public float maxVolumeRandomizer = 1f;
    [Tooltip("Max value your pitch can be multiplied by (cant be less then 0)")] public float maxPitchRandomizer = 1f;

    private void OnValidate()
    {
        if (maxPitchRandomizer < 1f)
            maxPitchRandomizer = 1f;
        if (maxVolumeRandomizer < 1f)
            maxVolumeRandomizer = 1f;
    }
}
