using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [SerializeField]
    private ClientSingleton clientPrefab;

    [SerializeField]
    private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;

    private ApplicationData appData;

    private const string GameSceneName = "GameplayScene1";

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
            Application.targetFrameRate = 60;
            appData = new ApplicationData();
            ServerSingleton server = Instantiate(serverPrefab);
            StartCoroutine(LoadGameSceneAsync(server));
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

    private IEnumerator LoadGameSceneAsync(ServerSingleton server)
    {
        AsyncOperation sc = SceneManager.LoadSceneAsync(GameSceneName);

        while (!sc.isDone)
        {
            yield return null;
        }

        Task createServerTask = server.CreateServer();
        yield return new WaitUntil(() => createServerTask.IsCompleted);
        Task startServerTask = server.GameManager.StartGameServerAsync();

        yield return new WaitUntil(() => startServerTask.IsCompleted);
    }
}
