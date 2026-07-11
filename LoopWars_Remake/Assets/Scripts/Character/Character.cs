using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Character : NetworkBehaviour
{
    public Rigidbody2D rigidbody { get; protected set; }
    public CapsuleCollider2D collider { get; protected set; }
    public SpriteRenderer[] spriteRenderers { get; protected set; }
    public Animator animator { get; protected set;   }
    private PlayerInput _pI;
    public PlayerInput playerInput { get { if (_pI == null) { _pI = GetComponent<PlayerInput>(); } return _pI; } protected set { _pI = value; } }

    public MovementManager movementManager { get; protected set; }
    public Movement movement { get; protected set; }
    public Jump jump { get; protected set; }
    public Dash dash { get; protected set; }
    public WeaponManager weaponManager { get; protected set; }
    public HealthSystem healthSystem { get; protected set; }
    public ObjectColorsHandler objectColorsHandler { get; protected set; }

    public static event Action<Character> onCharacterSpawned;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CapsuleCollider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        movementManager = GetComponent<MovementManager>();
        movement = GetComponent<Movement>();
        jump = GetComponent<Jump>();
        dash = GetComponent<Dash>();
        weaponManager = GetComponent<WeaponManager>();
        healthSystem = GetComponent<HealthSystem>();
        objectColorsHandler = GetComponent<ObjectColorsHandler>();

        rigidbody.freezeRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        onCharacterSpawned?.Invoke(this);
    }

    public static Character FindCharacter(ulong networkObjectId)
    {
        foreach(var character in FindObjectsOfType<Character>())
        {
            if (character.NetworkObjectId == networkObjectId)
                return character;
        }

        return null;
    }

    public static Character FindCharacterByPlayerId(ulong playerId)
    {
        foreach (var character in FindObjectsOfType<Character>())
        {
            if (character.OwnerClientId == playerId)
                return character;
        }

        return null;
    }
}
