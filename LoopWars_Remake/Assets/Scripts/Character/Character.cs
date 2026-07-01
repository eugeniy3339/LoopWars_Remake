using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Character : NetworkBehaviour
{
    public Rigidbody2D rigidbody { get; protected set; }
    public SpriteRenderer spriteRenderer { get; protected set; }
    private PlayerInput _pI;
    public PlayerInput playerInput { get { if (_pI == null) { _pI = GetComponent<PlayerInput>(); } return _pI; } protected set { _pI = value; } }

    public Movement movement { get; protected set; }
    public Jump jump { get; protected set; }
    public Dash dash { get; protected set; }
    public WeaponManager weaponManager { get; protected set; }
    public HealthSystem healthSystem { get; protected set; }

    const float defaultGravityScale = 3f;


    private bool _ug;
    public bool useGravity { 
        get { return _ug; }
        set { 
            _ug = value; 
            rigidbody.gravityScale = value ? defaultGravityScale : 0f; 
        }
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();

        movement = GetComponent<Movement>();
        jump = GetComponent<Jump>();
        dash = GetComponent<Dash>();
        weaponManager = GetComponent<WeaponManager>();
        healthSystem = GetComponent<HealthSystem>();

        useGravity = true;
        rigidbody.freezeRotation = true;
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
}
