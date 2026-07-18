using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class FadeInFadeOutManager
{
    private static Coroutine curFadeInFadeOutCoro;

    private static Image curFadeInFadeOutImage;

    public static event Action onFadeIn;
    public static event Action onFadeOut;

    private static event Action onFadeInFadeOutCoroEnded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        GameObject fadeInFadeOutCanvas = GameObject.Instantiate(Resources.Load<GameObject>("FadeInFadeOutCanvas"));
        curFadeInFadeOutImage = fadeInFadeOutCanvas.GetComponentInChildren<Image>();
        GameObject.DontDestroyOnLoad(fadeInFadeOutCanvas);
        FadeOut(0f);
    }

    public static void FadeIn(float time = 1f)
    {
        curFadeInFadeOutImage.gameObject.SetActive(true);

        UnsubscribeFromEvents();
        StopCurFadeInFadeOutCoro();

        onFadeInFadeOutCoroEnded += OnFadeInCoroEnded;
        curFadeInFadeOutCoro = MonoCommandsStarter.Instance.StartCoroutine(FadeInFadeOutCoro(1f, time));
    }

    public static void FadeOut(float time = 1f)
    {
        UnsubscribeFromEvents();
        StopCurFadeInFadeOutCoro();

        onFadeInFadeOutCoroEnded += OnFadeOutCoroEnded;
        curFadeInFadeOutCoro = MonoCommandsStarter.Instance.StartCoroutine(FadeInFadeOutCoro(0f, time));
    }

    private static IEnumerator FadeInFadeOutCoro(float endAlpha, float time)
    {
        float startAlpha = curFadeInFadeOutImage.color.a;
        float curTime = 0f;

        do
        {
            curTime += Time.deltaTime;
            curFadeInFadeOutImage.color = new Color(curFadeInFadeOutImage.color.r, curFadeInFadeOutImage.color.g, curFadeInFadeOutImage.color.b, Mathf.Lerp(startAlpha, endAlpha, curTime/time));
            yield return new WaitForEndOfFrame();
        }
        while (curTime < time);

        onFadeInFadeOutCoroEnded?.Invoke();
    }

    private static void OnFadeInCoroEnded()
    {
        onFadeIn?.Invoke();
    }

    private static void OnFadeOutCoroEnded()
    {
        curFadeInFadeOutImage.gameObject.SetActive(false);

        onFadeOut?.Invoke();
    }

    private static void UnsubscribeFromEvents()
    {
        onFadeInFadeOutCoroEnded -= OnFadeInCoroEnded;
        onFadeInFadeOutCoroEnded -= OnFadeOutCoroEnded;
    }

    private static void StopCurFadeInFadeOutCoro()
    {
        if (curFadeInFadeOutCoro != null)
            MonoCommandsStarter.Instance.StopCoroutine(curFadeInFadeOutCoro);
    }
}
