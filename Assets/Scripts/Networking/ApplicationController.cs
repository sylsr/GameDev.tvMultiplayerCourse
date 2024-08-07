using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField]
    private ClientSingleton clientPrefab;

    [SerializeField]
    private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;

    private ApplicationData appData;

    void Awake()
    {
        Debug.unityLogger.logHandler = new TimestampedLogHandler();
    }

    async void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            appData = new ApplicationData();
            ServerSingleton server = Instantiate(serverPrefab);
            await server.CreateServer();
            await server.GameManager.StartGameServerAsync();
        }
        else
        {
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool created = await clientSingleton.CreateClient();

            HostSingleton hostSingleton = Instantiate(hostPrefab);

            hostSingleton.CreateHost();

            if (created)
            {
                clientSingleton.GameManager.GoToMainMenu();
            }
        }
    }
}
