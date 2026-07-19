using System;
using UnityEngine;

public class AnimationEventsHandler : MonoBehaviour
{
    public event Action<string> onAnimationEvent;

    public void FireEvent(string evn)
    {
        onAnimationEvent?.Invoke(evn);
    }
}
