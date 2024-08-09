using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{
    [SerializeField] private Projectile projectile;

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (projectile.TeamNum != -1 && collision.attachedRigidbody != null &&  collision.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player))
        {
            if (player.TeamNumber.Value == projectile.TeamNum)
            {
                return;
            }
        }
        Destroy(this.gameObject);
    }
}
