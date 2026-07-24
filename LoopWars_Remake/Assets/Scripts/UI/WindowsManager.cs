using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WindowsManager : MonoBehaviour
{
    [SerializeField] private List<Panel> panels = new List<Panel>();
    protected Panel curOpenedPanel;

    public static WindowsManager Instance { get; protected set; }

    protected virtual void Awake()
    {
        Instance = this;
    }

    protected virtual void Start()
    {
        OpenPanel(0);
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    protected void CloseAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel.panel != null)
                panel.panel.SetActive(false);
        }
    }

    public void OpenPanel(GameObject gameObject)
    {
        Panel panel = GetPanel(gameObject);
        OpenPanel(panel);
    }

    protected void OpenPanel(int index)
    {
        OpenPanel(panels[index]);
    }

    protected void OpenPanel(Panel panel)
    {
        if (panel == null) return;
        CloseAllPanels();

        if(panel.panel != null)
            panel.panel.SetActive(true);
        SetSelectedObject(panel.firstObjectToSelect);
        curOpenedPanel = panel;
    }

    protected Panel GetPanel(GameObject panel)
    {
        return panels.Find((x) => x.panel == panel);
    }

    public void SetSelectedObject(GameObject selected)
    {
        EventSystem.current.SetSelectedGameObject(selected);
    }

    public void Quit()
    {
        Application.Quit();
    }

    [Serializable]
    public class Panel
    {
        public GameObject panel;
        public GameObject firstObjectToSelect;
    }
}
