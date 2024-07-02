using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUIHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_Text lobbyJoinCode;
    void Start()
    {
        if(HostSingleton.Instance != null)
        {
            lobbyJoinCode.text = HostSingleton.Instance.GameManager.joinCode;
        }
    }
}
