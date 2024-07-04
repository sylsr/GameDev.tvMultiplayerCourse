using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{
    private static ServerSingleton instance;

    public ServerGameManager GameManager { get; private set; }

    public static ServerSingleton Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<ServerSingleton>();

            if (instance == null)
            {

                return null;
            }

            return instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }

    public void CreateHost()
    {
        Start();
        GameManager = new ServerGameManager();
    }
}
