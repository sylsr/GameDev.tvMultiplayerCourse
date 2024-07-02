using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;

    public HostGameManager GameManager { get; private set; }

    public static HostSingleton Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<HostSingleton>();

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
        GameManager = new HostGameManager();
    }
}
