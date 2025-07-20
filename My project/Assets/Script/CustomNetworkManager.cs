using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Получаем точку спавна от SpawnManager
        Transform startPos = SpawnManager.Instance.GetSpawnPoint();

        // Спавним игрока в нужной позиции и повороте
        GameObject player = Instantiate(playerPrefab, startPos.position, startPos.rotation);

        // Добавляем игрока в игру
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
