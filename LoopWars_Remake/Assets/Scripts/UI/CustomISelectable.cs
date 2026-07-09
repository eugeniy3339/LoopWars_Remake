using UnityEngine;
using UnityEngine.InputSystem;

public interface CustomISelectable
{
    PlayerInput playerInput { get; set; }
    bool specificPlayersButton { get; set; }
    bool interactable { get; set; }

    bool CanBeSelected(PlayerInput selecter);
    bool CanInteract();
    void Select();
    void Interact();
}
