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
        if (!NetworkClient.ready)
            NetworkClient.Ready();

        if (!NetworkClient.localPlayer)
        {
            Debug.Log("👤 Requesting AddPlayer from client...");
            NetworkClient.AddPlayer();
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        Debug.Log($"🌀 Server scene changed to: {sceneName}");

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null) // Если игрок еще не создан
            {
                Transform spawn = SpawnManager.Instance?.GetSpawnPoint();
                if (spawn == null)
                {
                    Debug.LogError("❌ Spawn point is NULL!");
                    continue;
                }

                GameObject player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
                NetworkServer.AddPlayerForConnection(conn, player);

                Debug.Log($"✅ Игрок заспавнен на {spawn.position} в сцене {sceneName} для conn {conn.connectionId}");
            }
            else
            {
                Debug.Log($"ℹ️ Игрок уже существует для conn {conn.connectionId}");
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
