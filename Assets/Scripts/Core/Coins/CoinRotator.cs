using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    void Update()
    {
        this.transform.Rotate(0f, rotationSpeed * Time.deltaTime,0f );
    }
}
