using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField]
    private int damage = 25;

    [SerializeField] private Projectile projectile;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null)
        {
            return;
        }

        if (projectile.TeamNum != -1 && collision.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player))
        {
            if (player.TeamNumber.Value == projectile.TeamNum)
            {
                return;
            }
        }

        if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage);
        }

    }
}
