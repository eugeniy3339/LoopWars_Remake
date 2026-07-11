using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsThereADamagable(collision.gameObject, out IDamagable damagable))
        {
            damagable.Damage(transform, transform, 100f);
        }
    }

    protected bool IsThereADamagable(GameObject gameObject, out IDamagable damagable)
    {
        damagable = gameObject.GetComponentInChildren<IDamagable>();
        return damagable != null;
    }
}
