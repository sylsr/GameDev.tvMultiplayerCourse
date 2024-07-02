using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField]
    private int damage = 25;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.attachedRigidbody != null)
        {
            if(collision.attachedRigidbody.TryGetComponent<Health>(out Health health)){
                health.TakeDamage(damage);
            }
        }
    }
}
