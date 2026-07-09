using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CustomEventSystem : MonoBehaviour
{
    public static CustomEventSystem Instance { get; private set;     }

    private EventSystem eventSystem;
    public Dictionary<PlayerInput, CustomISelectable> curSelectableSelectables { get; private set; } = new Dictionary<PlayerInput, CustomISelectable>();

    private void Start()
    {
        Instance = this;
        eventSystem = EventSystem.current;
    }

    public void OnMove(InputAction.CallbackContext context, PlayerInput playerInput)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (!context.started) return;

        CustomISelectable selectableToSelect = GetNextISelectable(input, playerInput);

        if (selectableToSelect == null) return;

        if (!curSelectableSelectables.ContainsKey(playerInput))
            curSelectableSelectables.Add(playerInput, selectableToSelect);
        else
        {
            if (curSelectableSelectables[playerInput] == selectableToSelect)
                return;

            curSelectableSelectables[playerInput] = selectableToSelect;
        }

        selectableToSelect.Select();
    }

    public void OnInteract(InputAction.CallbackContext context, PlayerInput playerInput)
    {
        if (!context.canceled) return;
        if (!curSelectableSelectables.ContainsKey(playerInput)) return;
        if (!curSelectableSelectables[playerInput].CanInteract()) return;

        curSelectableSelectables[playerInput].Interact();
    }

    private CustomISelectable GetNextISelectable(Vector2 input, PlayerInput playerInput)
    {
        CustomISelectable curPlayersSelectable = null;
        if (curSelectableSelectables.ContainsKey(playerInput))
            curPlayersSelectable = curSelectableSelectables[playerInput];
        else
            curPlayersSelectable = GetFistISelectable(playerInput);

        if (curPlayersSelectable == null) return null;

        bool horizotalAxis = IsHorizotalAxisBigger(input);
        float value = BetterMath.NormalizedFloat(horizotalAxis ? input.x : input.y);

        CustomISelectable curSelectableToSelect = curPlayersSelectable;

        do
        {
            curSelectableToSelect = horizotalAxis ? GetNextISelectable(curSelectableToSelect, horizotalAxis, value) : null;
        }
        while (curSelectableToSelect != null && !curSelectableToSelect.CanBeSelected(playerInput));

        return curSelectableToSelect == null ? curPlayersSelectable : curSelectableToSelect;
    }

    private CustomISelectable GetNextISelectable(CustomISelectable curSelectable, bool horizontalAxis, float value)
    {
        if (curSelectable == null) return null;

        Transform curSelectableTransform = (curSelectable as MonoBehaviour).transform;

        foreach (var customISelectable in FindObjectsOfType<MonoBehaviour>().OfType<CustomISelectable>())
        {
            Transform customISelectableTransform = (customISelectable as MonoBehaviour).transform;
            if (customISelectableTransform == curSelectableTransform) continue;

            Vector2 directionToCustomISelectableTransform = customISelectableTransform.position - curSelectableTransform.position;
            if (BetterMath.NormalizedFloat(horizontalAxis ? directionToCustomISelectableTransform.x : directionToCustomISelectableTransform.y) == value)
                return customISelectable;
        }

        return null;
    }

    private CustomISelectable GetFistISelectable(PlayerInput playerInput)
    {
        foreach (var customISelectable in FindObjectsOfType<MonoBehaviour>().OfType<CustomISelectable>())
        {
            if (customISelectable.CanBeSelected(playerInput))
                return customISelectable;
        }

        return null;
    }

    private bool IsHorizotalAxisBigger(Vector2 input)
    {
        return input.x >= input.y;
    }
}