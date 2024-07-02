using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;

    private Vector3 previousPosition;

    private void Start()
    {
        if (IsServer)
        {
            return;
        }
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (IsServer)
        {
            return;
        }
        if(previousPosition != this.transform.position)
        {
            this.Show(true);
            previousPosition = this.transform.position;
        }
    }

    public override int Collect()
    {
        if (!IsServer)
        {
            this.Show(false);
            return 0;
        }

        if (collected)
        {
            return 0;
        }

        collected = true;
        OnCollected?.Invoke(this);
        return coinValue;
    }

    public void Reset(Vector2 newPosition)
    {
        this.transform.position = newPosition;
        collected = false;
    }
}
