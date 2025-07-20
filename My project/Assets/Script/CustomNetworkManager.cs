using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // �������� ����� ������ �� SpawnManager
        Transform startPos = SpawnManager.Instance.GetSpawnPoint();

        // ������� ������ � ������ ������� � ��������
        GameObject player = Instantiate(playerPrefab, startPos.position, startPos.rotation);

        // ��������� ������ � ����
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
