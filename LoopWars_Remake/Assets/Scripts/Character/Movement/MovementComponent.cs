using Unity.Netcode;
using UnityEngine;

public class MovementComponent : NetworkBehaviour
{
    protected Character character;

    protected MovementManager movementManager { get
        {
            if (character == null || character.movementManager == null)
            {
                return GetComponent<MovementManager>();
            }
            return character.movementManager;
        }
    }
    protected Rigidbody2D rigidbody
    {
        get
        {
            if (character == null || character.movementManager == null)
            {
                return GetComponent<Rigidbody2D>();
            }
            return character.rigidbody;
        }
    }

    [HideInInspector] public bool subscribedOnEvents;

    protected virtual void Awake()
    {
        character = GetComponent<Character>();
    }
}
