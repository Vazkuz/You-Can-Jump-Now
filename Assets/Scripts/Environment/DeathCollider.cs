using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCollider : MonoBehaviour
{
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerNetwork>().Death();
            return;
        }

        if (collision.CompareTag("Grabbable"))
        {
            collision.GetComponent<Grabbable>().Death();
            return;
        }
    }
}
