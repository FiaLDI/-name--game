using UnityEngine;
using Mirror;

public class LevelLoader : MonoBehaviour
{
    void Start()
    {
        Debug.Log("LevelLoader script started");
    }

    public void OnClick()
    {
        Debug.Log("Button clicked!");
    }

    // Вызывается из кнопок, передаём имя уровня
    public void OnStartLevelButtonPressed(string levelName)
    {
        Debug.Log($"Button pressed, trying to change scene to {levelName}...");
        if (NetworkServer.active)
        {
            Debug.Log($"Server active, changing scene to {levelName}");
            NetworkManager.singleton.ServerChangeScene(levelName);
        }
        else
        {
            Debug.LogWarning("Not server, can't change scene!");
        }
    }
}
