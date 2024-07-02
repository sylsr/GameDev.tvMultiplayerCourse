using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField]
    float destroyAfterSeconds = 5f;

    void Start()
    {
        Destroy(this.gameObject, destroyAfterSeconds);
    }
}
