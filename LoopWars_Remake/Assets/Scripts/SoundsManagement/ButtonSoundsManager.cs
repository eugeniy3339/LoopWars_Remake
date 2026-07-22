using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSoundsManager : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    private Button button;

    private SoundScriptableObject selectedSound;
    private SoundScriptableObject clickedSound;

    private void Awake()
    {
        button = GetComponent<Button>();

        selectedSound = SoundsListScriptableObject.DefaultSoundsList.GetSound("ButtonSelect");
        clickedSound = SoundsListScriptableObject.DefaultSoundsList.GetSound("ButtonClick");

        button.onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(button.interactable)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (selectedSound == null) return;
            SoundsManager.StartSound(selectedSound, null, true);
    }

    private void OnClick()
    {
        if (clickedSound == null) return;
            SoundsManager.StartSound(clickedSound, null, true);
    }
}
