using Cinemachine;
using System.Collections;
using UnityEngine;

public static class CameraShakeManager
{
    public static bool screenShaking { get; private set; }
    private static Coroutine curScreenShakingCoroutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {

    }

    public static void StartScreenShake(float time = 1f, float amplitude = 1f, float frequency = 1f)
    {
        if (curScreenShakingCoroutine != null)
            MonoCommandsStarter.Instance.StopCoroutine(curScreenShakingCoroutine);

        CinemachineVirtualCamera curVirtualCamera = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        curScreenShakingCoroutine = MonoCommandsStarter.Instance.StartCoroutine(ScreenShakeCoro(curVirtualCamera, time, amplitude, frequency));
    }

    private static IEnumerator ScreenShakeCoro(CinemachineVirtualCamera virtualCamera, float time, float amplitude, float frequency)
    {
        if (virtualCamera == null) yield break;

        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (cinemachineBasicMultiChannelPerlin == null) yield break;

        float curTime = 0f;

        cinemachineBasicMultiChannelPerlin.m_FrequencyGain = frequency;

        while (curTime <= time)
        {
            if (virtualCamera == null) yield break;

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(amplitude, 0f, curTime/time);
            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
}
