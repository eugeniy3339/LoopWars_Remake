using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseHandler : MonoBehaviour
{
    public static PauseHandler Instance { get; private set; }

    private PauseInputs pauseInputs;
    private bool paused;

    public static event Action onPause;
    public static event Action onUnpause;

    private void Awake()
    {
        Instance = this;
        pauseInputs = new PauseInputs();
    }

    private void PauseUnpause(InputAction.CallbackContext cotnext)
    {
        if (cotnext.started)
            PauseUnpause();
    }

    private void PauseUnpause()
    {
        if (paused)
            Unpause();
        else
            Pause();
    }

    public void Pause()
    {
        paused = true;
        onPause?.Invoke();
    }

    public void Unpause()
    {
        paused = false;
        onUnpause?.Invoke();
    }

    private void OnEnable()
    {
        pauseInputs.Enable();
        pauseInputs.Default.OpenClose.started += PauseUnpause;
    }

    private void OnDisable()
    {
        pauseInputs.Default.OpenClose.started -= PauseUnpause;
        pauseInputs.Disable();
    }
}
