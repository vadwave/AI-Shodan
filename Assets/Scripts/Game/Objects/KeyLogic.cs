using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLogic : MonoBehaviour, ITakeable
{
    public void Take(Collider2D collision)
    {
        if (!DetectCharacter(collision)) return;
        AddItem(collision);
        Destroy(this.gameObject);
    }

    bool DetectCharacter(Collider2D collision)
    {
        IMovable character = collision.transform.parent.GetComponent<IMovable>();
        if (character != null) return true;
        else return false;
    }
    void AddItem(Collider2D collision)
    {
            IPocket pocket = collision.transform.parent.GetComponent<IPocket>();
            pocket.AddKey();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Take(collision);
    }
}
