using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField]
    private Transform turretTransform;

    [SerializeField]
    private InputReader inputReader;

    private void LateUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        Vector2 aimPosition = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        turretTransform.up = new Vector2(aimPosition.x - turretTransform.position.x, aimPosition.y - turretTransform.position.y);
    }
}
