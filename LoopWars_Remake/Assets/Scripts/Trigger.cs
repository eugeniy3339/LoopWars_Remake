using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trigger : MonoBehaviour
{
    public event Action<Character, Trigger> onTriggerEnter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsThereCharacter(collision.gameObject, out Character character))
        {
            OnTrigger(character);
        }
    }

    private bool IsThereCharacter(GameObject gameObject, out Character character)
    {
        return character = gameObject.GetComponentInChildren<Character>();
    }

    protected virtual void OnTrigger(Character character)
    {
        onTriggerEnter?.Invoke(character, this);
    }
}
