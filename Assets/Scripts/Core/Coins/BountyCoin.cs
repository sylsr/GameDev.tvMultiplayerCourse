using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BountyCoin : Coin
{
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
        Destroy(this.gameObject);
        return coinValue;
    }

}
