using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Character : MonoBehaviour
{
    public Rigidbody2D rigidbody { get; protected set; }
    public SpriteRenderer spriteRenderer { get; protected set; }

    public Movement movement { get; protected set; }
    public Jump jump { get; protected set; }
    public Dash dash { get; protected set; }


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

        movement = GetComponent<Movement>();
        jump = GetComponent<Jump>();
        dash = GetComponent<Dash>();

        useGravity = true;
        rigidbody.freezeRotation = true;
    }
}
