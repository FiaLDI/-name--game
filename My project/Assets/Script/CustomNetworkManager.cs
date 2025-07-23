using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [Header("Scenes")]
    public string hubSceneName = "HUB";

    public override void Awake()
    {
        base.Awake();
        autoCreatePlayer = false;
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("Host started, switching to hub scene...");
        ServerChangeScene(hubSceneName);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Client connected to server");
        // Клиент переключится автоматически при смене сцены сервером
        NetworkClient.AddPlayer();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        Debug.Log($"Server scene changed to {sceneName}");

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null)
            {
                Transform startPos = SpawnManager.Instance.GetSpawnPoint();
                GameObject player = Instantiate(playerPrefab, startPos.position, startPos.rotation);
                NetworkServer.AddPlayerForConnection(conn, player);
            }
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            Debug.LogWarning("Player already exists for this connection");
            return;
        }

        // Доп. проверка: есть ли точки спавна
        if (SpawnManager.Instance == null)
        {
            Debug.LogError("SpawnManager not found in scene!");
            return;
        }

        Transform startPos = SpawnManager.Instance.GetSpawnPoint();
        if (startPos == null)
        {
            Debug.LogWarning("No spawn point found! Spawning at (0,0,0)");
            startPos = new GameObject("TempSpawn").transform;
        }

        GameObject player = Instantiate(playerPrefab, startPos.position, startPos.rotation);

        // Только здесь вызываем
        NetworkServer.AddPlayerForConnection(conn, player);
    }



}
