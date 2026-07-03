using Unity.Netcode;
using UnityEngine;

public class MovementComponent : NetworkBehaviour
{
    protected Character character;

    protected MovementManager movementManager;
    protected Rigidbody2D rigidbody;

    [HideInInspector] public bool subscribedOnEvents;

    protected virtual void Awake()
    {
        character = GetComponent<Character>();
    }

    protected virtual void Start()
    {
        movementManager = character.movementManager;
        rigidbody = character.rigidbody;
    }
}
