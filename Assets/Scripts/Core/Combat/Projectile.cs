using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int TeamNum { get; private set; }
    public void Initialize(int teamNum)
    {
        this.TeamNum = teamNum;
    }
}
