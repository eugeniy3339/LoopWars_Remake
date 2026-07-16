using UnityEngine;

public class GameWindowsManager : WindowsManager
{
    [SerializeField] private GameObject pausePanel;

    public void Leave()
    {
        GameManager.Instance.Leave();
    }

    public void Unpause()
    {
        PauseHandler.Instance.Unpause();
    }

    private void OnPause()
    {
        OpenPanel(pausePanel);
    }

    private void OnUnpause()
    {
        CloseAllPanels();
    }

    private void OnEnable()
    {
        PauseHandler.onPause += OnPause;
        PauseHandler.onUnpause += OnUnpause;
    }

    private void OnDisable()
    {
        PauseHandler.onPause -= OnPause;
        PauseHandler.onUnpause -= OnUnpause;
    }
}
