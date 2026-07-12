using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundScript : MonoBehaviour
{
    private AudioSource _as;
    public AudioSource audioSource
    {
        get
        {
            if(_as == null)
            {
                _as = GetComponent<AudioSource>();
            }

            return _as;
        }
    }

    private SoundScriptableObject _soundScriptableObject;
    public SoundScriptableObject soundScriptableObject
    {
        get
        {
            return _soundScriptableObject;
        }
        set
        {
            _soundScriptableObject = value;
            audioSource.clip = _soundScriptableObject.audioClip;
        }
    }

    private Coroutine _lifetimeCoroutine;
    private Coroutine _changeVolumeCoroutine;
    public event Action<SoundScript> onSoundFinished;
    public event Action<SoundScript> onSoundVolumeChanged;

    private bool _paused = false;
    
    private float _globalVolume;

    public void StartSound(float startTime)
    {
        _globalVolume = SoundsManager.globalVolume;
        float volume = _soundScriptableObject.volume * UnityEngine.Random.Range(1f, _soundScriptableObject.maxVolumeRandomizer) * _globalVolume;
        if (startTime <= 0f)
        {
            audioSource.volume = volume;
        }
        else
        {
            audioSource.volume = 0f;
            ChangeSoundVolume(volume, startTime);
        }
        audioSource.pitch = _soundScriptableObject.pitch * UnityEngine.Random.Range(1f, _soundScriptableObject.maxPitchRandomizer);
        audioSource.spatialBlend = _soundScriptableObject.threeD ? 1f : 0f;
        audioSource.loop = _soundScriptableObject.loop;
        audioSource.Play();

        if(_lifetimeCoroutine != null)
            StopCoroutine(_lifetimeCoroutine);

        if(!_soundScriptableObject.loop)
        {
            _lifetimeCoroutine = StartCoroutine(SoundLifeCoro(_soundScriptableObject.audioClip.length));
        }
    }

    private IEnumerator SoundLifeCoro(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        if (onSoundFinished != null)
            onSoundFinished(this);
    }

    public void ChangeSoundVolume(float finalVolume, float time = 0f)
    {
        if (_changeVolumeCoroutine != null)
            StopCoroutine(_changeVolumeCoroutine);
        _changeVolumeCoroutine = StartCoroutine(ChangeSoundVolumeCoro(finalVolume, time));
    }

    private IEnumerator ChangeSoundVolumeCoro(float finalVolume, float time)
    {
        float curTime = 0f;
        float startVolume = audioSource.volume;
        while (curTime < time)
        {
            curTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, finalVolume * _globalVolume, curTime / time);
            yield return new WaitForSeconds(0f);
        }
        audioSource.volume = finalVolume * _globalVolume;

        if (onSoundVolumeChanged != null)
            onSoundVolumeChanged(this);

        _changeVolumeCoroutine = null;
    }

    public void StopSound(float stopTime = 0f)
    {
        if (_lifetimeCoroutine != null)
            StopCoroutine(_lifetimeCoroutine);

        StartCoroutine(StopSoundCoro(stopTime));
    }

    public void PauseSound()
    {
        _paused = true;
        audioSource.Pause();
    }

    public void ResumeSound()
    {
        _paused = false;
        audioSource.UnPause();
    }

    private IEnumerator StopSoundCoro(float stopTime)
    {
        ChangeSoundVolume(0f, stopTime);
        float curTime = 0f;

        while (curTime < stopTime)
        {
            if (_paused) {
                yield return new WaitForEndOfFrame();
                continue;
            }

            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (onSoundFinished != null)
            onSoundFinished(this);
    }

    private void OnEnable()
    {
        SoundsManager.onGlobalVolumeChanged += OnGlobalVolumeChanged;
    }

    private void OnDestroy()
    {
        SoundsManager.onGlobalVolumeChanged -= OnGlobalVolumeChanged;
        if (onSoundFinished != null)
            onSoundFinished(this);
    }

    private void OnGlobalVolumeChanged(float globalVolume)
    {
        if(_changeVolumeCoroutine == null)
            audioSource.volume = audioSource.volume / _globalVolume * globalVolume;
        _globalVolume = globalVolume;
    }
}
