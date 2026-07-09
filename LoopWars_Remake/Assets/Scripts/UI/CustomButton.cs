using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CustomButton : MonoBehaviour, CustomISelectable
{
    [SerializeField] private bool _specificPlayersButton;
    public bool specificPlayersButton { get { return _specificPlayersButton; } set { _specificPlayersButton = value; } }
    public PlayerInput playerInput { get; set; }

    [SerializeField] private bool _interactable;
    public bool interactable { get { return _interactable; } set { _interactable = value; } }

    [SerializeField] private UnityEvent interactEvents;

    public bool CanBeSelected(PlayerInput selecter)
    {
        return interactable && (!specificPlayersButton || playerInput == selecter);
    }

    public bool CanInteract()
    {
        return interactable;
    }

    public void Select()
    {
        Debug.Log("OnSelected");
    }

    public void Interact()
    {
        Debug.Log("OnInteracted");
        interactEvents?.Invoke();
    }
}
